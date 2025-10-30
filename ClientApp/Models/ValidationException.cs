namespace ClientApp.Models;

/// <summary>
/// Exception thrown when server-side validation fails
/// </summary>
/// <remarks>
/// Contains the validation error details from the server's ValidationProblemDetails response.
/// This allows the client to display field-specific validation errors to the user.
/// </remarks>
public class ValidationException : Exception
{
    /// <summary>
    /// The raw JSON validation problem details from the server
    /// </summary>
    public string ValidationErrorsJson { get; }

    /// <summary>
    /// Initializes a new validation exception
    /// </summary>
    /// <param name="validationErrorsJson">The JSON string containing validation error details</param>
    public ValidationException(string validationErrorsJson)
        : base("One or more validation errors occurred.")
    {
        ValidationErrorsJson = validationErrorsJson;
    }

    /// <summary>
    /// Initializes a new validation exception with a custom message
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="validationErrorsJson">The JSON string containing validation error details</param>
    public ValidationException(string message, string validationErrorsJson)
        : base(message)
    {
        ValidationErrorsJson = validationErrorsJson;
    }
}
