using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Service for managing toast notifications
/// </summary>
public class ToastService
{
    private readonly List<ToastMessage> _toasts = new();

    /// <summary>
    /// Event fired when a toast is added or removed
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Gets the current list of active toasts
    /// </summary>
    public IReadOnlyList<ToastMessage> Toasts => _toasts.AsReadOnly();

    /// <summary>
    /// Shows a success toast notification
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="duration">Duration in milliseconds (default 3000)</param>
    public void ShowSuccess(string message, int duration = 3000)
    {
        ShowToast(message, ToastType.Success, duration);
    }

    /// <summary>
    /// Shows an info toast notification
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="duration">Duration in milliseconds (default 3000)</param>
    public void ShowInfo(string message, int duration = 3000)
    {
        ShowToast(message, ToastType.Info, duration);
    }

    /// <summary>
    /// Shows a warning toast notification
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="duration">Duration in milliseconds (default 4000)</param>
    public void ShowWarning(string message, int duration = 4000)
    {
        ShowToast(message, ToastType.Warning, duration);
    }

    /// <summary>
    /// Shows an error toast notification
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="duration">Duration in milliseconds (default 5000)</param>
    public void ShowError(string message, int duration = 5000)
    {
        ShowToast(message, ToastType.Error, duration);
    }

    /// <summary>
    /// Shows a toast notification with custom settings
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="type">The type of toast</param>
    /// <param name="duration">Duration in milliseconds (0 = no auto-dismiss)</param>
    private void ShowToast(string message, ToastType type, int duration)
    {
        var toast = new ToastMessage
        {
            Message = message,
            Type = type,
            Duration = duration
        };

        _toasts.Add(toast);
        NotifyStateChanged();

        // Auto-dismiss after duration if duration > 0
        if (duration > 0)
        {
            _ = Task.Delay(duration).ContinueWith(_ => RemoveToast(toast.Id));
        }
    }

    /// <summary>
    /// Removes a specific toast by ID
    /// </summary>
    /// <param name="toastId">The ID of the toast to remove</param>
    public void RemoveToast(string toastId)
    {
        var toast = _toasts.FirstOrDefault(t => t.Id == toastId);
        if (toast != null)
        {
            _toasts.Remove(toast);
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Clears all active toasts
    /// </summary>
    public void ClearAll()
    {
        _toasts.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Notifies subscribers that the toast list has changed
    /// </summary>
    private void NotifyStateChanged() => OnChange?.Invoke();
}
