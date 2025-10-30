using System.Net;

namespace ClientApp.Models;

/// <summary>
/// Exception thrown when a ProductService operation fails
/// </summary>
/// <remarks>
/// Provides structured error information including HTTP status codes,
/// operation context, and response details for better error handling and debugging.
/// </remarks>
public class ProductServiceException : Exception
{
    /// <summary>
    /// The HTTP status code from the failed API response
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// The name of the operation that failed
    /// </summary>
    /// <example>CreateProduct, UpdateProduct, DeleteProduct, GetProducts</example>
    public string Operation { get; }

    /// <summary>
    /// Raw response body from the server for debugging
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Additional context information about the failed operation
    /// </summary>
    /// <remarks>
    /// May include product IDs, names, filter parameters, etc.
    /// </remarks>
    public Dictionary<string, object>? Context { get; }

    /// <summary>
    /// Initializes a new ProductServiceException with message and operation
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="operation">Name of the operation that failed</param>
    public ProductServiceException(string message, string operation)
        : base(message)
    {
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new ProductServiceException with status code
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="operation">Name of the operation that failed</param>
    /// <param name="statusCode">HTTP status code from the response</param>
    public ProductServiceException(string message, string operation, HttpStatusCode statusCode)
        : base(message)
    {
        Operation = operation;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new ProductServiceException with inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="operation">Name of the operation that failed</param>
    /// <param name="innerException">The exception that caused this error</param>
    public ProductServiceException(string message, string operation, Exception innerException)
        : base(message, innerException)
    {
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new ProductServiceException with full details
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="operation">Name of the operation that failed</param>
    /// <param name="statusCode">HTTP status code from the response</param>
    /// <param name="responseBody">Raw response body from the server</param>
    /// <param name="context">Additional context about the operation</param>
    public ProductServiceException(
        string message, 
        string operation, 
        HttpStatusCode statusCode,
        string? responseBody = null,
        Dictionary<string, object>? context = null)
        : base(message)
    {
        Operation = operation;
        StatusCode = statusCode;
        ResponseBody = responseBody;
        Context = context;
    }

    /// <summary>
    /// Initializes a new ProductServiceException with full details and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="operation">Name of the operation that failed</param>
    /// <param name="statusCode">HTTP status code from the response</param>
    /// <param name="innerException">The exception that caused this error</param>
    /// <param name="responseBody">Raw response body from the server</param>
    /// <param name="context">Additional context about the operation</param>
    public ProductServiceException(
        string message,
        string operation,
        HttpStatusCode statusCode,
        Exception innerException,
        string? responseBody = null,
        Dictionary<string, object>? context = null)
        : base(message, innerException)
    {
        Operation = operation;
        StatusCode = statusCode;
        ResponseBody = responseBody;
        Context = context;
    }
}
