using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
using System.Reflection;
using ServerApp.Data;
using ServerApp.Endpoints;
using ServerApp.Middleware;
using ServerApp.Models;
using ServerApp.Services;

// ============================================
// SECTION 1: Application Builder Setup
// ============================================
var builder = WebApplication.CreateBuilder(args);

// ============================================
// SECTION 2: Service Configuration
// ============================================
// Features: In-memory caching, performance monitoring, CORS, OpenAPI documentation

// OpenAPI/Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InventoryHub API",
        Version = "v1",
        Description = "Product inventory management API with caching and performance monitoring",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@inventoryhub.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    options.EnableAnnotations();
});

// Database - In-memory for development
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("InventoryHub"));

// Memory caching with CacheService for product data (5 min absolute, 2 min sliding)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CacheService>();

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// CORS - Allow specific Blazor client origins
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5019", "https://localhost:7253")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Response Compression - Brotli (preferred) and Gzip fallback
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // Enable compression over HTTPS
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    
    // MIME types to compress (JSON, XML, text)
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",           // API responses
        "application/xml",            // XML responses
        "text/plain",                 // Text responses
        "text/css",                   // CSS files
        "application/javascript",     // JavaScript files
        "text/html",                  // HTML files
        "image/svg+xml"              // SVG images
    });
});

// Configure Brotli compression level (Fastest = good balance of speed and compression)
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Configure Gzip compression level (Fastest = good balance of speed and compression)
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// ============================================
// SECTION 3: Build Application
// ============================================
var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// ============================================
// SECTION 4: Middleware Pipeline Configuration
// ============================================

// Initialize database with seed data
await DbInitializerService.InitializeAsync(app.Services);

// Enable CORS
app.UseCors();

// Enable response compression (must be before UseStaticFiles and endpoints)
app.UseResponseCompression();

// Enable performance monitoring middleware
app.UsePerformanceMonitoring();

// Log application startup
logger.LogInformation("InventoryHub ServerApp starting up - Performance monitoring enabled");

// ============================================
// SECTION 5: API Endpoint Mapping
// ============================================

// Product endpoints (extracted to ProductEndpoints.cs)
app.MapProductEndpoints();

// Category endpoint - Get all categories for dropdown selectors
app.MapGet("/api/categories", (ILogger<Program> logger) =>
{
    try
    {
        var categories = SeedingService.GetCategories();
        logger.LogDebug("Retrieved {Count} categories", categories.Length);
        return Results.Ok(categories);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving categories");
        throw;
    }
})
.WithName("GetCategories")
.WithTags("Categories")
.Produces<Category[]>(StatusCodes.Status200OK);

// ============================================
// SECTION 6: Static Files & Documentation
// ============================================

// Static files
app.UseStaticFiles();

// Swagger UI (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "InventoryHub API v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "InventoryHub API Documentation";
        options.EnableTryItOutByDefault();
        options.DisplayRequestDuration();
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
    });
}

// ============================================
// SECTION 7: Start Application
// ============================================
logger.LogInformation("InventoryHub ServerApp configured and ready to serve requests");

app.Run();
