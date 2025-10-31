# Phase 3: Consolidate Validation Logic - Agent Instructions

## Overview
This phase consolidates scattered validation logic into the existing ValidationService, creating a single source of truth for validation rules.

**Estimated Time**: 2-3 hours  
**Risk Level**: Medium  
**Rollback Strategy**: Git branch allows complete rollback  
**Prerequisites**: Phase 2 complete (Business service layer exists)

---

## Pre-Phase Checklist

**Agent verifies**:
- [ ] On master branch
- [ ] Working directory clean
- [ ] Phases 1-2 complete
- [ ] Solution builds successfully

**Agent actions**:
```powershell
cd "s:\Microsoft Full-Stack Developer Professional Certificate\07 Full-Stack Integration\Final Project\InventoryHub"
git status
git checkout master
git checkout -b refactor/phase3-validation
```

**Agent reports**: "✅ Branch 'refactor/phase3-validation' created"

---

## Step 3.1: Analyze Current ValidationService

**Agent reads**: `ServerApp/Services/ValidationService.cs` (full file)

**Agent reports**: Current validation methods found

**Expected methods**:
- `ValidateCreateProductRequest(CreateProductRequest request)`
- `ValidateUpdateProductRequest(UpdateProductRequest request)`

**Agent asks**: "⏸️ CHECKPOINT: Continue to enhance validation? (yes/no)"

---

## Step 3.2: Enhance ValidationService

### Add Comprehensive Validation Methods

**Agent reads**: `ServerApp/Services/ValidationService.cs`

**Agent enhances** with additional validation methods:

**Agent adds** after existing methods:

```csharp
/// <summary>
/// Validates pagination parameters
/// </summary>
public Dictionary<string, string[]> ValidatePaginationParams(PaginationParams paginationParams)
{
    var errors = new Dictionary<string, string[]>();

    if (paginationParams.Page < 1)
    {
        errors.Add(nameof(paginationParams.Page), new[] { "Page must be greater than or equal to 1" });
    }

    if (paginationParams.PageSize < 1)
    {
        errors.Add(nameof(paginationParams.PageSize), new[] { "PageSize must be greater than or equal to 1" });
    }

    if (paginationParams.PageSize > 100)
    {
        errors.Add(nameof(paginationParams.PageSize), new[] { "PageSize cannot exceed 100" });
    }

    return errors;
}

/// <summary>
/// Validates product ID
/// </summary>
public Dictionary<string, string[]> ValidateProductId(int id)
{
    var errors = new Dictionary<string, string[]>();

    if (id <= 0)
    {
        errors.Add("Id", new[] { "Product ID must be greater than 0" });
    }

    return errors;
}

/// <summary>
/// Validates product name is unique (excluding specified ID)
/// </summary>
public async Task<bool> IsProductNameUniqueAsync(string name, int? excludeId = null)
{
    if (excludeId.HasValue)
    {
        return !await _context.Products.AnyAsync(p => p.Name == name && p.Id != excludeId.Value);
    }
    
    return !await _context.Products.AnyAsync(p => p.Name == name);
}

/// <summary>
/// Validates category exists
/// </summary>
public async Task<bool> CategoryExistsAsync(int categoryId)
{
    return await _context.Categories.AnyAsync(c => c.Id == categoryId);
}

/// <summary>
/// Validates product price is within acceptable range
/// </summary>
public Dictionary<string, string[]> ValidatePrice(decimal price)
{
    var errors = new Dictionary<string, string[]>();

    if (price < 0)
    {
        errors.Add("Price", new[] { "Price cannot be negative" });
    }

    if (price > 1000000)
    {
        errors.Add("Price", new[] { "Price cannot exceed 1,000,000" });
    }

    return errors;
}

/// <summary>
/// Validates product stock level
/// </summary>
public Dictionary<string, string[]> ValidateStock(int stock)
{
    var errors = new Dictionary<string, string[]>();

    if (stock < 0)
    {
        errors.Add("Stock", new[] { "Stock cannot be negative" });
    }

    if (stock > 100000)
    {
        errors.Add("Stock", new[] { "Stock cannot exceed 100,000" });
    }

    return errors;
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Services/ValidationService.cs
git commit -m "Enhance ValidationService with additional validation methods"
```

**Agent reports**: "✅ Step 3.2 complete. ValidationService enhanced."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 3.3? (yes/no)"

---

## Step 3.3: Update ProductBusinessService to Use Enhanced Validation

**Agent reads**: `ServerApp/Services/ProductBusinessService.cs`

**Agent updates GetProductsAsync method** to validate pagination:

**Find**:
```csharp
public async Task<PaginatedList<Product>> GetProductsAsync(PaginationParams paginationParams)
{
    _logger.LogInformation("Retrieving products - Page: {Page}, PageSize: {PageSize}", 
        paginationParams.Page, paginationParams.PageSize);
```

**Replace with**:
```csharp
public async Task<PaginatedList<Product>> GetProductsAsync(PaginationParams paginationParams)
{
    // Validate pagination parameters
    var validationErrors = _validationService.ValidatePaginationParams(paginationParams);
    if (validationErrors.Any())
    {
        _logger.LogWarning("Invalid pagination parameters");
        throw new ValidationException("Pagination validation failed", validationErrors);
    }

    _logger.LogInformation("Retrieving products - Page: {Page}, PageSize: {PageSize}", 
        paginationParams.Page, paginationParams.PageSize);
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Services/ProductBusinessService.cs
git commit -m "Add pagination validation to GetProductsAsync"
```

**Agent reports**: "✅ Pagination validation added"

---

### Update GetProductByIdAsync

**Agent updates GetProductByIdAsync** method:

**Find**:
```csharp
public async Task<Product?> GetProductByIdAsync(int id)
{
    _logger.LogInformation("Retrieving product with ID: {ProductId}", id);
```

**Replace with**:
```csharp
public async Task<Product?> GetProductByIdAsync(int id)
{
    // Validate product ID
    var validationErrors = _validationService.ValidateProductId(id);
    if (validationErrors.Any())
    {
        _logger.LogWarning("Invalid product ID: {ProductId}", id);
        throw new ValidationException("Product ID validation failed", validationErrors);
    }

    _logger.LogInformation("Retrieving product with ID: {ProductId}", id);
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Services/ProductBusinessService.cs
git commit -m "Add ID validation to GetProductByIdAsync"
```

**Agent reports**: "✅ ID validation added"

---

### Update CreateProductAsync - Use Async Validation

**Agent finds** duplicate name check in `CreateProductAsync`:

**Find**:
```csharp
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
```

**Replace with**:
```csharp
// Check for duplicate name
var isNameUnique = await _validationService.IsProductNameUniqueAsync(request.Name);
if (!isNameUnique)
{
    _logger.LogWarning("Product with name '{ProductName}' already exists", request.Name);
    throw new ValidationException("Product validation failed", 
        new Dictionary<string, string[]>
        {
            { "Name", new[] { "A product with this name already exists" } }
        });
}
```

**Agent finds** category validation:

**Find**:
```csharp
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
```

**Replace with**:
```csharp
// Validate category exists
if (request.CategoryId.HasValue)
{
    var categoryExists = await _validationService.CategoryExistsAsync(request.CategoryId.Value);
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
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Services/ProductBusinessService.cs
git commit -m "Refactor CreateProductAsync to use ValidationService methods"
```

**Agent reports**: "✅ CreateProductAsync refactored"

---

### Update UpdateProductAsync - Use Async Validation

**Agent finds** duplicate name check in `UpdateProductAsync`:

**Find**:
```csharp
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
```

**Replace with**:
```csharp
// Check for duplicate name (excluding current product)
var isNameUnique = await _validationService.IsProductNameUniqueAsync(request.Name, id);
if (!isNameUnique)
{
    _logger.LogWarning("Product name '{ProductName}' already in use", request.Name);
    throw new ValidationException("Product validation failed", 
        new Dictionary<string, string[]>
        {
            { "Name", new[] { "A product with this name already exists" } }
        });
}
```

**Agent finds** category validation in UpdateProductAsync:

**Replace** with ValidationService.CategoryExistsAsync call (same pattern as Create)

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Services/ProductBusinessService.cs
git commit -m "Refactor UpdateProductAsync to use ValidationService methods"
```

**Agent reports**: "✅ UpdateProductAsync refactored"

**Agent reports**: "✅ Step 3.3 complete. ProductBusinessService now uses consolidated validation."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 3.4? (yes/no)"

---

## Step 3.4: Remove Direct DbContext Access from ProductBusinessService

**Agent verifies**: ProductBusinessService now only uses ValidationService for existence checks

**Agent checks** if there are any remaining direct `_context.Products.AnyAsync` or similar calls outside of core CRUD operations

**Expected state**:
- ✅ Pagination validation via ValidationService
- ✅ ID validation via ValidationService
- ✅ Name uniqueness via ValidationService
- ✅ Category existence via ValidationService
- ✅ Direct `_context` only for CRUD (Get, Create, Update, Delete actual data)

**Agent reports**: "✅ Step 3.4 complete. Validation properly delegated."

---

## Step 3.5: Update Endpoints to Handle Validation Errors

**Agent reads**: `ServerApp/Endpoints/ProductEndpoints.cs`

**Agent checks** GET /products endpoint:

**Current**:
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

**Agent wraps** with try-catch:

```csharp
products.MapGet("/", async (
    IProductBusinessService businessService,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10) =>
{
    try
    {
        var paginationParams = new PaginationParams { Page = page, PageSize = pageSize };
        var result = await businessService.GetProductsAsync(paginationParams);
        return Results.Ok(result);
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
git commit -m "Add validation error handling to GET /products"
```

**Agent reports**: "✅ GET /products error handling added"

---

**Agent updates** GET /products/{id} endpoint similarly:

```csharp
products.MapGet("/{id}", async (int id, IProductBusinessService businessService) =>
{
    try
    {
        var product = await businessService.GetProductByIdAsync(id);
        return product == null ? Results.NotFound() : Results.Ok(product);
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
git commit -m "Add validation error handling to GET /products/{id}"
```

**Agent reports**: "✅ GET /products/{id} error handling added"

**Agent reports**: "✅ Step 3.5 complete. All endpoints handle validation errors."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 3.6? (yes/no)"

---

## Step 3.6: Full Solution Build

**Agent executes**:
```powershell
dotnet clean InventoryHub.sln
dotnet build InventoryHub.sln
```

**Agent verifies**: 0 errors

**If build succeeds**:
```powershell
git add .
git commit -m "Phase 3 complete: Validation logic consolidated"
```

**Agent reports**: "✅ Step 3.6 complete. Solution builds successfully."

---

## Step 3.7: Integration Testing (MANUAL CHECKPOINT)

**Agent reports**: "⏸️ VALIDATION TESTING REQUIRED"

**Agent provides instructions**:

```
==========================================
CHECKPOINT: Validation Testing
==========================================

Please test validation scenarios:

1. START SERVERAPP:
   - Run: dotnet run (in ServerApp folder)
   - Open: http://localhost:5132/swagger

2. TEST PAGINATION VALIDATION:
   
   A. GET /api/products?page=0&pageSize=10
      - Expected: 400 Bad Request
      - Error: "Page must be greater than or equal to 1"
   
   B. GET /api/products?page=1&pageSize=0
      - Expected: 400 Bad Request
      - Error: "PageSize must be greater than or equal to 1"
   
   C. GET /api/products?page=1&pageSize=101
      - Expected: 400 Bad Request
      - Error: "PageSize cannot exceed 100"
   
   D. GET /api/products?page=1&pageSize=20
      - Expected: 200 OK (valid request)

3. TEST ID VALIDATION:
   
   A. GET /api/products/0
      - Expected: 400 Bad Request
      - Error: "Product ID must be greater than 0"
   
   B. GET /api/products/-1
      - Expected: 400 Bad Request
   
   C. GET /api/products/1
      - Expected: 200 OK (if product exists) or 404 (if not)

4. TEST CREATE PRODUCT VALIDATION:
   
   A. Empty name:
      {
        "name": "",
        "price": 10,
        "stock": 5
      }
      - Expected: 400 Bad Request
   
   B. Duplicate name:
      Use existing product name
      - Expected: 400 Bad Request
      - Error: "A product with this name already exists"
   
   C. Invalid category:
      {
        "name": "Test",
        "price": 10,
        "stock": 5,
        "categoryId": 9999
      }
      - Expected: 400 Bad Request
      - Error: "The specified category does not exist"
   
   D. Negative price:
      {
        "name": "Test",
        "price": -10,
        "stock": 5
      }
      - Expected: 400 Bad Request

5. TEST UPDATE VALIDATION:
   
   Similar tests as create, plus:
   
   A. Update with duplicate name:
      - Expected: 400 Bad Request
   
   B. Update non-existent product (ID 9999):
      - Expected: 404 Not Found

6. VERIFY EXISTING FUNCTIONALITY:
   - Create valid product: Should work
   - Update valid product: Should work
   - Delete product: Should work
   - Browse products in ClientApp: Should work

==========================================
Please report results:
- Type "pass" if all validation tests successful
- Type "fail: [description]" if issues found
==========================================
```

**Agent waits for**: Human response

---

## Step 3.8: Update Checklist

**Agent updates**: `REFACTORING_CHECKLIST.md`

```markdown
## ✅ Phase 3: Consolidate Validation Logic (100% Complete)

**Completed**: [Current Date]

- [x] Enhance ValidationService with new methods
- [x] Add pagination validation
- [x] Add ID validation
- [x] Add async name uniqueness check
- [x] Add async category existence check
- [x] Add price validation
- [x] Add stock validation
- [x] Refactor ProductBusinessService to use ValidationService
- [x] Update all endpoints to handle validation errors
- [x] Test pagination validation
- [x] Test ID validation
- [x] Test create/update validation
- [x] Verify no regression in existing functionality
```

**Agent commits**:
```powershell
git add REFACTORING_CHECKLIST.md
git commit -m "Update checklist: Phase 3 complete"
```

**Agent reports**: "✅ Checklist updated"

---

## Step 3.9: Merge Decision

**Agent asks**: "⏸️ DECISION POINT: Merge Phase 3 to master? (yes/no/later)"

**On "yes"**:
```powershell
git checkout master
git merge refactor/phase3-validation --no-ff
git tag phase3-complete
```

**Agent reports**: 
```
✅ Phase 3 Complete!

Summary:
- Branch: refactor/phase3-validation merged to master
- Tag: phase3-complete created
- Commits: [count]
- Files modified: 3
  - ValidationService.cs (enhanced)
  - ProductBusinessService.cs (refactored)
  - ProductEndpoints.cs (error handling)
- Lines added: ~150
- Status: SUCCESS

Benefits Achieved:
✅ Single source of truth for validation
✅ Reusable validation methods
✅ Consistent validation across all endpoints
✅ Better error messages
✅ Easier to maintain and extend validation rules
✅ Testable validation logic

Next Phase: Phase 4 - Extract Magic Numbers/Strings
```

**Agent asks**: "⏸️ Push to remote? (yes/no/later)"

**Agent asks**: "⏸️ Start Phase 4? (yes/no/later)"

---

## Error Recovery

### If ValidationException Not Found

**Agent checks**:
```powershell
Test-Path "ServerApp/Models/ValidationException.cs"
```

**If missing**, agent asks: "ValidationException class not found. Should I create it? (yes/no)"

**On "yes"**, agent creates:
```csharp
namespace ServerApp.Models;

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message, Dictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }
}
```

---

## Success Metrics

**Phase 3 is successful when**:
- ✅ ValidationService has comprehensive validation methods
- ✅ All validation logic centralized (no scattered validations)
- ✅ ProductBusinessService uses ValidationService exclusively
- ✅ Endpoints properly handle validation errors
- ✅ Invalid pagination returns 400 with clear errors
- ✅ Invalid IDs return 400 with clear errors
- ✅ Duplicate names detected and rejected
- ✅ Invalid categories detected and rejected
- ✅ All existing functionality preserved

---

## Files Modified Summary

**Modified** (4 files):
- ServerApp/Services/ValidationService.cs (enhanced with 7 new methods)
- ServerApp/Services/ProductBusinessService.cs (refactored validation)
- ServerApp/Endpoints/ProductEndpoints.cs (error handling)
- REFACTORING_CHECKLIST.md (progress update)

**Total**: 4 files affected

---

**Document Version**: 1.0  
**Phase**: 3 of 10  
**Prerequisites**: Phases 1-2 complete  
**Next Phase**: refactor-phase4-agent.md (Extract Constants)
