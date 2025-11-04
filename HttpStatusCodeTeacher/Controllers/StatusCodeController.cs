using Microsoft.AspNetCore.Mvc;
using HttpStatusCodeTeacher.Models;
using HttpStatusCodeTeacher.Services;

namespace HttpStatusCodeTeacher.Controllers;

/// <summary>
/// Controller for HTTP status code education endpoints
/// </summary>
[ApiController]
[Route("api/v1")]
[Tags("HTTP Status Code Teacher")]
public class StatusCodeController(AiServiceFactory aiServiceFactory, ILogger<StatusCodeController> logger)
    : ControllerBase
{
    private readonly IAiService _aiService = aiServiceFactory.GetAiService();

    /// <summary>
    /// Explains a specific HTTP status code with detailed educational content.
    /// </summary>
    /// <param name="code">The HTTP status code to explain (e.g., 200, 404, 500)</param>
    /// <returns>A JSON response containing detailed explanation of the status code.</returns>
    [HttpGet("explain")]
    [ProducesResponseType(typeof(StatusCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExplainStatusCode([FromQuery] int code)
    {
        logger.LogInformation("API Call: Explaining HTTP status code: {Code}", code);

        if (code is < 100 or > 599)
        {
            return BadRequest(new { error = "Invalid status code. Must be between 100 and 599." });
        }

        try
        {
            var explanation = await _aiService.ExplainStatusCodeAsync(code);

            var response = new StatusCodeResponse
            {
                Status = "success",
                Explanation = explanation
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error explaining status code: {Code}", code);
            return StatusCode(500, new { error = "Failed to generate explanation for the status code" });
        }
    }

    /// <summary>
    /// Lists common HTTP status codes by category.
    /// </summary>
    /// <param name="category">Optional category filter (1xx, 2xx, 3xx, 4xx, 5xx)</param>
    /// <returns>A list of common HTTP status codes</returns>
    [HttpGet("codes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ListCommonCodes([FromQuery] string? category = null)
    {
        var codes = new Dictionary<string, List<object>>
        {
            ["1xx"] =
            [
                new { code = 100, name = "Continue" },
                new { code = 101, name = "Switching Protocols" },
                new { code = 102, name = "Processing" }
            ],
            ["2xx"] =
            [
                new { code = 200, name = "OK" },
                new { code = 201, name = "Created" },
                new { code = 202, name = "Accepted" },
                new { code = 204, name = "No Content" },
                new { code = 206, name = "Partial Content" }
            ],
            ["3xx"] =
            [
                new { code = 301, name = "Moved Permanently" },
                new { code = 302, name = "Found" },
                new { code = 304, name = "Not Modified" },
                new { code = 307, name = "Temporary Redirect" },
                new { code = 308, name = "Permanent Redirect" }
            ],
            ["4xx"] =
            [
                new { code = 400, name = "Bad Request" },
                new { code = 401, name = "Unauthorized" },
                new { code = 403, name = "Forbidden" },
                new { code = 404, name = "Not Found" },
                new { code = 405, name = "Method Not Allowed" },
                new { code = 409, name = "Conflict" },
                new { code = 429, name = "Too Many Requests" }
            ],
            ["5xx"] =
            [
                new { code = 500, name = "Internal Server Error" },
                new { code = 501, name = "Not Implemented" },
                new { code = 502, name = "Bad Gateway" },
                new { code = 503, name = "Service Unavailable" },
                new { code = 504, name = "Gateway Timeout" }
            ]
        };

        if (string.IsNullOrEmpty(category)) return Ok(codes);
        var normalizedCategory = category.ToLower();
        if (codes.TryGetValue(normalizedCategory, out var value))
        {
            return Ok(new { category = normalizedCategory, codes = value });
        }
        return BadRequest(new { error = "Invalid category. Use 1xx, 2xx, 3xx, 4xx, or 5xx." });

    }
}
