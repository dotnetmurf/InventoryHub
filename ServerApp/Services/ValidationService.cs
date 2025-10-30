using System.ComponentModel.DataAnnotations;
using ServerApp.Models;

namespace ServerApp.Services;

/// <summary>
/// Service for validating request models and generating standardized error responses
/// </summary>
public static class ValidationService
{
    /// <summary>
    /// Validates a request object using Data Annotations and returns validation results
    /// </summary>
    /// <typeparam name="T">The type of object to validate</typeparam>
    /// <param name="request">The request object to validate</param>
    /// <param name="problemDetails">Output parameter containing validation errors if validation fails</param>
    /// <returns>True if validation passes, false otherwise</returns>
    public static bool TryValidate<T>(T request, out ValidationProblemDetails? problemDetails) where T : class
    {
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        
        bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);
        
        if (isValid)
        {
            problemDetails = null;
            return true;
        }
        
        // Build the problem details response
        problemDetails = new ValidationProblemDetails
        {
            Title = "One or more validation errors occurred.",
            Status = 400,
            Detail = "Please correct the validation errors and try again."
        };
        
        // Group validation errors by property name
        foreach (var validationResult in validationResults)
        {
            var propertyName = validationResult.MemberNames.FirstOrDefault() ?? "General";
            var errorMessage = validationResult.ErrorMessage ?? "Validation error";
            
            if (!problemDetails.Errors.ContainsKey(propertyName))
            {
                problemDetails.Errors[propertyName] = new List<string>();
            }
            
            problemDetails.Errors[propertyName].Add(errorMessage);
        }
        
        return false;
    }
    
    /// <summary>
    /// Creates a ValidationProblemDetails response for a specific field error
    /// </summary>
    /// <param name="fieldName">The name of the field with the error</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="traceId">Optional trace ID for debugging</param>
    /// <returns>ValidationProblemDetails with the specified error</returns>
    public static ValidationProblemDetails CreateFieldError(string fieldName, string errorMessage, string? traceId = null)
    {
        return new ValidationProblemDetails
        {
            Title = "Validation error occurred.",
            Status = 400,
            Detail = errorMessage,
            Errors = new Dictionary<string, List<string>>
            {
                { fieldName, new List<string> { errorMessage } }
            },
            TraceId = traceId
        };
    }
}
