# InventoryHub Refactoring Execution Instructions

## Overview
This document provides step-by-step instructions for executing the complete refactoring plan with built-in verification at every step. Each phase includes git branching, incremental builds, and error correction procedures.

## Prerequisites
- Git repository initialized and clean working directory
- .NET 9.0 SDK installed
- Visual Studio or VS Code with C# extensions
- Terminal/PowerShell access

## General Workflow Pattern

For each phase:
1. Create feature branch
2. Make changes incrementally
3. Build after each significant change
4. Fix errors immediately
5. Test functionality
6. Commit changes
7. Merge to main (or keep branch for review)

---

# PHASE 1: SharedModels Integration

## Branch Setup
```powershell
# Ensure main is clean
cd "s:\Microsoft Full-Stack Developer Professional Certificate\07 Full-Stack Integration\Final Project\InventoryHub"
git status
git checkout master
git pull

# Create feature branch
git checkout -b refactor/phase1-shared-models
```

## Step 1.1: Add Project References

```powershell
# Add ClientApp reference to SharedModels
dotnet add ClientApp/ClientApp.csproj reference SharedModels/SharedModels.csproj

# Verify reference added
dotnet list ClientApp/ClientApp.csproj reference

# Build to verify
dotnet build ClientApp/ClientApp.csproj

# If build succeeds, commit
git add ClientApp/ClientApp.csproj
git commit -m "Add SharedModels reference to ClientApp"
```

**Expected Output**: Build should succeed (SharedModels not used yet, so no breaking changes)

```powershell
# Add ServerApp reference to SharedModels
dotnet add ServerApp/ServerApp.csproj reference SharedModels/SharedModels.csproj

# Verify reference added
dotnet list ServerApp/ServerApp.csproj reference

# Build to verify
dotnet build ServerApp/ServerApp.csproj

# If build succeeds, commit
git add ServerApp/ServerApp.csproj
git commit -m "Add SharedModels reference to ServerApp"
```

## Step 1.2: Update ClientApp Using Statements

### Update ClientApp/Services/ProductService.cs
```powershell
# Open file in editor
code ClientApp/Services/ProductService.cs
```

**Changes to make**:
1. Find: `using ClientApp.Models;`
2. Replace with: `using SharedModels;`

**Save and build**:
```powershell
dotnet build ClientApp/ClientApp.csproj
```

**If errors occur**: Review error messages, ensure SharedModels has all required types. Fix and rebuild.

**If build succeeds**:
```powershell
git add ClientApp/Services/ProductService.cs
git commit -m "Update ProductService to use SharedModels"
```

### Update ClientApp/Services/ProductsStateService.cs
```powershell
code ClientApp/Services/ProductsStateService.cs
```

**Changes to make**:
1. Check if it uses `ClientApp.Models` - if not, skip this file
2. If yes, replace with `using SharedModels;`

**Build and commit** (same pattern as above)

### Update ClientApp/Pages/*.razor files

For each Razor file that uses models:
- `Pages/Products.razor`
- `Pages/ProductDetails.razor`
- `Pages/CreateProduct.razor`
- `Pages/EditProduct.razor`
- `Pages/DeleteProduct.razor`

**For each file**:
```powershell
code ClientApp/Pages/Products.razor
```

**Changes to make**:
1. Find: `@using ClientApp.Models`
2. Replace with: `@using SharedModels`

**After ALL Razor files are updated, build**:
```powershell
dotnet build ClientApp/ClientApp.csproj
```

**If build succeeds**:
```powershell
git add ClientApp/Pages/*.razor
git commit -m "Update all Razor pages to use SharedModels"
```

### Update ClientApp/Shared/*.razor components

Check these files:
- `Shared/ProductCard.razor`
- `Shared/ProductForm.razor`
- `Shared/ErrorAlert.razor` (may not need changes)
- `Shared/ToastContainer.razor` (may not need changes)

**Same pattern**: Update `@using` directives, build, commit.

### Final ClientApp Build
```powershell
# Clean build to ensure everything works
dotnet clean ClientApp/ClientApp.csproj
dotnet build ClientApp/ClientApp.csproj
```

**If successful**:
```powershell
git add ClientApp/
git commit -m "Complete ClientApp migration to SharedModels"
```

## Step 1.3: Update ServerApp Using Statements

### Update ServerApp/Endpoints/ProductEndpoints.cs
```powershell
code ServerApp/Endpoints/ProductEndpoints.cs
```

**Changes to make**:
1. Find: `using ServerApp.Models;`
2. Add above it: `using SharedModels;`
3. Note: Keep `ServerApp.Models` for now (has non-shared types like PaginationParams)

**Build**:
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Commit if successful**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Update ProductEndpoints to use SharedModels"
```

### Update ServerApp/Data/AppDbContext.cs
```powershell
code ServerApp/Data/AppDbContext.cs
```

**Changes to make**:
1. Find: `using ServerApp.Models;`
2. Add: `using SharedModels;`

**Build and commit**:
```powershell
dotnet build ServerApp/ServerApp.csproj
git add ServerApp/Data/AppDbContext.cs
git commit -m "Update AppDbContext to use SharedModels"
```

### Update ServerApp/Services/*.cs files

For each service file:
- `Services/CacheService.cs`
- `Services/DbInitializerService.cs`
- `Services/SeedingService.cs`
- `Services/ValidationService.cs`

**Pattern for each**:
```powershell
code ServerApp/Services/CacheService.cs
# Add: using SharedModels;
# Build
dotnet build ServerApp/ServerApp.csproj
# Commit
git add ServerApp/Services/CacheService.cs
git commit -m "Update CacheService to use SharedModels"
```

### Update ServerApp/Program.cs
```powershell
code ServerApp/Program.cs
```

**Changes to make**:
1. Add: `using SharedModels;` at the top
2. Check for any `ServerApp.Models` references to Product/Category

**Build and commit**:
```powershell
dotnet build ServerApp/ServerApp.csproj
git add ServerApp/Program.cs
git commit -m "Update Program.cs to use SharedModels"
```

### Final ServerApp Build
```powershell
# Clean build
dotnet clean ServerApp/ServerApp.csproj
dotnet build ServerApp/ServerApp.csproj
```

## Step 1.4: Remove Duplicate Files

**IMPORTANT**: Only do this AFTER all builds succeed!

```powershell
# Remove ClientApp duplicate models
Remove-Item "ClientApp/Models/Product.cs"
Remove-Item "ClientApp/Models/Category.cs"
Remove-Item "ClientApp/Models/PaginatedList.cs"

# Build ClientApp
dotnet build ClientApp/ClientApp.csproj

# If build fails, you missed updating a using statement. Fix and rebuild.
# If build succeeds, commit
git add ClientApp/Models/
git commit -m "Remove duplicate models from ClientApp"
```

```powershell
# Remove ServerApp duplicate models
Remove-Item "ServerApp/Models/Product.cs"
Remove-Item "ServerApp/Models/Category.cs"
Remove-Item "ServerApp/Models/PaginatedList.cs"

# Build ServerApp
dotnet build ServerApp/ServerApp.csproj

# If build fails, fix and rebuild
# If build succeeds, commit
git add ServerApp/Models/
git commit -m "Remove duplicate models from ServerApp"
```

## Step 1.5: Full Solution Build

```powershell
# Build entire solution
dotnet build InventoryHub.sln
```

**Expected Output**: 
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**If successful**:
```powershell
git add .
git commit -m "Phase 1 complete: SharedModels integration successful"
```

## Step 1.6: Runtime Testing

### Test ServerApp
```powershell
cd ServerApp
dotnet run
```

**Verify**:
- Application starts without errors
- Navigate to http://localhost:5132
- Swagger UI loads
- Test API endpoints (GET /api/products)

**Press Ctrl+C to stop**

### Test ClientApp
```powershell
cd ../ClientApp
dotnet run
```

**Verify**:
- Application starts without errors
- Navigate to displayed URL (usually http://localhost:5xxx)
- Products page loads
- Create/Edit/Delete operations work
- No console errors in browser dev tools

**Press Ctrl+C to stop**

## Step 1.7: Phase 1 Completion

```powershell
cd ..
# Ensure all changes committed
git status

# Update checklist
code REFACTORING_CHECKLIST.md
```

**In checklist, mark these as [x]**:
- All Phase 1 tasks
- Update progress to 100%

```powershell
git add REFACTORING_CHECKLIST.md
git commit -m "Update checklist: Phase 1 complete"

# Merge to main (or create PR)
git checkout master
git merge refactor/phase1-shared-models

# Tag the completion
git tag phase1-complete
git push origin master --tags
```

---

# PHASE 2: Extract Business Logic to Service Layer

## Branch Setup
```powershell
git checkout master
git pull
git checkout -b refactor/phase2-business-service
```

## Step 2.1: Create ProductBusinessService Interface

```powershell
# Create interface file
code ServerApp/Services/IProductBusinessService.cs
```

**File content**:
```csharp
using SharedModels;

namespace ServerApp.Services;

/// <summary>
/// Interface for product business logic operations
/// </summary>
public interface IProductBusinessService
{
    Task<PaginatedList<Product>> GetProductsAsync(
        int pageNumber, int pageSize, string? searchTerm, int? categoryId);
    
    Task<Product?> GetProductByIdAsync(int id);
    
    Task<Product> CreateProductAsync(Product product);
    
    Task<Product> UpdateProductAsync(int id, Product updatedProduct);
    
    Task DeleteProductAsync(int id);
    
    Task RefreshSampleDataAsync();
}
```

**Save, build, commit**:
```powershell
dotnet build ServerApp/ServerApp.csproj
git add ServerApp/Services/IProductBusinessService.cs
git commit -m "Add IProductBusinessService interface"
```

## Step 2.2: Create ProductBusinessService Implementation

```powershell
code ServerApp/Services/ProductBusinessService.cs
```

**File content**: See `REFACTORING.md` section 1 for complete implementation

**Key points**:
- Move query building logic from ProductEndpoints
- Move caching coordination
- Inject AppDbContext, CacheService, ILogger
- Keep as much business logic as possible

**Build after creating**:
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Fix any errors**, then:
```powershell
git add ServerApp/Services/ProductBusinessService.cs
git commit -m "Add ProductBusinessService implementation"
```

## Step 2.3: Register Service in DI Container

```powershell
code ServerApp/Program.cs
```

**Add this line** in the service registration section:
```csharp
// Business services
builder.Services.AddScoped<IProductBusinessService, ProductBusinessService>();
```

**Build**:
```powershell
dotnet build ServerApp/ServerApp.csproj
git add ServerApp/Program.cs
git commit -m "Register ProductBusinessService in DI container"
```

## Step 2.4: Update ProductEndpoints - GetProducts

```powershell
code ServerApp/Endpoints/ProductEndpoints.cs
```

**Refactor GetProducts method** to use the service:
```csharp
private static async Task<IResult> GetProducts(
    HttpContext context,
    IProductBusinessService productService,
    int pageNumber = 1,
    int pageSize = 10,
    string? searchTerm = null,
    int? categoryId = null)
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var products = await productService.GetProductsAsync(
            pageNumber, pageSize, searchTerm, categoryId);
        return Results.Ok(products);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving products");
        return Results.Problem(
            title: "Error retrieving products",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
```

**Build**:
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Test endpoint**:
```powershell
dotnet run
# Test in browser or with curl: http://localhost:5132/api/products
# Ctrl+C when verified
```

**Commit**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Refactor GetProducts endpoint to use business service"
```

## Step 2.5: Update Remaining Endpoints

**Repeat the same pattern for**:
- GetProductById
- CreateProduct
- UpdateProduct
- DeleteProduct
- RefreshSampleData

**For each method**:
1. Update signature to inject IProductBusinessService
2. Simplify to just call service method
3. Keep error handling in endpoint
4. Build and test
5. Commit

**After all endpoints updated**:
```powershell
dotnet build ServerApp/ServerApp.csproj
dotnet run
# Test all CRUD operations in Swagger UI
# Ctrl+C

git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Complete endpoint refactoring to use business service"
```

## Step 2.6: Integration Testing

```powershell
# Start ServerApp
cd ServerApp
dotnet run
# Leave running in this terminal

# Open new terminal, start ClientApp
cd ClientApp
dotnet run
# Test all CRUD operations in browser
# Verify everything works
# Stop both servers (Ctrl+C in each terminal)
```

## Step 2.7: Phase 2 Completion

```powershell
cd ..
git status
code REFACTORING_CHECKLIST.md
# Mark all Phase 2 tasks as [x]

git add REFACTORING_CHECKLIST.md
git commit -m "Update checklist: Phase 2 complete"

git checkout master
git merge refactor/phase2-business-service
git tag phase2-complete
git push origin master --tags
```

---

# PHASE 3: Consolidate Validation

## Branch Setup
```powershell
git checkout master
git pull
git checkout -b refactor/phase3-validation
```

## Step 3.1: Review Current Validation

```powershell
# Check ValidationService
code ServerApp/Services/ValidationService.cs

# Check current usage
grep -r "ValidationService" ServerApp/
```

## Step 3.2: Update CreateProduct to Use ValidationService

```powershell
code ServerApp/Services/ProductBusinessService.cs
```

**In CreateProductAsync method**, add validation:
```csharp
public async Task<Product> CreateProductAsync(Product product)
{
    // Validate product
    if (!ValidationService.TryValidate(product, out var problemDetails))
    {
        throw new ValidationException("Product validation failed", problemDetails);
    }
    
    // Rest of create logic
}
```

**Note**: You may need to create a ValidationException class if it doesn't exist.

**Build and test**:
```powershell
dotnet build ServerApp/ServerApp.csproj
dotnet run
# Test creating invalid product in Swagger
# Ctrl+C
```

**Commit**:
```powershell
git add ServerApp/Services/ProductBusinessService.cs
git commit -m "Add validation to CreateProduct"
```

## Step 3.3: Update UpdateProduct to Use ValidationService

**Same pattern as CreateProduct**

## Step 3.4: Add Parameter Validation Methods

Create validation for pagination parameters:
```csharp
// In ValidationService.cs
public static (int pageNumber, int pageSize) ValidatePaginationParams(
    int pageNumber, int pageSize)
{
    if (pageNumber < 1) pageNumber = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;
    return (pageNumber, pageSize);
}
```

**Use in GetProductsAsync**:
```csharp
var (validPageNumber, validPageSize) = 
    ValidationService.ValidatePaginationParams(pageNumber, pageSize);
```

**Build, test, commit**

## Step 3.5: Phase 3 Completion

```powershell
dotnet build ServerApp/ServerApp.csproj
dotnet run
# Test validation scenarios
# Ctrl+C

git status
code REFACTORING_CHECKLIST.md
# Mark Phase 3 tasks as [x]

git add .
git commit -m "Update checklist: Phase 3 complete"

git checkout master
git merge refactor/phase3-validation
git tag phase3-complete
git push origin master --tags
```

---

# PHASE 4: Extract Constants

## Branch Setup
```powershell
git checkout master
git pull
git checkout -b refactor/phase4-constants
```

## Step 4.1: Create Constants File

```powershell
# Create directory if needed
New-Item -ItemType Directory -Path "ServerApp/Constants" -Force

code ServerApp/Constants/AppConstants.cs
```

**File content**:
```csharp
namespace ServerApp.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class AppConstants
{
    public static class Cache
    {
        public static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(2);
    }
    
    public static class Pagination
    {
        public const int DefaultPageSize = 12;
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;
        public static readonly int[] AllowedPageSizes = { 12, 24, 36, 48, 60 };
    }
    
    public static class Database
    {
        public const string InMemoryDatabaseName = "InventoryHub";
    }
}
```

**Build**:
```powershell
dotnet build ServerApp/ServerApp.csproj
git add ServerApp/Constants/AppConstants.cs
git commit -m "Add AppConstants class"
```

## Step 4.2: Update CacheService

```powershell
code ServerApp/Services/CacheService.cs
```

**Changes**:
1. Add: `using ServerApp.Constants;`
2. Replace hardcoded TimeSpans with `AppConstants.Cache.AbsoluteExpiration`

**Build, test, commit**

## Step 4.3: Update Program.cs

Replace `"InventoryHub"` with `AppConstants.Database.InMemoryDatabaseName`

## Step 4.4: Create ClientApp Constants

```powershell
New-Item -ItemType Directory -Path "ClientApp/Constants" -Force
code ClientApp/Constants/AppConstants.cs
```

**Content**:
```csharp
namespace ClientApp.Constants;

public static class AppConstants
{
    public static class Api
    {
        public const string DefaultBaseUrl = "http://localhost:5132";
    }
    
    public static class Pagination
    {
        public static readonly int[] AllowedPageSizes = { 12, 24, 36, 48, 60 };
        public const int DefaultPageSize = 12;
    }
}
```

## Step 4.5: Update ClientApp/Program.cs

```powershell
code ClientApp/Program.cs
```

Replace hardcoded URL with constant.

## Step 4.6: Update Products.razor

Replace hardcoded page size options with loop over `AppConstants.Pagination.AllowedPageSizes`

## Step 4.7: Phase 4 Completion

```powershell
# Build solution
dotnet build InventoryHub.sln

# Test both apps
cd ServerApp; dotnet run
# Ctrl+C
cd ../ClientApp; dotnet run
# Ctrl+C

cd ..
git add .
git commit -m "Complete constants extraction"

code REFACTORING_CHECKLIST.md
# Mark Phase 4 tasks as [x]

git add REFACTORING_CHECKLIST.md
git commit -m "Update checklist: Phase 4 complete"

git checkout master
git merge refactor/phase4-constants
git tag phase4-complete
git push origin master --tags
```

---

# PHASE 5: Add Unit Tests

## Branch Setup
```powershell
git checkout master
git pull
git checkout -b refactor/phase5-unit-tests
```

## Step 5.1: Create Test Projects

```powershell
# Create ServerApp.Tests
dotnet new xunit -n ServerApp.Tests -f net9.0
dotnet sln add ServerApp.Tests/ServerApp.Tests.csproj

# Add reference to ServerApp
dotnet add ServerApp.Tests/ServerApp.Tests.csproj reference ServerApp/ServerApp.csproj

# Add Moq package
dotnet add ServerApp.Tests/ServerApp.Tests.csproj package Moq

# Build
dotnet build ServerApp.Tests/ServerApp.Tests.csproj

git add ServerApp.Tests/
git commit -m "Create ServerApp.Tests project"
```

```powershell
# Create ClientApp.Tests
dotnet new xunit -n ClientApp.Tests -f net9.0
dotnet sln add ClientApp.Tests/ClientApp.Tests.csproj

# Add reference to ClientApp
dotnet add ClientApp.Tests/ClientApp.Tests.csproj reference ClientApp/ClientApp.csproj

# Add bUnit package for Blazor component testing
dotnet add ClientApp.Tests/ClientApp.Tests.csproj package bUnit

# Build
dotnet build ClientApp.Tests/ClientApp.Tests.csproj

git add ClientApp.Tests/
git commit -m "Create ClientApp.Tests project"
```

## Step 5.2: Write First ServerApp Test

```powershell
code ServerApp.Tests/ValidationServiceTests.cs
```

**Content**:
```csharp
using ServerApp.Services;
using SharedModels;
using Xunit;

namespace ServerApp.Tests;

public class ValidationServiceTests
{
    [Fact]
    public void TryValidate_ValidProduct_ReturnsTrue()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.99M,
            Stock = 5,
            CategoryId = 1
        };
        
        // Act
        var result = ValidationService.TryValidate(product, out var problemDetails);
        
        // Assert
        Assert.True(result);
        Assert.Null(problemDetails);
    }
    
    [Fact]
    public void TryValidate_InvalidProduct_ReturnsFalse()
    {
        // Arrange
        var product = new Product
        {
            Name = "", // Invalid: required
            Price = -1, // Invalid: must be positive
            CategoryId = 0 // Invalid: must be >= 1
        };
        
        // Act
        var result = ValidationService.TryValidate(product, out var problemDetails);
        
        // Assert
        Assert.False(result);
        Assert.NotNull(problemDetails);
        Assert.True(problemDetails.Errors.Count > 0);
    }
}
```

**Build and run tests**:
```powershell
dotnet build ServerApp.Tests/ServerApp.Tests.csproj
dotnet test ServerApp.Tests/ServerApp.Tests.csproj
```

**If tests pass**:
```powershell
git add ServerApp.Tests/ValidationServiceTests.cs
git commit -m "Add ValidationService unit tests"
```

## Step 5.3: Add More Tests

Create test files for:
- `CacheServiceTests.cs`
- `ProductBusinessServiceTests.cs` (use Moq for dependencies)
- `SeedingServiceTests.cs`

**After each test file**:
1. Write tests
2. Run: `dotnet test ServerApp.Tests/ServerApp.Tests.csproj`
3. Fix any failures
4. Commit

## Step 5.4: ClientApp Tests

```powershell
code ClientApp.Tests/ProductsStateServiceTests.cs
```

Write tests for state management.

## Step 5.5: Integration Tests (Optional but Recommended)

```powershell
dotnet new xunit -n ServerApp.IntegrationTests -f net9.0
dotnet sln add ServerApp.IntegrationTests/ServerApp.IntegrationTests.csproj
dotnet add ServerApp.IntegrationTests/ServerApp.IntegrationTests.csproj reference ServerApp/ServerApp.csproj
dotnet add ServerApp.IntegrationTests/ServerApp.IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing
```

## Step 5.6: Run All Tests

```powershell
# Run all tests in solution
dotnet test InventoryHub.sln

# Expected output should show all tests passing
```

## Step 5.7: Phase 5 Completion

```powershell
git add .
git commit -m "Complete unit test implementation"

code REFACTORING_CHECKLIST.md
# Mark Phase 5 tasks as [x]

git add REFACTORING_CHECKLIST.md
git commit -m "Update checklist: Phase 5 complete"

git checkout master
git merge refactor/phase5-unit-tests
git tag phase5-complete
git push origin master --tags
```

---

# PHASE 6: Security Hardening

## Branch Setup
```powershell
git checkout master
git pull
git checkout -b refactor/phase6-security
```

## Step 6.1: Update CORS Policy

```powershell
code ServerApp/Program.cs
```

**Replace CORS configuration**:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow any origin
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Production: Restrict to specific origins
            policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});
```

**Build and test**:
```powershell
dotnet build ServerApp/ServerApp.csproj
dotnet run
# Verify CORS still works in development
# Ctrl+C
```

**Commit**:
```powershell
git add ServerApp/Program.cs
git commit -m "Update CORS policy for environment-specific security"
```

## Step 6.2: Add Input Sanitization

Create sanitization helper:
```powershell
code ServerApp/Helpers/InputSanitizer.cs
```

**Add sanitization to search terms in ProductBusinessService**

## Step 6.3: Phase 6 Completion

```powershell
dotnet build InventoryHub.sln
dotnet test InventoryHub.sln

git add .
git commit -m "Complete security hardening"

code REFACTORING_CHECKLIST.md
# Mark Phase 6 tasks as [x]

git add REFACTORING_CHECKLIST.md
git commit -m "Update checklist: Phase 6 complete"

git checkout master
git merge refactor/phase6-security
git tag phase6-complete
git push origin master --tags
```

---

# ERROR HANDLING GUIDE

## Build Errors

### "Type or namespace not found"
1. Check using statements
2. Verify project references are correct
3. Run `dotnet restore`
4. Try clean build: `dotnet clean && dotnet build`

### "Ambiguous reference"
1. Check for duplicate model definitions
2. Fully qualify type name
3. Remove old using statements

### Compilation Errors After Refactoring
1. Revert last change: `git checkout .`
2. Review the step carefully
3. Make smaller, incremental changes
4. Build after each small change

## Runtime Errors

### Dependency Injection Errors
1. Verify service registered in Program.cs
2. Check service lifetime (Scoped, Singleton, Transient)
3. Ensure interface and implementation match

### 404 Errors
1. Verify endpoint routes are correct
2. Check middleware order in Program.cs
3. Verify app.MapProductEndpoints() is called

### Database Errors
1. Clear in-memory database (restart app)
2. Verify seeding runs correctly
3. Check entity configurations

## Test Failures

### Unit Test Failures
1. Check test setup/arrangement
2. Verify mocks are configured correctly
3. Check expected vs actual values
4. Review business logic for changes

### Integration Test Failures
1. Ensure test server starts correctly
2. Verify test data seeding
3. Check authentication/authorization
4. Review endpoint routes

## Git Issues

### Merge Conflicts
1. `git status` to see conflicted files
2. Open files and resolve conflicts
3. `git add <resolved-files>`
4. `git commit -m "Resolve merge conflicts"`

### Need to Rollback
```powershell
# Rollback to previous commit
git reset --hard HEAD~1

# Or rollback to specific tag
git reset --hard phase1-complete
```

---

# VERIFICATION CHECKLIST

After each phase:

- [ ] Solution builds without errors: `dotnet build InventoryHub.sln`
- [ ] All tests pass: `dotnet test InventoryHub.sln`
- [ ] ServerApp runs: `cd ServerApp && dotnet run`
- [ ] ClientApp runs: `cd ClientApp && dotnet run`
- [ ] All CRUD operations work in browser
- [ ] No console errors in browser dev tools
- [ ] Git branch has all commits
- [ ] Checklist updated
- [ ] Changes merged to master
- [ ] Tag created for phase

---

# FINAL COMPLETION

When all phases complete:

```powershell
# Ensure on master with all merges
git checkout master
git pull

# Verify final state
dotnet build InventoryHub.sln
dotnet test InventoryHub.sln

# Run both apps and test thoroughly
cd ServerApp; dotnet run
# (In another terminal)
cd ClientApp; dotnet run

# Update all documentation
code REFACTORING_CHECKLIST.md
# Mark all items as [x]
# Update progress to 100%

git add REFACTORING_CHECKLIST.md
git commit -m "Refactoring complete: All phases finished"

# Create final tag
git tag refactoring-complete
git push origin master --tags

# Generate summary report
git log --oneline --graph --all --decorate > REFACTORING_GIT_HISTORY.txt
```

---

# NOTES

- **Build often**: After every significant change, not just at the end of each step
- **Commit often**: Small, focused commits are easier to debug and rollback
- **Test continuously**: Don't wait until the end to test
- **Read error messages carefully**: They usually tell you exactly what's wrong
- **Don't skip steps**: Each step builds on the previous one
- **Keep branches clean**: One branch per phase, merge to master when complete
- **Document issues**: If you encounter problems, add notes to help future refactoring

---

**Created**: October 31, 2025  
**For**: InventoryHub Refactoring Project  
**Estimated Total Time**: 30-45 hours across all phases  
**Recommended Pace**: 1-2 phases per week
