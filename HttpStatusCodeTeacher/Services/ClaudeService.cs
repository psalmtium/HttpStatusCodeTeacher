using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Newtonsoft.Json;
using HttpStatusCodeTeacher.Models;

namespace HttpStatusCodeTeacher.Services;

/// <summary>
/// Service for interacting with Claude AI API to teach HTTP status codes
/// </summary>
public class ClaudeService : IAiService
{
    private readonly AnthropicClient _client;
    private readonly ILogger<ClaudeService> _logger;
    private readonly string? _apiKey;
    public ClaudeService(IConfiguration configuration, ILogger<ClaudeService> logger)
    {
        _logger = logger;
        _apiKey = configuration["Claude:ApiKey"];

        _client = !string.IsNullOrEmpty(_apiKey) ? new AnthropicClient(_apiKey) : null!;
    }

    public async Task<StatusCodeExplanation> ExplainStatusCodeAsync(int statusCode)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("API Key not found. Cannot call Claude Service.");
            return GetFallbackExplanation(statusCode);
        }

        const string systemPrompt = """
                                    You are an expert teacher on HTTP status codes and web development. Your goal is to provide clear,
                                    educational, and comprehensive explanations about HTTP status codes. The response MUST be a valid JSON object that strictly adheres
                                    to the provided schema. Each generated string must be professional, educational, and informative. Focus on practical examples
                                    and real-world scenarios that developers encounter.

                                    The JSON must have exactly these fields:
                                    - code: The HTTP status code number (integer)
                                    - name: The official name of the status code (e.g., 'Not Found', 'OK')
                                    - category: The category (1xx Informational, 2xx Success, 3xx Redirection, 4xx Client Error, 5xx Server Error)
                                    - description: A clear, detailed explanation of what this status code means
                                    - when_to_use: Specific situations when a server should return this status code
                                    - common_scenarios: Real-world examples and common use cases where this code appears
                                    - best_practices: Guidelines for properly using and handling this status code
                                    - example_response: A sample HTTP response showing headers and body for this status code
                                    - related_codes: Other related HTTP status codes that developers should know about

                                    Return ONLY the JSON object, no additional text or markdown formatting.
                                    """;

        var userQuery = $"Provide a comprehensive educational explanation for HTTP status code {statusCode}";

        _logger.LogInformation("Fetching explanation from Claude for status code: {StatusCode}", statusCode);

        const int maxRetries = 3;
        var delay = 1000;

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var messages = new List<Message>
                {
                    new(RoleType.User, userQuery)
                };

                var parameters = new MessageParameters
                {
                    Messages = messages,
                    Model = AnthropicModels.Claude45Sonnet,
                    Stream = false,
                    Temperature = 0.7m,
                    MaxTokens = 4096,
                    System = [new SystemMessage(systemPrompt)]
                };

                var response = await _client.Messages.GetClaudeMessageAsync(parameters);

                if (response?.Content != null && response.Content.Count != 0)
                {
                    var textContent = response.Content.FirstOrDefault() as TextContent;
                    if (textContent?.Text == null)
                    {
                        return GetFallbackExplanation(statusCode);
                    }

                    var jsonText = textContent.Text;

                    // Remove Markdown code blocks if present
                    jsonText = jsonText.Trim();
                    if (jsonText.StartsWith("```json"))
                    {
                        jsonText = jsonText[7..];
                    }
                    if (jsonText.StartsWith("```"))
                    {
                        jsonText = jsonText[3..];
                    }
                    if (jsonText.EndsWith("```"))
                    {
                        jsonText = jsonText[..^3];
                    }
                    jsonText = jsonText.Trim();

                    var explanation = JsonConvert.DeserializeObject<StatusCodeExplanation>(jsonText);

                    if (explanation != null)
                    {
                        return explanation;
                    }
                }

                return GetFallbackExplanation(statusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on attempt {Attempt}: {Message}", attempt + 1, ex.Message);

                if (attempt < maxRetries - 1)
                {
                    _logger.LogInformation("Retrying in {Delay}ms...", delay);
                    await Task.Delay(delay);
                    delay *= 2;
                }
                else
                {
                    _logger.LogError("Failed to get explanation from Claude after {MaxRetries} attempts", maxRetries);
                    return GetFallbackExplanation(statusCode);
                }
            }
        }

        return GetFallbackExplanation(statusCode);
    }

    private static StatusCodeExplanation GetFallbackExplanation(int statusCode)
    {
        return new StatusCodeExplanation
        {
            Code = statusCode,
            Name = "Unknown",
            Category = "Unknown",
            Description = $"Explanation for HTTP status code {statusCode} is unavailable (API error or key missing).",
            WhenToUse = "API unavailable",
            CommonScenarios = "API unavailable",
            BestPractices = "API unavailable",
            ExampleResponse = "API unavailable",
            RelatedCodes = "API unavailable"
        };
    }
}
