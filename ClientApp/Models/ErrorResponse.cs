namespace ClientApp.Models;

/// <summary>
/// Standardized error response from the API
/// </summary>
/// <remarks>
/// This model represents error information returned by the server API.
/// It provides structured error details including validation errors and correlation IDs.
/// </remarks>
public class ErrorResponse
{
    /// <summary>
    /// The main error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details or technical information
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Error code for programmatic handling
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Field-specific validation errors
    /// </summary>
    /// <remarks>
    /// Key: field name, Value: array of error messages for that field
    /// </remarks>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Unique identifier for tracing this error across logs
    /// </summary>
    public string? CorrelationId { get; set; }
}
