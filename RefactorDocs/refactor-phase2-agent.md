# Phase 2: Extract Business Logic - Agent Instructions

## Overview
This phase extracts business logic from ProductEndpoints.cs into a dedicated service layer, following single responsibility principle and improving testability.

**Estimated Time**: 4-6 hours  
**Risk Level**: High (major refactoring of core logic)  
**Rollback Strategy**: Git branch allows complete rollback  
**Prerequisites**: Phase 1 complete (SharedModels integrated)

---

## Pre-Phase Checklist

**Agent verifies**:
- [ ] On master branch
- [ ] Working directory clean
- [ ] Phase 1 complete (SharedModels in use)
- [ ] Solution builds successfully

**Agent actions**:
```powershell
cd "s:\Microsoft Full-Stack Developer Professional Certificate\07 Full-Stack Integration\Final Project\InventoryHub"
git status
git checkout master
git pull origin master  # If using remote
git checkout -b refactor/phase2-business-logic
```

**Agent reports**: "✅ Branch 'refactor/phase2-business-logic' created"

---

## Step 2.1: Create Business Service Interface

### Create IProductBusinessService.cs

**Agent creates**: `ServerApp/Services/IProductBusinessService.cs`

**Agent content**:
```csharp
using SharedModels;
using ServerApp.Models;

namespace ServerApp.Services;

/// <summary>
/// Business logic interface for product operations
/// </summary>
public interface IProductBusinessService
{
    /// <summary>
    /// Retrieves paginated list of products
    /// </summary>
    Task<PaginatedList<Product>> GetProductsAsync(PaginationParams paginationParams);
    
    /// <summary>
    /// Retrieves a single product by ID
    /// </summary>
    Task<Product?> GetProductByIdAsync(int id);
    
    /// <summary>
    /// Creates a new product with validation
    /// </summary>
    Task<Product> CreateProductAsync(CreateProductRequest request);
    
    /// <summary>
    /// Updates an existing product with validation
    /// </summary>
    Task<Product> UpdateProductAsync(int id, UpdateProductRequest request);
    
    /// <summary>
    /// Deletes a product by ID
    /// </summary>
    Task<bool> DeleteProductAsync(int id);
    
    /// <summary>
    /// Clears product cache
    /// </summary>
    void ClearCache();
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Services/IProductBusinessService.cs
git commit -m "Add IProductBusinessService interface"
```

**Agent reports**: "✅ Step 2.1 complete. Interface created."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 2.2? (yes/no)"

---

## Step 2.2: Create Business Service Implementation

### Create ProductBusinessService.cs

**Agent creates**: `ServerApp/Services/ProductBusinessService.cs`

**Agent content**:
```csharp
using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using ServerApp.Models;
using SharedModels;

namespace ServerApp.Services;

/// <summary>
/// Business logic implementation for product operations
/// </summary>
public class ProductBusinessService : IProductBusinessService
{
    private readonly AppDbContext _context;
    private readonly ValidationService _validationService;
    private readonly CacheService _cacheService;
    private readonly ILogger<ProductBusinessService> _logger;

    public ProductBusinessService(
        AppDbContext context,
        ValidationService validationService,
        CacheService cacheService,
        ILogger<ProductBusinessService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaginatedList<Product>> GetProductsAsync(PaginationParams paginationParams)
    {
        _logger.LogInformation("Retrieving products - Page: {Page}, PageSize: {PageSize}", 
            paginationParams.Page, paginationParams.PageSize);

        var query = _context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        
        var products = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} products", products.Count);

        return new PaginatedList<Product>(
            products,
            totalCount,
            paginationParams.Page,
            paginationParams.PageSize
        );
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving product with ID: {ProductId}", id);

        var cacheKey = $"product_{id}";
        var cachedProduct = _cacheService.Get<Product>(cacheKey);
        
        if (cachedProduct != null)
        {
            _logger.LogInformation("Product {ProductId} retrieved from cache", id);
            return cachedProduct;
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product != null)
        {
            _cacheService.Set(cacheKey, product);
            _logger.LogInformation("Product {ProductId} retrieved from database and cached", id);
        }
        else
        {
            _logger.LogWarning("Product {ProductId} not found", id);
        }

        return product;
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        _logger.LogInformation("Creating new product: {ProductName}", request.Name);

        // Validate request
        var validationErrors = _validationService.ValidateCreateProductRequest(request);
        if (validationErrors.Any())
        {
            _logger.LogWarning("Product creation validation failed for: {ProductName}", request.Name);
            throw new ValidationException("Product validation failed", validationErrors);
        }

        // Check for duplicate name
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Name == request.Name);
        
        if (existingProduct != null)
        {
            _logger.LogWarning("Product with name '{ProductName}' already exists", request.Name);
            throw new ValidationException("Product validation failed", 
                new Dictionary<string, string[]>
                {
                    { "Name", new[] { "A product with this name already exists" } }
                });
        }

        // Validate category exists
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId.Value);
            
            if (!categoryExists)
            {
                _logger.LogWarning("Category {CategoryId} not found", request.CategoryId.Value);
                throw new ValidationException("Product validation failed", 
                    new Dictionary<string, string[]>
                    {
                        { "CategoryId", new[] { "The specified category does not exist" } }
                    });
            }
        }

        // Create product
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Load category for response
        if (product.CategoryId.HasValue)
        {
            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();
        }

        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

        // Clear cache
        ClearCache();

        return product;
    }

    public async Task<Product> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        _logger.LogInformation("Updating product {ProductId}", id);

        // Find existing product
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found for update", id);
            throw new KeyNotFoundException($"Product with ID {id} not found");
        }

        // Validate request
        var validationErrors = _validationService.ValidateUpdateProductRequest(request);
        if (validationErrors.Any())
        {
            _logger.LogWarning("Product update validation failed for ID: {ProductId}", id);
            throw new ValidationException("Product validation failed", validationErrors);
        }

        // Check for duplicate name (excluding current product)
        var duplicateName = await _context.Products
            .AnyAsync(p => p.Name == request.Name && p.Id != id);
        
        if (duplicateName)
        {
            _logger.LogWarning("Product name '{ProductName}' already in use", request.Name);
            throw new ValidationException("Product validation failed", 
                new Dictionary<string, string[]>
                {
                    { "Name", new[] { "A product with this name already exists" } }
                });
        }

        // Validate category if changed
        if (request.CategoryId.HasValue && request.CategoryId != product.CategoryId)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId.Value);
            
            if (!categoryExists)
            {
                _logger.LogWarning("Category {CategoryId} not found", request.CategoryId.Value);
                throw new ValidationException("Product validation failed", 
                    new Dictionary<string, string[]>
                    {
                        { "CategoryId", new[] { "The specified category does not exist" } }
                    });
            }
        }

        // Update product
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload category if changed
        if (product.CategoryId.HasValue)
        {
            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();
        }

        _logger.LogInformation("Product {ProductId} updated successfully", id);

        // Clear cache
        ClearCache();

        return product;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        _logger.LogInformation("Deleting product {ProductId}", id);

        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found for deletion", id);
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deleted successfully", id);

        // Clear cache
        ClearCache();

        return true;
    }

    public void ClearCache()
    {
        _cacheService.ClearByPrefix("product_");
        _logger.LogInformation("Product cache cleared");
    }
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Services/ProductBusinessService.cs
git commit -m "Add ProductBusinessService implementation"
```

**Agent reports**: "✅ Step 2.2 complete. Service implementation created."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 2.3? (yes/no)"

---

## Step 2.3: Register Service in DI Container

**Agent reads**: `ServerApp/Program.cs`

**Agent finds** the service registration section (after `builder.Services.AddScoped<ValidationService>()`)

**Agent adds**:
```csharp
builder.Services.AddScoped<IProductBusinessService, ProductBusinessService>();
```

**Example context**:
```csharp
builder.Services.AddScoped<CacheService>();
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<IProductBusinessService, ProductBusinessService>(); // Add this line
builder.Services.AddScoped<DbInitializerService>();
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Program.cs
git commit -m "Register ProductBusinessService in DI container"
```

**Agent reports**: "✅ Step 2.3 complete. Service registered."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 2.4? (yes/no)"

---

## Step 2.4: Refactor Endpoints (One by One)

### Step 2.4.1: Refactor GET /products Endpoint

**Agent reads**: `ServerApp/Endpoints/ProductEndpoints.cs`

**Agent finds MapGet for `/products`** and refactors it

**OLD CODE**:
```csharp
products.MapGet("/", async (
    AppDbContext context,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10) =>
{
    var totalCount = await context.Products.CountAsync();
    var items = await context.Products
        .Include(p => p.Category)
        .OrderBy(p => p.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Results.Ok(new PaginatedList<Product>(items, totalCount, page, pageSize));
})
```

**NEW CODE**:
```csharp
products.MapGet("/", async (
    IProductBusinessService businessService,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10) =>
{
    var paginationParams = new PaginationParams { Page = page, PageSize = pageSize };
    var result = await businessService.GetProductsAsync(paginationParams);
    return Results.Ok(result);
})
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Refactor GET /products to use business service"
```

**Agent reports**: "✅ GET /products refactored"

---

### Step 2.4.2: Refactor GET /products/{id} Endpoint

**Agent finds MapGet for `/products/{id}`**

**OLD CODE**:
```csharp
products.MapGet("/{id}", async (int id, AppDbContext context, CacheService cacheService) =>
{
    var cacheKey = $"product_{id}";
    var cachedProduct = cacheService.Get<Product>(cacheKey);
    if (cachedProduct != null)
    {
        return Results.Ok(cachedProduct);
    }

    var product = await context.Products
        .Include(p => p.Category)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound();
    }

    cacheService.Set(cacheKey, product);
    return Results.Ok(product);
})
```

**NEW CODE**:
```csharp
products.MapGet("/{id}", async (int id, IProductBusinessService businessService) =>
{
    var product = await businessService.GetProductByIdAsync(id);
    return product == null ? Results.NotFound() : Results.Ok(product);
})
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Refactor GET /products/{id} to use business service"
```

**Agent reports**: "✅ GET /products/{id} refactored"

---

### Step 2.4.3: Refactor POST /products Endpoint

**Agent finds MapPost for `/products`**

**OLD CODE** (extensive validation and business logic)

**NEW CODE**:
```csharp
products.MapPost("/", async (
    CreateProductRequest request,
    IProductBusinessService businessService) =>
{
    try
    {
        var product = await businessService.CreateProductAsync(request);
        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new ValidationProblemDetails
        {
            Title = ex.Message,
            Errors = ex.Errors
        });
    }
})
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Refactor POST /products to use business service"
```

**Agent reports**: "✅ POST /products refactored"

**Agent asks**: "⏸️ CHECKPOINT: Test POST endpoint in Swagger? (pass/fail/skip)"

---

### Step 2.4.4: Refactor PUT /products/{id} Endpoint

**Agent finds MapPut for `/products/{id}`**

**OLD CODE** (extensive validation and update logic)

**NEW CODE**:
```csharp
products.MapPut("/{id}", async (
    int id,
    UpdateProductRequest request,
    IProductBusinessService businessService) =>
{
    try
    {
        var product = await businessService.UpdateProductAsync(id, request);
        return Results.Ok(product);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new ValidationProblemDetails
        {
            Title = ex.Message,
            Errors = ex.Errors
        });
    }
})
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Refactor PUT /products/{id} to use business service"
```

**Agent reports**: "✅ PUT /products/{id} refactored"

---

### Step 2.4.5: Refactor DELETE /products/{id} Endpoint

**Agent finds MapDelete for `/products/{id}`**

**OLD CODE**:
```csharp
products.MapDelete("/{id}", async (int id, AppDbContext context, CacheService cacheService) =>
{
    var product = await context.Products.FindAsync(id);
    if (product == null)
    {
        return Results.NotFound();
    }

    context.Products.Remove(product);
    await context.SaveChangesAsync();
    cacheService.ClearByPrefix("product_");

    return Results.NoContent();
})
```

**NEW CODE**:
```csharp
products.MapDelete("/{id}", async (int id, IProductBusinessService businessService) =>
{
    var deleted = await businessService.DeleteProductAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Refactor DELETE /products/{id} to use business service"
```

**Agent reports**: "✅ DELETE /products/{id} refactored"

**Agent reports**: "✅ Step 2.4 complete. All 5 endpoints refactored."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 2.5? (yes/no)"

---

## Step 2.5: Clean Up ProductEndpoints.cs

**Agent reads**: `ServerApp/Endpoints/ProductEndpoints.cs`

**Agent removes** unused using statements:
- `using Microsoft.EntityFrameworkCore;` (if no longer used)
- `using ServerApp.Data;` (AppDbContext no longer directly injected)

**Agent verifies** remaining using statements are necessary

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Clean up ProductEndpoints - remove unused dependencies"
```

**Agent reports**: "✅ Step 2.5 complete. Endpoints cleaned up."

---

## Step 2.6: Full Solution Build

**Agent executes**:
```powershell
dotnet clean InventoryHub.sln
dotnet build InventoryHub.sln
```

**Agent analyzes output**:
- Count errors: Should be 0
- Count warnings: Document for review

**If build succeeds**:
```powershell
git add .
git commit -m "Phase 2 complete: Business logic extracted to service layer"
```

**Agent reports**: 
```
✅ Step 2.6 complete
- Solution builds successfully
- 0 errors, X warnings
- Ready for integration testing
```

---

## Step 2.7: Integration Testing (MANUAL CHECKPOINT)

**Agent reports**: "⏸️ COMPREHENSIVE TESTING REQUIRED"

**Agent provides instructions**:

```
==========================================
CHECKPOINT: Integration Testing
==========================================

Please perform comprehensive API testing:

1. START SERVERAPP:
   - Open terminal in ServerApp folder
   - Run: dotnet run
   - Navigate to: http://localhost:5132/swagger

2. TEST ALL ENDPOINTS:

   A. GET /api/products
      - Verify: Returns paginated list
      - Test: page=1, pageSize=5
      - Test: page=2, pageSize=10
      - Expected: Correct pagination metadata

   B. GET /api/products/{id}
      - Test with valid ID (e.g., 1)
      - Expected: Product with category
      - Test with invalid ID (e.g., 999)
      - Expected: 404 Not Found

   C. POST /api/products
      - Test valid product:
        {
          "name": "Test Product",
          "description": "Test Description",
          "price": 29.99,
          "stock": 50,
          "categoryId": 1
        }
      - Expected: 201 Created with product details
      
      - Test invalid product (empty name):
        {
          "name": "",
          "description": "Test",
          "price": 10,
          "stock": 5
        }
      - Expected: 400 Bad Request with validation errors
      
      - Test duplicate name:
        Use name from existing product
      - Expected: 400 Bad Request

   D. PUT /api/products/{id}
      - Test valid update to product created above
      - Expected: 200 OK with updated product
      
      - Test invalid ID (e.g., 999)
      - Expected: 404 Not Found
      
      - Test invalid data (negative price)
      - Expected: 400 Bad Request

   E. DELETE /api/products/{id}
      - Test with valid ID (product created above)
      - Expected: 204 No Content
      
      - Verify deletion (GET same ID)
      - Expected: 404 Not Found

3. TEST CLIENTAPP INTEGRATION:
   - Open new terminal in ClientApp folder
   - Run: dotnet run
   - Navigate to URL shown
   - Test all CRUD operations through UI
   - Verify no console errors

4. TEST CACHING:
   - GET product by ID twice
   - Check ServerApp logs for cache hit
   - Update product
   - GET same product again
   - Verify updated data returned

5. TEST ERROR HANDLING:
   - Create product with invalid category ID
   - Expected: Proper validation error
   - Update non-existent product
   - Expected: 404 response

==========================================
Please report results:
- Type "pass" if all tests successful
- Type "fail: [description]" if issues found
==========================================
```

**Agent waits for**: Human response

**On "pass"**: Agent continues to Step 2.8

**On "fail: [description]"**: 
- Agent logs issue
- Agent asks: "Investigate? (yes/rollback/manual)"

---

## Step 2.8: Update Checklist

**Agent updates**: `REFACTORING_CHECKLIST.md`

**Agent marks Phase 2 complete**:
```markdown
## ✅ Phase 2: Extract Business Logic (100% Complete)

**Completed**: [Current Date]

- [x] Create IProductBusinessService interface
- [x] Create ProductBusinessService implementation
- [x] Register service in DI container
- [x] Refactor GET /products endpoint
- [x] Refactor GET /products/{id} endpoint
- [x] Refactor POST /products endpoint
- [x] Refactor PUT /products/{id} endpoint
- [x] Refactor DELETE /products/{id} endpoint
- [x] Clean up ProductEndpoints.cs
- [x] Test all endpoints via Swagger
- [x] Test integration with ClientApp
- [x] Verify caching still works
- [x] Verify validation still works
```

**Agent commits**:
```powershell
git add REFACTORING_CHECKLIST.md
git commit -m "Update checklist: Phase 2 complete"
```

**Agent reports**: "✅ Checklist updated"

---

## Step 2.9: Merge Decision

**Agent asks**: "⏸️ DECISION POINT: Merge Phase 2 to master? (yes/no/later)"

**On "yes"**:
```powershell
git checkout master
git merge refactor/phase2-business-logic --no-ff
git tag phase2-complete
```

**Agent reports**: 
```
✅ Phase 2 Complete!

Summary:
- Branch: refactor/phase2-business-logic merged to master
- Tag: phase2-complete created
- Commits: [count]
- Files created: 2 (IProductBusinessService.cs, ProductBusinessService.cs)
- Files modified: 2 (ProductEndpoints.cs, Program.cs)
- Lines of code: ~400 added
- Status: SUCCESS

Benefits Achieved:
✅ Separated business logic from HTTP handling
✅ Improved testability (can unit test service independently)
✅ Better code organization and maintainability
✅ Single Responsibility Principle applied
✅ Easier to add new business rules in future

Next Phase: Phase 3 - Consolidate Validation Logic
```

**Agent asks**: "⏸️ Push to remote? (yes/no/later)"

**Agent asks**: "⏸️ Start Phase 3? (yes/no/later)"

---

## Error Recovery

### Compilation Errors

**If ValidationException not found**:
- Agent checks if it exists in ServerApp/Models
- If not, agent creates it or asks for guidance

**If build fails**:
```
❌ BUILD FAILED
Step: [step number]
Error: [error message]
Likely cause: [analysis]
Action: Run `dotnet restore` and retry
```

### Runtime Errors

**If service injection fails**:
- Verify service registration in Program.cs
- Check interface and implementation namespaces match

---

## Success Metrics

**Phase 2 is successful when**:
- ✅ All projects build without errors
- ✅ Business service interface and implementation created
- ✅ Service registered in DI container
- ✅ All 5 endpoints refactored to use service
- ✅ ProductEndpoints.cs simplified (no direct DB access)
- ✅ All CRUD operations work via Swagger
- ✅ ClientApp integration still works
- ✅ Caching functionality preserved
- ✅ Validation functionality preserved
- ✅ Proper error responses maintained

---

## Files Modified Summary

**Created** (2 files):
- ServerApp/Services/IProductBusinessService.cs
- ServerApp/Services/ProductBusinessService.cs

**Modified** (3 files):
- ServerApp/Endpoints/ProductEndpoints.cs (major refactoring)
- ServerApp/Program.cs (service registration)
- REFACTORING_CHECKLIST.md (progress update)

**Total**: 5 files affected

---

**Document Version**: 1.0  
**Phase**: 2 of 10  
**Prerequisites**: Phase 1 complete (SharedModels)  
**Next Phase**: refactor-phase3-agent.md (Consolidate Validation)
