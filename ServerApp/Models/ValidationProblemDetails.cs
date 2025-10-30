using System.Text.Json.Serialization;

namespace ServerApp.Models;

/// <summary>
/// RFC 7807 Problem Details response for validation errors
/// </summary>
/// <remarks>
/// Provides a standardized format for validation error responses,
/// making it easier for clients to parse and display field-specific errors.
/// </remarks>
public class ValidationProblemDetails
{
    /// <summary>
    /// A URI reference that identifies the problem type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "https://tools.ietf.org/html/rfc7807";

    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = "One or more validation errors occurred.";

    /// <summary>
    /// The HTTP status code
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; } = 400;

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem
    /// </summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// A dictionary of field names and their associated validation error messages
    /// </summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>> Errors { get; set; } = new();

    /// <summary>
    /// Optional trace identifier for debugging
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}
