using HttpStatusCodeTeacher.Models;

namespace HttpStatusCodeTeacher.Services;

/// <summary>
/// Interface for AI service providers (Gemini, Claude, etc.)
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Gets explanation for a given HTTP status code
    /// </summary>
    Task<StatusCodeExplanation> ExplainStatusCodeAsync(int statusCode);
}
