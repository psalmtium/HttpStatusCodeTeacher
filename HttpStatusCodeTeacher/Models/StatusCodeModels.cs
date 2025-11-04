using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HttpStatusCodeTeacher.Models;

/// <summary>
/// The structured explanation returned by the AI service for an HTTP status code.
/// </summary>
public class StatusCodeExplanation
{
    [Required]
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("when_to_use")]
    public string WhenToUse { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("common_scenarios")]
    public string CommonScenarios { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("best_practices")]
    public string BestPractices { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("example_response")]
    public string ExampleResponse { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("related_codes")]
    public string RelatedCodes { get; set; } = string.Empty;
}

/// <summary>
/// Full response schema for the public /api/v1/explain endpoint.
/// </summary>
public class StatusCodeResponse
{
    [Required]
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("explanation")]
    public StatusCodeExplanation Explanation { get; set; } = new();
}
