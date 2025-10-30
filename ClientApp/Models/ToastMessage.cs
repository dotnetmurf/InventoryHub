namespace ClientApp.Models;

/// <summary>
/// Represents a toast notification message
/// </summary>
public class ToastMessage
{
    /// <summary>
    /// Unique identifier for the toast
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The main message to display
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The type of toast (success, info, warning, error)
    /// </summary>
    public ToastType Type { get; set; } = ToastType.Success;

    /// <summary>
    /// Duration in milliseconds before auto-dismiss (0 = no auto-dismiss)
    /// </summary>
    public int Duration { get; set; } = 3000;

    /// <summary>
    /// Timestamp when the toast was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Toast notification types
/// </summary>
public enum ToastType
{
    Success,
    Info,
    Warning,
    Error
}
