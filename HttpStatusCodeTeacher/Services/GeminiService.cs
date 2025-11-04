using System.Text;
using System.Text.Json;
using HttpStatusCodeTeacher.Models;

namespace HttpStatusCodeTeacher.Services;

/// <summary>
/// Service for interacting with Google Gemini API to teach HTTP status codes
/// </summary>
public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiService> _logger;
    private readonly string? _apiKey;
    private const string GeminiModel = "gemini-2.5-flash-preview-09-2025";

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"];
    }

    public async Task<StatusCodeExplanation> ExplainStatusCodeAsync(int statusCode)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("API Key not found. Cannot call Gemini Service.");
            return GetFallbackExplanation(statusCode);
        }

        var systemPrompt = @"You are an expert teacher on HTTP status codes and web development. Your goal is to provide clear,
educational, and comprehensive explanations about HTTP status codes. The response MUST be a JSON object that strictly adheres
to the provided schema. Each generated string must be professional, educational, and informative. Focus on practical examples
and real-world scenarios that developers encounter.";

        var userQuery = $"Provide a comprehensive educational explanation for HTTP status code {statusCode}";

        var responseSchema = new
        {
            type = "OBJECT",
            properties = new Dictionary<string, object>
            {
                ["code"] = new { type = "INTEGER", description = "The HTTP status code number." },
                ["name"] = new { type = "STRING", description = "The official name of the status code (e.g., 'Not Found', 'OK')." },
                ["category"] = new { type = "STRING", description = "The category (1xx Informational, 2xx Success, 3xx Redirection, 4xx Client Error, 5xx Server Error)." },
                ["description"] = new { type = "STRING", description = "A clear, detailed explanation of what this status code means." },
                ["when_to_use"] = new { type = "STRING", description = "Specific situations when a server should return this status code." },
                ["common_scenarios"] = new { type = "STRING", description = "Real-world examples and common use cases where this code appears." },
                ["best_practices"] = new { type = "STRING", description = "Guidelines for properly using and handling this status code." },
                ["example_response"] = new { type = "STRING", description = "A sample HTTP response showing headers and body for this status code." },
                ["related_codes"] = new { type = "STRING", description = "Other related HTTP status codes that developers should know about." }
            },
            required = new[]
            {
                "code", "name", "category", "description", "when_to_use",
                "common_scenarios", "best_practices", "example_response", "related_codes"
            }
        };

        var payload = new
        {
            contents = new[] { new { parts = new[] { new { text = userQuery } } } },
            systemInstruction = new { parts = new[] { new { text = systemPrompt } } },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema
            }
        };

        var maxRetries = 3;
        var delay = 1000;

        _logger.LogInformation("Fetching explanation from Gemini for status code: {StatusCode}", statusCode);

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={_apiKey}";
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                var jsonText = result
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                var explanation = JsonSerializer.Deserialize<StatusCodeExplanation>(jsonText ?? "{}");
                return explanation ?? GetFallbackExplanation(statusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error on attempt {Attempt}: {StatusCode}", attempt + 1, ex.StatusCode);

                if ((ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                     ex.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                     ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) &&
                    attempt < maxRetries - 1)
                {
                    _logger.LogInformation("Retrying in {Delay}ms...", delay);
                    await Task.Delay(delay);
                    delay *= 2;
                }
                else
                {
                    throw new Exception($"Failed to get explanation after {maxRetries} attempts", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during API call");
                throw;
            }
        }

        throw new Exception("API call failed after all retries.");
    }

    private StatusCodeExplanation GetFallbackExplanation(int statusCode)
    {
        return new StatusCodeExplanation
        {
            Code = statusCode,
            Name = "Unknown",
            Category = "Unknown",
            Description = $"Explanation for HTTP status code {statusCode} is unavailable (API key missing).",
            WhenToUse = "API unavailable",
            CommonScenarios = "API unavailable",
            BestPractices = "API unavailable",
            ExampleResponse = "API unavailable",
            RelatedCodes = "API unavailable"
        };
    }
}
