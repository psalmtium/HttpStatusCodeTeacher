using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HttpStatusCodeTeacher.Models;

/// <summary>
/// Represents a part in a message (can be text or data)
/// </summary>
public class MessagePart
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("data")]
    public List<MessagePart>? Data { get; set; }
}

/// <summary>
/// Metadata information from Telex
/// </summary>
public class MessageMetadata
{
    [JsonPropertyName("telex_user_id")]
    public string? TelexUserId { get; set; }

    [JsonPropertyName("telex_channel_id")]
    public string? TelexChannelId { get; set; }

    [JsonPropertyName("org_id")]
    public string? OrgId { get; set; }
}

/// <summary>
/// Push notification configuration
/// </summary>
public class PushNotificationConfig
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("authentication")]
    public Dictionary<string, object>? Authentication { get; set; }
}

/// <summary>
/// Configuration for the agent
/// </summary>
public class A2AConfiguration
{
    [JsonPropertyName("acceptedOutputModes")]
    public List<string>? AcceptedOutputModes { get; set; }

    [JsonPropertyName("historyLength")]
    public int? HistoryLength { get; set; }

    [JsonPropertyName("pushNotificationConfig")]
    public PushNotificationConfig? PushNotificationConfig { get; set; }

    [JsonPropertyName("blocking")]
    public bool? Blocking { get; set; }
}

/// <summary>
/// Represents a message with role and parts
/// </summary>
public class A2AMessage
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [Required]
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("parts")]
    public List<MessagePart> Parts { get; set; } = new();

    [JsonPropertyName("metadata")]
    public MessageMetadata? Metadata { get; set; }

    [JsonPropertyName("messageId")]
    public string? MessageId { get; set; }
}

/// <summary>
/// Parameters for the JSON-RPC request
/// </summary>
public class A2AParams
{
    [Required]
    [JsonPropertyName("message")]
    public A2AMessage Message { get; set; } = new();

    [JsonPropertyName("configuration")]
    public A2AConfiguration? Configuration { get; set; }
}

/// <summary>
/// The main JSON-RPC request structure from Telex.
/// </summary>
public class A2ARequest
{
    [Required]
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [Required]
    [JsonPropertyName("id")]
    public object? Id { get; set; }  // Can be string, int, or null per JSON-RPC 2.0

    [Required]
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("params")]
    public A2AParams Params { get; set; } = new();
}

/// <summary>
/// Task information for the response
/// </summary>
public class TaskInfo
{
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("status")]
    public string Status { get; set; } = "completed";
}

/// <summary>
/// Message information for the response
/// </summary>
public class MessageInfo
{
    [Required]
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;
}

/// <summary>
/// Result for successful JSON-RPC response
/// </summary>
public class A2AResult
{
    [Required]
    [JsonPropertyName("role")]
    public string Role { get; set; } = "agent";

    [Required]
    [JsonPropertyName("parts")]
    public List<MessagePart> Parts { get; set; } = new();

    [Required]
    [JsonPropertyName("Task")]
    public TaskInfo Task { get; set; } = new();

    [Required]
    [JsonPropertyName("Message")]
    public MessageInfo Message { get; set; } = new();

    [Required]
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "message";

    [Required]
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = string.Empty;
}

/// <summary>
/// Success response structure
/// </summary>
public class A2ASuccessResponse
{
    [Required]
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [Required]
    [JsonPropertyName("id")]
    public object? Id { get; set; }  // Can be string, int, or null per JSON-RPC 2.0

    [Required]
    [JsonPropertyName("result")]
    public A2AResult Result { get; set; } = new();
}

/// <summary>
/// Error details for error response
/// </summary>
public class A2AError
{
    [Required]
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [Required]
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Error response structure
/// </summary>
public class A2AErrorResponse
{
    [Required]
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [Required]
    [JsonPropertyName("id")]
    public object? Id { get; set; }  // Can be string, int, or null per JSON-RPC 2.0

    [Required]
    [JsonPropertyName("error")]
    public A2AError Error { get; set; } = new();
}
