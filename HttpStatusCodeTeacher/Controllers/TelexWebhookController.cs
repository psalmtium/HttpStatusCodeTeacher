using Microsoft.AspNetCore.Mvc;
using HttpStatusCodeTeacher.Models;
using HttpStatusCodeTeacher.Services;

namespace HttpStatusCodeTeacher.Controllers;

/// <summary>
/// Controller for Telex A2A webhook endpoints
/// </summary>
[ApiController]
[Route("api/v1")]
[Tags("Telex Webhook")]
public class TelexWebhookController(
    AiServiceFactory aiServiceFactory,
    ILogger<TelexWebhookController> logger,
    IConfiguration configuration)
    : ControllerBase
{
    private readonly IAiService _aiService = aiServiceFactory.GetAiService();
    private const string A2AWebhookPath = "a2a/status-code-teacher";

    /// <summary>
    /// Serve the Agent-to-Agent (A2A) protocol agent definition.
    /// </summary>
    [HttpGet(".well-known/agent.json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetAgentCard()
    {
        var agentId = configuration["Agent:Id"] ?? "http-status-code-teacher";
        var agentName = configuration["Agent:Name"] ?? "HTTP Status Code Teacher";
        var agentDescription = configuration["Agent:Description"] ?? "AI-powered educational agent that teaches HTTP status codes.";
        var agentDomain = configuration["Agent:Domain"] ?? "https://localhost:5001";
        var agentCategory = configuration["Agent:Category"] ?? "education";

        logger.LogInformation("Serving Agent Card for ID: {AgentId}", agentId);

        var webhookUrl = $"{agentDomain}/api/v1/{A2AWebhookPath}";

        var agentCard = new AgentCard
        {
            Active = true,
            Category = agentCategory,
            Description = agentDescription,
            Id = agentId,
            LongDescription = "You are a helpful HTTP status code teacher that provides accurate information about HTTP status codes. Your primary function is to help users understand what different HTTP status codes mean, when to use them, and best practices for implementing them in web applications.",
            Name = agentName,
            Nodes =
            [
                new AgentNode
                {
                    Id = "status_code_teacher",
                    Name = "HTTP Status Code Teacher",
                    Parameters = new Dictionary<string, object>(),
                    Position = [816, -112],
                    Type = "a2a/mastra-a2a-node",
                    TypeVersion = 1,
                    Url = webhookUrl
                }
            ],
            PinData = new Dictionary<string, object>(),
            Settings = new AgentSettings
            {
                ExecutionOrder = "v1"
            },
            ShortDescription = "Learn about HTTP status codes"
        };

        return Ok(agentCard);
    }

    /// <summary>
    /// Handle A2A JSON-RPC 'message/send' requests and return HTTP status code explanations.
    /// </summary>
    [HttpPost(A2AWebhookPath)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> TelexWebhook([FromBody] A2ARequest request)
    {
        try
        {
            // Validate JSON-RPC version
            if (request.JsonRpc != "2.0")
            {
                return Ok(new A2AErrorResponse
                {
                    JsonRpc = "2.0",
                    Id = request.Id,
                    Error = new A2AError
                    {
                        Code = -32600,
                        Message = "Invalid Request: JSON-RPC version must be 2.0"
                    }
                });
            }

            // Validate method
            if (request.Method != "message/send")
            {
                return Ok(new A2AErrorResponse
                {
                    JsonRpc = "2.0",
                    Id = request.Id,
                    Error = new A2AError
                    {
                        Code = -32601,
                        Message = $"Method not found: {request.Method}"
                    }
                });
            }

            // Extract all text parts from the message (including nested data)
            var allTextParts = ExtractAllTextParts(request.Params?.Message?.Parts);

            if (allTextParts.Count == 0)
            {
                return Ok(new A2AErrorResponse
                {
                    JsonRpc = "2.0",
                    Id = request.Id,
                    Error = new A2AError
                    {
                        Code = -32602,
                        Message = "Invalid params: No text content found in message"
                    }
                });
            }

            // Get the last text item (usually what should be responded to)
            var messageText = allTextParts.Last();

            logger.LogInformation("Received A2A message from {Role}: {Message}",
                request.Params?.Message?.Role ?? "unknown",
                messageText);

            // Try to extract status code from message
            // Look for patterns like "200", "explain 404", "what is 500", etc.
            var statusCode = ExtractStatusCodeFromMessage(messageText);

            string responseText;

            if (statusCode.HasValue)
            {
                logger.LogInformation("Extracted status code {StatusCode} from message", statusCode.Value);

                // Get AI-generated explanation
                try
                {
                    var explanation = await _aiService.ExplainStatusCodeAsync(statusCode.Value);
                    responseText = FormatExplanation(statusCode.Value, explanation);
                }
                catch (Exception aiErr)
                {
                    logger.LogWarning(aiErr, "AI service error: {Message}", aiErr.Message);
                    responseText = $"HTTP {statusCode.Value} - {GetCategory(statusCode.Value)}";
                }
            }
            else
            {
                // No status code found, provide general help
                responseText = "I'm the HTTP Status Code Teacher! Ask me about any HTTP status code (100-599). " +
                              "For example, you can ask 'What is 404?' or 'Explain 200' or just send a status code like '500'.";
            }

            // Use the messageId from the request
            var messageId = request.Params?.Message?.MessageId ?? GenerateMessageId();

            // Return success response
            var response = new A2ASuccessResponse
            {
                JsonRpc = "2.0",
                Id = request.Id,
                Result = new A2AResult
                {
                    Role = "agent",
                    Parts =
                    [
                        new MessagePart
                        {
                            Kind = "text",
                            Type = "text",
                            Text = responseText
                        }
                    ],
                    Task = new TaskInfo
                    {
                        Id = messageId,
                        Status = "completed"
                    },
                    Message = new MessageInfo
                    {
                        MessageId = messageId
                    },
                    Kind = "message",
                    MessageId = messageId
                }
            };

            // Log the response for debugging
            logger.LogInformation("Sending A2A response: {@Response}", response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A2A Webhook Internal Error: {Message}", ex.Message);

            // Return JSON-RPC error response
            return Ok(new A2AErrorResponse
            {
                JsonRpc = "2.0",
                Id = request.Id,
                Error = new A2AError
                {
                    Code = -32603,
                    Message = "Internal error"
                }
            });
        }
    }

    /// <summary>
    /// Extract all text content from message parts (including nested data)
    /// </summary>
    private static List<string> ExtractAllTextParts(List<MessagePart>? parts)
    {
        var textParts = new List<string>();

        if (parts == null || parts.Count == 0)
            return textParts;

        foreach (var part in parts)
        {
            // Check if this part has direct text content
            if (!string.IsNullOrWhiteSpace(part.Text))
            {
                // Strip HTML tags for cleaner text
                var cleanText = System.Text.RegularExpressions.Regex.Replace(part.Text, "<.*?>", string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(cleanText))
                {
                    textParts.Add(cleanText);
                }
            }

            // Check if this part has nested data
            if (part.Data != null && part.Data.Count > 0)
            {
                // Recursively extract text from nested data
                var nestedTexts = ExtractAllTextParts(part.Data);
                textParts.AddRange(nestedTexts);
            }
        }

        return textParts;
    }

    /// <summary>
    /// Extract HTTP status code from user message
    /// </summary>
    private static int? ExtractStatusCodeFromMessage(string message)
    {
        // Look for 3-digit numbers between 100-599
        var matches = System.Text.RegularExpressions.Regex.Matches(message, @"\b([1-5]\d{2})\b");

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (int.TryParse(match.Value, out var code) && code >= 100 && code <= 599)
            {
                return code;
            }
        }

        return null;
    }

    /// <summary>
    /// Format status code explanation into readable text
    /// </summary>
    private static string FormatExplanation(int code, StatusCodeExplanation explanation)
    {
        return $"**HTTP {code} - {explanation.Name}**\n\n" +
               $"**Category:** {explanation.Category}\n\n" +
               $"**Description:** {explanation.Description}\n\n" +
               $"**When to Use:** {explanation.WhenToUse}\n\n" +
               $"**Common Scenarios:** {explanation.CommonScenarios}\n\n" +
               $"**Best Practices:** {explanation.BestPractices}\n\n" +
               $"**Example:** {explanation.ExampleResponse}\n\n" +
               $"**Related Codes:** {explanation.RelatedCodes}";
    }

    /// <summary>
    /// Generate a unique message ID
    /// </summary>
    private static string GenerateMessageId()
    {
        // Generate a random ID similar to the example: "fbuvdhb4ke6vq8hbva9qh9ha"
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 24)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string GetCategory(int code)
    {
        return code switch
        {
            >= 100 and < 200 => "1xx Informational",
            >= 200 and < 300 => "2xx Success",
            >= 300 and < 400 => "3xx Redirection",
            >= 400 and < 500 => "4xx Client Error",
            >= 500 and < 600 => "5xx Server Error",
            _ => "Unknown"
        };
    }
}
