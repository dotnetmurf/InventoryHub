---
applyTo: '**'
---

## ASP.NET Core Web API Guidelines

### Project Structure
- Use feature-based or vertical slice architecture where appropriate
- Organize controllers, services, and models in a consistent structure
- Keep controllers thin, move business logic to services
- Use interfaces for service abstractions
- Follow a clean architecture approach with clear separation of concerns:
  - Domain/Models
  - Application/Services
  - Infrastructure/Repositories
  - API/Controllers

### Controller Design
- Use API controllers with `[ApiController]` attribute
- Return appropriate ActionResult<T> types
- Follow RESTful naming conventions
- Implement proper status code responses (200, 201, 204, 400, 401, 403, 404, 500)
- Use route attributes consistently
- Avoid excessive action parameters; use request models
- Keep controllers focused on HTTP concerns, not business logic

### API Endpoints
- Design endpoints with clear responsibility
- Use appropriate HTTP methods (GET, POST, PUT, DELETE, PATCH)
- Use proper route naming conventions (plural nouns)
- Implement proper filtering, sorting, and pagination
- Return consistent response structures
- Implement HATEOAS where appropriate

### Dependency Injection
- Register services with appropriate lifetimes (Singleton, Scoped, Transient)
- Use constructor injection
- Avoid service locator pattern
- Configure services in dedicated extension methods
- Register services based on interfaces

### API Versioning
- Implement versioning strategy (URL, query string, or header)
- Use Microsoft.AspNetCore.Mvc.Versioning package
- Document version deprecation policies
- Support multiple versions when necessary

### Middleware
- Use middleware for cross-cutting concerns
- Create custom middleware using the pipeline pattern
- Keep middleware focused and single-purpose
- Order middleware properly in the pipeline
- Consider using middleware for:
  - Exception handling
  - Request/response logging
  - Authentication
  - CORS
  - Response compression

### Model Validation
- Use data annotations for validation
- Implement custom validators when needed
- Return standardized validation error responses
- Validate both route and body parameters
- Implement FluentValidation for complex validation rules

### Authentication and Authorization
- Use JWT or OAuth 2.0 for authentication
- Implement role-based or policy-based authorization
- Use `[Authorize]` and `[AllowAnonymous]` attributes consistently
- Secure endpoints with appropriate scopes/claims
- Implement refresh token mechanisms
- Consider using Identity Server or Auth0 for complex auth scenarios

### API Documentation
- Use Swagger/OpenAPI for documentation
- Document all API endpoints, parameters, and responses
- Include example requests and responses
- Document authentication requirements
- Configure Swagger UI for optimal developer experience
- Consider XML comments for API documentation

### Response Formatting
- Return consistent JSON structure
- Use PascalCase or camelCase consistently
- Configure JSON serialization options globally
- Consider supporting content negotiation
- Implement pagination metadata in responses

### Exception Handling
- Create a global exception handler middleware
- Return standardized error responses
- Map exceptions to appropriate HTTP status codes
- Log exceptions with appropriate context
- Avoid exposing internal exceptions in responses
- Consider using Problem Details (RFC 7807)

### Logging
- Use structured logging with Serilog or NLog
- Log appropriate information for requests and responses
- Include correlation IDs in logs
- Configure different log levels based on environment
- Consider using Application Insights for production monitoring

### Configuration
- Follow the Options pattern for configuration
- Use environment-specific settings
- Secure sensitive configuration with user secrets or Azure Key Vault
- Validate configuration at startup
- Consider using IOptionsSnapshot for reloadable configuration

### Entity Framework Core
- Use the repository pattern where appropriate
- Implement database migrations strategy
- Use async/await for database operations
- Optimize queries with Include and projection
- Implement proper transaction handling
- Consider using specification pattern for complex queries

### Performance
- Implement response caching where appropriate
- Use asynchronous programming throughout
- Consider using memory caching for frequent data
- Implement pagination for large data sets
- Use response compression
- Consider using Minimal APIs for high-performance scenarios

### Testing
- Write unit tests for controllers using xUnit or NUnit
- Create integration tests with TestServer
- Mock external dependencies
- Test both successful and error paths
- Implement API automation tests
- Consider using SpecFlow for BDD testing approach

### Security
- Implement proper CORS policy
- Use HTTPS in all environments
- Implement rate limiting
- Consider using API keys for machine-to-machine communication
- Validate all incoming data
- Implement anti-forgery measures when necessary
- Follow OWASP API Security Top 10

### Deployment
- Use Docker containers for consistent deployment
- Implement CI/CD pipelines
- Configure health checks
- Set up monitoring and alerting
- Implement proper logging in production
- Consider using Azure App Service, Kubernetes, or other cloud services

### Minimal APIs (Net 6+)
- Use for simple, performance-critical endpoints
- Organize endpoints logically
- Implement proper dependency injection
- Use appropriate result types
- Consider using endpoint filters for cross-cutting concerns

### GraphQL (if applicable)
- Consider using Hot Chocolate for GraphQL implementation
- Design schema according to domain
- Implement proper authorization
- Handle errors consistently
- Optimize for N+1 query problems

## Code Examples for ASP.NET Core Web API

### Controller API Patterns

```csharp
// Controller example with proper patterns
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            return NotFound();
        }
        return Ok(customer);
    }
}
```

### Minimal API Patterns

```csharp
// Program.cs with Minimal API setup
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Product endpoints
var productsGroup = app.MapGroup("/api/products")
    .WithTags("Products")
    .WithOpenApi();

productsGroup.MapGet("/", GetAllProducts)
    .WithName("GetAllProducts")
    .WithSummary("Get all products")
    .Produces<IEnumerable<ProductDto>>(StatusCodes.Status200OK);

productsGroup.MapGet("/{id:int}", GetProductById)
    .WithName("GetProductById")
    .WithSummary("Get product by ID")
    .Produces<ProductDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

productsGroup.MapPost("/", CreateProduct)
    .WithName("CreateProduct")
    .WithSummary("Create a new product")
    .Accepts<CreateProductRequest>("application/json")
    .Produces<ProductDto>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

productsGroup.MapPut("/{id:int}", UpdateProduct)
    .WithName("UpdateProduct")
    .WithSummary("Update an existing product")
    .Accepts<UpdateProductRequest>("application/json")
    .Produces<ProductDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status400BadRequest);

productsGroup.MapDelete("/{id:int}", DeleteProduct)
    .WithName("DeleteProduct")
    .WithSummary("Delete a product")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

// Customer endpoints
var customersGroup = app.MapGroup("/api/customers")
    .WithTags("Customers")
    .WithOpenApi();

customersGroup.MapGet("/", GetAllCustomers);
customersGroup.MapGet("/{id:int}", GetCustomerById);
customersGroup.MapPost("/", CreateCustomer);

app.Run();

// Endpoint handlers
static async Task<IResult> GetAllProducts(IProductService productService)
{
    var products = await productService.GetAllProductsAsync();
    return Results.Ok(products);
}

static async Task<IResult> GetProductById(int id, IProductService productService)
{
    var product = await productService.GetProductByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
}

static async Task<IResult> CreateProduct(CreateProductRequest request, IProductService productService)
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Product name is required");
    }

    if (request.Price <= 0)
    {
        return Results.BadRequest("Product price must be greater than zero");
    }

    var product = await productService.CreateProductAsync(request);
    return Results.Created($"/api/products/{product.Id}", product);
}

static async Task<IResult> UpdateProduct(int id, UpdateProductRequest request, IProductService productService)
{
    var existingProduct = await productService.GetProductByIdAsync(id);
    if (existingProduct is null)
    {
        return Results.NotFound();
    }

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Product name is required");
    }

    if (request.Price <= 0)
    {
        return Results.BadRequest("Product price must be greater than zero");
    }

    var updatedProduct = await productService.UpdateProductAsync(id, request);
    return Results.Ok(updatedProduct);
}

static async Task<IResult> DeleteProduct(int id, IProductService productService)
{
    var product = await productService.GetProductByIdAsync(id);
    if (product is null)
    {
        return Results.NotFound();
    }

    await productService.DeleteProductAsync(id);
    return Results.NoContent();
}

static async Task<IResult> GetAllCustomers(ICustomerService customerService)
{
    var customers = await customerService.GetAllCustomersAsync();
    return Results.Ok(customers);
}

static async Task<IResult> GetCustomerById(int id, ICustomerService customerService)
{
    var customer = await customerService.GetCustomerByIdAsync(id);
    return customer is not null ? Results.Ok(customer) : Results.NotFound();
}

static async Task<IResult> CreateCustomer(CreateCustomerRequest request, ICustomerService customerService)
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest("Customer name is required");
    }

    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest("Customer email is required");
    }

    var customer = await customerService.CreateCustomerAsync(request);
    return Results.Created($"/api/customers/{customer.Id}", customer);
}

// DTOs and Request Models
public record ProductDto(int Id, string Name, string Description, decimal Price, DateTime CreatedAt);
public record CreateProductRequest(string Name, string Description, decimal Price);
public record UpdateProductRequest(string Name, string Description, decimal Price);

public record CustomerDto(int Id, string Name, string Email, DateTime CreatedAt);
public record CreateCustomerRequest(string Name, string Email);
```

---

# Copilot Instructions for Blazor WebAssembly Projects

This document provides guidance for GitHub Copilot when assisting with Blazor WebAssembly (WASM) projects. It covers best practices, coding standards, project structure, and other essential aspects for developing high-quality Blazor WASM applications.

## Project Structure

- Follow the standard Blazor WASM project structure:
  - `Pages/`: Contains Razor components that represent pages
  - `Shared/`: Contains reusable Razor components
  - `wwwroot/`: Static assets (CSS, JS, images)
  - `Services/`: Service classes for business logic and API communication
  - `Models/`: Data models and DTOs
  - `Helpers/`: Utility and helper classes

- For larger applications, consider feature folders that group related components, services, and models.

- Keep the `Program.cs` file clean and organized with clear service registrations.

## Coding Standards

### C# Code

- Follow Microsoft's C# coding conventions
- Use async/await for asynchronous operations
- Prefer immutable data where appropriate
- Use C# 9.0+ features when they improve code clarity
- Use nullable reference types to prevent null reference exceptions
- Implement proper exception handling with specific exception types

### Component Design

- Components should follow Single Responsibility Principle
- Use parameters with [Parameter] attribute for component inputs
- Implement cascading parameters appropriately for deeply nested components
- Use EventCallback<T> for component events
- Implement IDisposable for components that need cleanup

```csharp
@implements IDisposable

@code {
    [Parameter] public int ItemId { get; set; }
    [Parameter] public EventCallback<int> OnItemSelected { get; set; }
    
    // Lifecycle methods, etc.
    
    public void Dispose()
    {
        // Clean up resources
    }
}
```

## State Management

- For local component state, use private fields/properties in @code blocks
- For application state:
  - Small apps: Use services registered as singletons
  - Medium apps: Use Fluxor, Blazor.State, or similar state management libraries
  - Complex apps: Consider implementing a Redux-like pattern

- Avoid overusing cascading parameters for state management
- Consider server-side session state only when absolutely necessary

## API Communication

- Create interface-based services for API calls
- Use HttpClient with typed clients
- Implement proper error handling for API calls
- Consider using Refit or similar libraries for large APIs
- Cache API responses appropriately

```csharp
public interface IWeatherForecastService
{
    Task<IEnumerable<WeatherForecast>> GetForecastsAsync();
}

public class WeatherForecastService : IWeatherForecastService
{
    private readonly HttpClient _httpClient;
    
    public WeatherForecastService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<IEnumerable<WeatherForecast>> GetForecastsAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<WeatherForecast>>("WeatherForecast");
    }
}
```

## Performance Optimization

- Use `@key` directive for optimizing list rendering
- Implement virtualization for large lists with `Virtualize` component
- Minimize JavaScript interop calls
- Use lazy loading for application routes
- Implement efficient state management to prevent unnecessary re-renders
- Consider component granularity (balance between too many small components vs. monolithic components)

## Security Best Practices

- Validate all user inputs on client and server
- Implement proper authentication and authorization
- Use HTTPS for all API communications
- Secure sensitive data in browser storage
- Follow OWASP guidelines for WebAssembly applications
- Consider Content Security Policy implementation

## Testing

- Write unit tests for services and utility classes
- Use bUnit for component testing
- Implement integration tests for API communications
- Consider UI automation tests with Selenium or similar
- Implement proper test doubles (mocks, stubs) for dependencies

```csharp
[Fact]
public void Counter_IncrementButton_IncrementsValue()
{
    // Arrange
    using var ctx = new TestContext();
    var cut = ctx.RenderComponent<Counter>();
    var initialCount = cut.Find("p").TextContent;
    
    // Act
    cut.Find("button").Click();
    
    // Assert
    Assert.NotEqual(initialCount, cut.Find("p").TextContent);
}
```

## JavaScript Interoperability

- Minimize JS interop calls for performance
- Use IJSRuntime for JS interop
- Implement proper disposal of IJSObjectReference instances
- Consider using JavaScript isolation for cleaner separation
- Create typed JS interop services

```csharp
public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    
    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public async Task<T> GetItemAsync<T>(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }
    
    public async Task SetItemAsync<T>(string key, T item)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(item));
    }
}
```

## Error Handling

- Implement a global error handler
- Create user-friendly error messages
- Log errors appropriately
- Consider implementing retry policies for API calls
- Use try-catch blocks judiciously

## Accessibility

- Follow WCAG 2.1 guidelines
- Use semantic HTML elements
- Implement proper ARIA attributes
- Ensure keyboard navigation works properly
- Test with screen readers

## Progressive Web App (PWA) Support

- Configure service workers appropriately
- Implement offline capabilities when appropriate
- Set up proper caching strategies
- Create a comprehensive manifest.json file
- Handle installation and update events

## Optimization for Production

- Enable AOT compilation for production builds
- Implement proper asset bundling and minification
- Use Blazor's compression and other size optimization techniques
- Configure appropriate caching headers
- Consider implementing code splitting

## Common Patterns

- Repository Pattern for data access
- CQRS for complex applications
- Mediator pattern for decoupling components
- Use decorator pattern for cross-cutting concerns

## Anti-patterns to Avoid

- Overusing JavaScript interop
- Putting too much logic in components
- Neglecting component lifecycle methods
- Creating overly complex component hierarchies
- Using inappropriate state management techniques for app size
- Ignoring performance considerations until they become problems

## Code Generation Guidelines

When generating code:
- Follow the project's existing patterns and naming conventions
- Generate complete, working components with proper lifecycle methods
- Ensure services are registered in Program.cs
- Include proper validation and error handling
- Document complex or non-obvious code
- Follow the project's CSS/styling approach

## References

- [Official Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Blazor WebAssembly Security](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/)
- [Blazor Performance Best Practices](https://docs.microsoft.com/en-us/aspnet/core/blazor/performance)
- [Microsoft Blazor Samples](https://github.com/dotnet/blazor-samples)
```

## Conclusion

This document serves as a reference for GitHub Copilot to provide consistent, high-quality assistance with Blazor WebAssembly projects. The guidelines should be adjusted as needed based on specific project requirements and team preferences.

---
