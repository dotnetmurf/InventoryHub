# Phase 8: Improve Error Handling

## Overview
**Purpose**: Implement comprehensive error handling with custom exceptions and global middleware  
**Time Estimate**: 2-3 hours  
**Risk Level**: Medium  
**Prerequisites**: Phases 1-7 complete

## Success Criteria
- ✅ Custom exception types created
- ✅ Global exception middleware implemented
- ✅ Correlation IDs added for request tracking
- ✅ Consistent error responses across all endpoints
- ✅ Existing error handling refactored to use new system

---

## Pre-Phase Setup

### Check Prerequisites
```powershell
# Verify on master branch
git branch --show-current
# Should show: master

# Verify no uncommitted changes
git status
# Should show: nothing to commit, working tree clean

# Verify previous phases complete
git tag --list | Where-Object { $_ -like "phase*" }
# Should show: phase1-complete through phase7-complete
```

### Create Feature Branch
```powershell
git checkout -b refactor/phase8-error-handling
```

---

## Step 8.1: Create Custom Exception Types

### Create Exceptions Folder
```powershell
New-Item -Path "ServerApp/Exceptions" -ItemType Directory -Force
```

### Create NotFoundException.cs

**Create file**: `ServerApp/Exceptions/NotFoundException.cs`

```csharp
namespace ServerApp.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found in the database
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Creates a NotFoundException with a custom message
    /// </summary>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a NotFoundException with entity name and key
    /// </summary>
    /// <param name="entityName">The name of the entity type (e.g., "Product")</param>
    /// <param name="key">The key value that was not found</param>
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' not found")
    {
    }
}
```

### Create DuplicateEntityException.cs

**Create file**: `ServerApp/Exceptions/DuplicateEntityException.cs`

```csharp
namespace ServerApp.Exceptions;

/// <summary>
/// Exception thrown when attempting to create an entity that would violate uniqueness constraints
/// </summary>
public class DuplicateEntityException : Exception
{
    /// <summary>
    /// The property name that has a duplicate value
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Creates a DuplicateEntityException
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="propertyName">The property that has a duplicate value</param>
    public DuplicateEntityException(string message, string propertyName)
        : base(message)
    {
        PropertyName = propertyName;
    }
}
```

### Create BusinessRuleException.cs

**Create file**: `ServerApp/Exceptions/BusinessRuleException.cs`

```csharp
namespace ServerApp.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : Exception
{
    /// <summary>
    /// The business rule that was violated
    /// </summary>
    public string RuleName { get; }

    /// <summary>
    /// Creates a BusinessRuleException
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="ruleName">The name of the business rule that was violated</param>
    public BusinessRuleException(string message, string ruleName = "")
        : base(message)
    {
        RuleName = ruleName;
    }
}
```

### Build to Verify
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Expected**: Build succeeds

---

## Step 8.2: Create Global Exception Middleware

### Create GlobalExceptionMiddleware.cs

**Create file**: `ServerApp/Middleware/GlobalExceptionMiddleware.cs`

```csharp
using System.Net;
using System.Text.Json;
using ServerApp.Exceptions;
using ServerApp.Models;

namespace ServerApp.Middleware;

/// <summary>
/// Middleware that catches all unhandled exceptions and returns appropriate HTTP responses
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Response.Headers["X-Correlation-Id"].ToString();

        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException notFoundEx => new
            {
                statusCode = HttpStatusCode.NotFound,
                message = notFoundEx.Message,
                correlationId
            },
            DuplicateEntityException duplicateEx => new
            {
                statusCode = HttpStatusCode.Conflict,
                message = duplicateEx.Message,
                property = duplicateEx.PropertyName,
                correlationId
            },
            BusinessRuleException businessEx => new
            {
                statusCode = HttpStatusCode.BadRequest,
                message = businessEx.Message,
                rule = businessEx.RuleName,
                correlationId
            },
            FluentValidation.ValidationException validationEx => new
            {
                statusCode = HttpStatusCode.BadRequest,
                message = "Validation failed",
                errors = validationEx.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    error = e.ErrorMessage
                }),
                correlationId
            },
            _ => new
            {
                statusCode = HttpStatusCode.InternalServerError,
                message = "An unexpected error occurred",
                correlationId
            }
        };

        context.Response.StatusCode = (int)response.statusCode;

        // Log the exception with appropriate level
        if (response.statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception,
                "Unhandled exception occurred | CorrelationId: {CorrelationId}",
                correlationId);
        }
        else
        {
            _logger.LogWarning(exception,
                "Expected exception occurred: {ExceptionType} | Message: {Message} | CorrelationId: {CorrelationId}",
                exception.GetType().Name,
                exception.Message,
                correlationId);
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
```

### Build to Verify
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Expected**: Build succeeds

---

## Step 8.3: Update PerformanceMiddleware with Correlation IDs

### Update PerformanceMiddleware.cs

**Edit file**: `ServerApp/Middleware/PerformanceMiddleware.cs`

Update the `InvokeAsync` method to add correlation ID:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Generate and add correlation ID
    var correlationId = Guid.NewGuid().ToString();
    context.Response.Headers.Add("X-Correlation-Id", correlationId);

    var sw = Stopwatch.StartNew();
    var requestMethod = context.Request.Method;
    var requestPath = context.Request.Path;

    _logger.LogInformation(
        "Request started: {Method} {Path} | CorrelationId: {CorrelationId}",
        requestMethod,
        requestPath,
        correlationId);

    await _next(context);

    sw.Stop();

    _logger.LogInformation(
        "Request completed: {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | CorrelationId: {CorrelationId}",
        requestMethod,
        requestPath,
        context.Response.StatusCode,
        sw.ElapsedMilliseconds,
        correlationId);
}
```

### Build to Verify
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Expected**: Build succeeds

---

## Step 8.4: Register Middleware in Program.cs

### Update Program.cs

**Edit file**: `ServerApp/Program.cs`

Add the global exception middleware BEFORE other middleware:

```csharp
// Add after builder.Build()
var app = builder.Build();

// Global exception handler must be first
app.UseMiddleware<GlobalExceptionMiddleware>();

// Then performance middleware
app.UseMiddleware<PerformanceMiddleware>();

// Then other middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Rest of middleware...
```

### Build to Verify
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Expected**: Build succeeds

---

## Step 8.5: Refactor ValidationService to Use Custom Exceptions

### Update ValidationService.cs

**Edit file**: `ServerApp/Services/ValidationService.cs`

Replace `KeyNotFoundException` with `NotFoundException`:

```csharp
using ServerApp.Exceptions;

// In ValidateProductExistsAsync method:
if (!exists)
{
    throw new NotFoundException("Product", id);
}

// In ValidateProductNameUniqueAsync method:
if (exists)
{
    throw new DuplicateEntityException(
        $"A product with the name '{name}' already exists",
        nameof(CreateProductRequest.Name));
}

// In ValidateProductNameUniqueForUpdateAsync method:
if (exists)
{
    throw new DuplicateEntityException(
        $"A product with the name '{name}' already exists",
        nameof(UpdateProductRequest.Name));
}
```

### Build to Verify
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Expected**: Build succeeds

---

## Step 8.6: Update ProductEndpoints Error Handling

### Update ProductEndpoints.cs

**Edit file**: `ServerApp/Endpoints/ProductEndpoints.cs`

Simplify error handling since middleware now handles exceptions:

```csharp
// GetById endpoint - remove try-catch
products.MapGet("/{id}", async (int id, IProductBusinessService productService) =>
{
    var product = await productService.GetByIdAsync(id);
    return Results.Ok(product);
})
.WithName("GetProductById")
.Produces<Product>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// Create endpoint - remove try-catch
products.MapPost("/", async (CreateProductRequest request, IProductBusinessService productService) =>
{
    var product = await productService.CreateAsync(request);
    return Results.Created($"/api/products/{product.Id}", product);
})
.WithName("CreateProduct")
.Produces<Product>(StatusCodes.Status201Created)
.Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status409Conflict);

// Update endpoint - remove try-catch
products.MapPut("/{id}", async (int id, UpdateProductRequest request, IProductBusinessService productService) =>
{
    var product = await productService.UpdateAsync(id, request);
    return Results.Ok(product);
})
.WithName("UpdateProduct")
.Produces<Product>(StatusCodes.Status200OK)
.Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status409Conflict);

// Delete endpoint - remove try-catch
products.MapDelete("/{id}", async (int id, IProductBusinessService productService) =>
{
    await productService.DeleteAsync(id);
    return Results.NoContent();
})
.WithName("DeleteProduct")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);
```

### Build to Verify
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Expected**: Build succeeds

---

## Step 8.7: Update Unit Tests

### Update ProductBusinessServiceTests.cs

**Edit file**: `ServerApp.Tests/Services/ProductBusinessServiceTests.cs`

Update exception assertions to use new exception types:

```csharp
using ServerApp.Exceptions;

// Update GetByIdAsync_ThrowsNotFoundException test:
[Fact]
public async Task GetByIdAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
{
    // Arrange
    var id = 999;

    // Act & Assert
    await _sut.Invoking(s => s.GetByIdAsync(id))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Product with key '{id}' not found");
}

// Update CreateAsync duplicate name test:
[Fact]
public async Task CreateAsync_ThrowsDuplicateEntityException_WhenNameAlreadyExists()
{
    // Arrange
    var request = new CreateProductRequest
    {
        Name = "Duplicate Product",
        Description = "Test",
        Price = 10.00m,
        Stock = 5,
        CategoryId = 1
    };

    // Create first product
    await _sut.CreateAsync(request);

    // Act & Assert
    await _sut.Invoking(s => s.CreateAsync(request))
        .Should().ThrowAsync<DuplicateEntityException>()
        .WithMessage("*already exists*");
}
```

### Run Tests
```powershell
dotnet test ServerApp.Tests/ServerApp.Tests.csproj
```

**Expected**: All tests pass (may need to update a few more test assertions)

---

## Step 8.8: Commit Changes

### Stage and Commit
```powershell
git add .
git commit -m "Phase 8: Improve error handling

- Created custom exception types (NotFoundException, DuplicateEntityException, BusinessRuleException)
- Implemented GlobalExceptionMiddleware for centralized error handling
- Added correlation IDs to all requests via PerformanceMiddleware
- Refactored ValidationService to use custom exceptions
- Simplified ProductEndpoints error handling (middleware handles exceptions)
- Updated unit tests to assert new exception types
- All error responses now include correlation IDs for troubleshooting"
```

---

## Manual Testing Checkpoint ⏸️

### Test Error Handling

1. **Start the ServerApp**:
```powershell
cd ServerApp
dotnet run
```

2. **Test 404 Not Found**:
```powershell
# In another terminal
curl -X GET "http://localhost:5000/api/products/9999" -i
```

Expected response:
```json
HTTP/1.1 404 Not Found
X-Correlation-Id: <guid>

{
  "statusCode": 404,
  "message": "Product with key '9999' not found",
  "correlationId": "<same-guid>"
}
```

3. **Test 409 Conflict (Duplicate Name)**:
```powershell
# Create a product
curl -X POST "http://localhost:5000/api/products" `
  -H "Content-Type: application/json" `
  -d '{"name":"Test Product","description":"Test","price":10.00,"stock":5,"categoryId":1}'

# Try to create another with same name
curl -X POST "http://localhost:5000/api/products" `
  -H "Content-Type: application/json" `
  -d '{"name":"Test Product","description":"Test2","price":15.00,"stock":10,"categoryId":1}' -i
```

Expected response:
```json
HTTP/1.1 409 Conflict
X-Correlation-Id: <guid>

{
  "statusCode": 409,
  "message": "A product with the name 'Test Product' already exists",
  "property": "Name",
  "correlationId": "<same-guid>"
}
```

4. **Test 400 Validation Error**:
```powershell
curl -X POST "http://localhost:5000/api/products" `
  -H "Content-Type: application/json" `
  -d '{"name":"","description":"Test","price":-5,"stock":5,"categoryId":1}' -i
```

Expected response:
```json
HTTP/1.1 400 Bad Request
X-Correlation-Id: <guid>

{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "property": "Name",
      "error": "Name is required"
    },
    {
      "property": "Price",
      "error": "Price must be greater than 0"
    }
  ],
  "correlationId": "<same-guid>"
}
```

5. **Verify Correlation IDs in Logs**:
Check the ServerApp console output. You should see:
```
Request started: POST /api/products | CorrelationId: <guid>
Expected exception occurred: DuplicateEntityException | Message: A product with the name 'Test Product' already exists | CorrelationId: <guid>
Request completed: POST /api/products | Status: 409 | Duration: 45ms | CorrelationId: <guid>
```

**Human Response Required**:
- Type `pass` if all error responses include correlation IDs and proper status codes
- Type `fail` if any errors are not handled correctly

---

## Merge to Master

### After Manual Testing Passes
```powershell
# Stop the running server (Ctrl+C)

# Switch to master
git checkout master

# Merge feature branch
git merge refactor/phase8-error-handling --no-ff -m "Merge Phase 8: Improved Error Handling"

# Tag completion
git tag -a phase8-complete -m "Phase 8: Error handling with custom exceptions and middleware complete"

# Push to remote (if applicable)
git push origin master --tags
```

---

## Phase 8 Complete! ✅

### Summary of Changes
- **Files Created**: 4
  - `ServerApp/Exceptions/NotFoundException.cs`
  - `ServerApp/Exceptions/DuplicateEntityException.cs`
  - `ServerApp/Exceptions/BusinessRuleException.cs`
  - `ServerApp/Middleware/GlobalExceptionMiddleware.cs`

- **Files Modified**: 4
  - `ServerApp/Program.cs` (middleware registration)
  - `ServerApp/Middleware/PerformanceMiddleware.cs` (correlation IDs)
  - `ServerApp/Services/ValidationService.cs` (custom exceptions)
  - `ServerApp/Endpoints/ProductEndpoints.cs` (simplified error handling)
  - `ServerApp.Tests/Services/ProductBusinessServiceTests.cs` (updated assertions)

### Key Improvements
1. **Centralized Error Handling**: All exceptions caught by middleware
2. **Correlation IDs**: Every request/response includes unique identifier for troubleshooting
3. **Consistent Error Responses**: Standard JSON format for all errors
4. **Better Logging**: Correlation IDs in all log messages
5. **Simplified Endpoints**: No more try-catch blocks cluttering business logic
6. **Type-Safe Exceptions**: Custom exception types for different error scenarios

### Next Phase
Proceed to **Phase 9: Add Response Caching** (refactor-phase9-agent.md)

---

## Error Recovery

### If Build Fails
```powershell
# Check for compilation errors
dotnet build ServerApp/ServerApp.csproj --verbosity detailed

# If namespace issues
# Verify all using statements include ServerApp.Exceptions

# If middleware registration issues
# Verify GlobalExceptionMiddleware is registered BEFORE other middleware
```

### If Tests Fail
```powershell
# Run tests with detailed output
dotnet test ServerApp.Tests/ServerApp.Tests.csproj --verbosity detailed

# Update any tests still expecting KeyNotFoundException
# Replace with NotFoundException

# Update any tests expecting different error messages
# Use .WithMessage("*partial match*") for flexible matching
```

### If Middleware Not Working
```powershell
# Check Program.cs middleware order
# GlobalExceptionMiddleware MUST be first:
# app.UseMiddleware<GlobalExceptionMiddleware>();
# app.UseMiddleware<PerformanceMiddleware>();
# app.UseSwagger(); // etc.

# Restart the application completely
# Stop with Ctrl+C and run dotnet run again
```

---

**Document Version**: 1.0  
**Last Updated**: Phase 8 detailed instructions  
**Estimated Completion Time**: 2-3 hours  
**Risk Level**: Medium
