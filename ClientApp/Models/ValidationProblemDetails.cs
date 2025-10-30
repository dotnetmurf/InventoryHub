using System.Text.Json.Serialization;

namespace ClientApp.Models;

/// <summary>
/// Client-side model for RFC 7807 Problem Details validation error responses
/// </summary>
/// <remarks>
/// Matches the server's ValidationProblemDetails format,
/// allowing the client to parse and display field-specific validation errors.
/// </remarks>
public class ValidationProblemDetails
{
    /// <summary>
    /// A URI reference that identifies the problem type
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// The HTTP status code
    /// </summary>
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// A dictionary of field names and their associated validation error messages
    /// </summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>>? Errors { get; set; }

    /// <summary>
    /// Optional trace identifier for debugging
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}
