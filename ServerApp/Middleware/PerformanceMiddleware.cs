using System.Diagnostics;

namespace ServerApp.Middleware;

/// <summary>
/// Middleware that monitors and logs the performance of HTTP requests
/// </summary>
/// <remarks>
/// Automatically tracks request duration using Stopwatch and logs timing information.
/// Wraps requests in try-catch to ensure timing is captured even on errors.
/// Provides consistent performance monitoring across all endpoints without code duplication.
/// </remarks>
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the PerformanceMiddleware
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">Logger for performance metrics</param>
    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to track request performance
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        try
        {
            // Call the next middleware in the pipeline
            await _next(context);

            sw.Stop();

            // Log successful request with timing
            var statusCode = context.Response.StatusCode;
            _logger.LogInformation(
                "{Method} {Path} responded with {StatusCode} in {ElapsedMs} ms",
                method,
                path,
                statusCode,
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();

            // Log failed request with timing and exception
            _logger.LogError(ex,
                "{Method} {Path} failed after {ElapsedMs} ms",
                method,
                path,
                sw.ElapsedMilliseconds);

            // Re-throw to let global error handling deal with it
            throw;
        }
    }
}

/// <summary>
/// Extension methods for registering PerformanceMiddleware
/// </summary>
public static class PerformanceMiddlewareExtensions
{
    /// <summary>
    /// Adds PerformanceMiddleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for method chaining</returns>
    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PerformanceMiddleware>();
    }
}
