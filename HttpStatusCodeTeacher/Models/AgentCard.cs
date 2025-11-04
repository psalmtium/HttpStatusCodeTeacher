using System.Text.Json.Serialization;

namespace HttpStatusCodeTeacher.Models;

/// <summary>
/// Agent Card model for Telex/Mastra workflow format
/// </summary>
public class AgentCard
{
    [JsonPropertyName("active")]
    public bool Active { get; set; } = true;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("long_description")]
    public string LongDescription { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("nodes")]
    public List<AgentNode> Nodes { get; set; } = new();

    [JsonPropertyName("pinData")]
    public Dictionary<string, object> PinData { get; set; } = new();

    [JsonPropertyName("settings")]
    public AgentSettings Settings { get; set; } = new();

    [JsonPropertyName("short_description")]
    public string ShortDescription { get; set; } = string.Empty;
}

public class AgentNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();

    [JsonPropertyName("position")]
    public int[] Position { get; set; } = new int[2];

    [JsonPropertyName("type")]
    public string Type { get; set; } = "a2a/mastra-a2a-node";

    [JsonPropertyName("typeVersion")]
    public int TypeVersion { get; set; } = 1;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public class AgentSettings
{
    [JsonPropertyName("executionOrder")]
    public string ExecutionOrder { get; set; } = "v1";
}
