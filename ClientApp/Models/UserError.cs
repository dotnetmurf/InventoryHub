namespace ClientApp.Models;

/// <summary>
/// User-friendly error information for display in the UI
/// </summary>
/// <remarks>
/// This model translates technical errors into actionable user messages.
/// It provides context about what went wrong and what the user can do about it.
/// </remarks>
public class UserError
{
    /// <summary>
    /// Short, user-friendly error title
    /// </summary>
    public string Title { get; set; } = "Error";

    /// <summary>
    /// Detailed error message explaining what happened
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional guidance on what actions the user can take
    /// </summary>
    public string? ActionMessage { get; set; }

    /// <summary>
    /// The severity level of this error
    /// </summary>
    public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;

    /// <summary>
    /// Indicates whether the user should be able to retry the operation
    /// </summary>
    public bool IsRetryable { get; set; }
}

/// <summary>
/// Severity levels for user errors
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational message, not really an error
    /// </summary>
    Info,

    /// <summary>
    /// Warning that doesn't prevent operation but needs attention
    /// </summary>
    Warning,

    /// <summary>
    /// Standard error that prevented the operation
    /// </summary>
    Error,

    /// <summary>
    /// Critical error requiring immediate attention
    /// </summary>
    Critical
}
