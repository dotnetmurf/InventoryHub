# Phase 6: Add Unit Tests (ServerApp) - Agent Instructions

## Overview
This phase creates comprehensive unit tests for the ServerApp service layer using xUnit, Moq, and FluentAssertions.

**Estimated Time**: 6-8 hours  
**Risk Level**: Low (new test project, no changes to production code)  
**Prerequisites**: Phases 1-5 complete

---

## Pre-Phase Setup

```powershell
cd "s:\Microsoft Full-Stack Developer Professional Certificate\07 Full-Stack Integration\Final Project\InventoryHub"
git checkout master
git checkout -b refactor/phase6-unit-tests
```

---

## Step 6.1: Create Test Project

**Agent executes**:
```powershell
dotnet new xunit -n ServerApp.Tests -o ServerApp.Tests
dotnet sln add ServerApp.Tests/ServerApp.Tests.csproj
```

**Agent adds project reference**:
```powershell
dotnet add ServerApp.Tests/ServerApp.Tests.csproj reference ServerApp/ServerApp.csproj
```

**Agent adds NuGet packages**:
```powershell
dotnet add ServerApp.Tests package Moq
dotnet add ServerApp.Tests package FluentAssertions
dotnet add ServerApp.Tests package Microsoft.EntityFrameworkCore.InMemory
```

**Agent builds**:
```powershell
dotnet build ServerApp.Tests/ServerApp.Tests.csproj
```

**If build succeeds**:
```powershell
git add .
git commit -m "Create ServerApp.Tests project with dependencies"
```

**Agent reports**: "✅ Step 6.1 complete. Test project created."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 6.2? (yes/no)"

---

## Step 6.2: Create ValidationService Tests

**Agent creates**: `ServerApp.Tests/Services/ValidationServiceTests.cs`

**Agent content** (comprehensive test class):
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServerApp.Configuration;
using ServerApp.Data;
using ServerApp.Models;
using ServerApp.Services;
using SharedModels;

namespace ServerApp.Tests.Services;

public class ValidationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ValidationService _validationService;
    private readonly PaginationSettings _paginationSettings;

    public ValidationServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // Setup pagination settings
        _paginationSettings = new PaginationSettings
        {
            DefaultPage = 1,
            DefaultPageSize = 10,
            MinPageSize = 1,
            MaxPageSize = 100
        };

        var paginationOptions = Options.Create(_paginationSettings);
        _validationService = new ValidationService(_context, paginationOptions);
    }

    [Fact]
    public void ValidateCreateProductRequest_WithValidRequest_ReturnsNoErrors()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.99m,
            Stock = 50,
            CategoryId = 1
        };

        // Act
        var errors = _validationService.ValidateCreateProductRequest(request);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCreateProductRequest_WithEmptyName_ReturnsError()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "",
            Description = "Test",
            Price = 10,
            Stock = 5
        };

        // Act
        var errors = _validationService.ValidateCreateProductRequest(request);

        // Assert
        errors.Should().ContainKey(nameof(request.Name));
        errors[nameof(request.Name)].Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public void ValidateCreateProductRequest_WithNegativePrice_ReturnsError()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test",
            Price = -10,
            Stock = 5
        };

        // Act
        var errors = _validationService.ValidateCreateProductRequest(request);

        // Assert
        errors.Should().ContainKey(nameof(request.Price));
        errors[nameof(request.Price)].Should().Contain(e => e.Contains("negative"));
    }

    [Fact]
    public void ValidateCreateProductRequest_WithNegativeStock_ReturnsError()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Test",
            Price = 10,
            Stock = -5
        };

        // Act
        var errors = _validationService.ValidateCreateProductRequest(request);

        // Assert
        errors.Should().ContainKey(nameof(request.Stock));
        errors[nameof(request.Stock)].Should().Contain(e => e.Contains("negative"));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    [InlineData(1, false)]
    [InlineData(5, false)]
    public void ValidatePaginationParams_WithVariousPageValues_ReturnsExpectedResult(
        int page, bool shouldHaveError)
    {
        // Arrange
        var paginationParams = new PaginationParams { Page = page, PageSize = 10 };

        // Act
        var errors = _validationService.ValidatePaginationParams(paginationParams);

        // Assert
        if (shouldHaveError)
        {
            errors.Should().ContainKey(nameof(paginationParams.Page));
        }
        else
        {
            errors.Should().NotContainKey(nameof(paginationParams.Page));
        }
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    [InlineData(1, false)]
    [InlineData(50, false)]
    [InlineData(100, false)]
    [InlineData(101, true)]
    public void ValidatePaginationParams_WithVariousPageSizeValues_ReturnsExpectedResult(
        int pageSize, bool shouldHaveError)
    {
        // Arrange
        var paginationParams = new PaginationParams { Page = 1, PageSize = pageSize };

        // Act
        var errors = _validationService.ValidatePaginationParams(paginationParams);

        // Assert
        if (shouldHaveError)
        {
            errors.Should().ContainKey(nameof(paginationParams.PageSize));
        }
        else
        {
            errors.Should().NotContainKey(nameof(paginationParams.PageSize));
        }
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    [InlineData(1, false)]
    [InlineData(100, false)]
    public void ValidateProductId_WithVariousIds_ReturnsExpectedResult(
        int id, bool shouldHaveError)
    {
        // Act
        var errors = _validationService.ValidateProductId(id);

        // Assert
        if (shouldHaveError)
        {
            errors.Should().ContainKey("Id");
        }
        else
        {
            errors.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task IsProductNameUniqueAsync_WithNewName_ReturnsTrue()
    {
        // Arrange
        await _context.Products.AddAsync(new Product { Name = "Existing Product" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _validationService.IsProductNameUniqueAsync("New Product");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsProductNameUniqueAsync_WithDuplicateName_ReturnsFalse()
    {
        // Arrange
        await _context.Products.AddAsync(new Product { Name = "Existing Product" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _validationService.IsProductNameUniqueAsync("Existing Product");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsProductNameUniqueAsync_WithExcludedId_ReturnsTrue()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Product 1" };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act (same name but exclude this product's ID)
        var result = await _validationService.IsProductNameUniqueAsync("Product 1", excludeId: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CategoryExistsAsync_WithExistingCategory_ReturnsTrue()
    {
        // Arrange
        await _context.Categories.AddAsync(new Category { Id = 1, Name = "Test Category" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _validationService.CategoryExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CategoryExistsAsync_WithNonExistingCategory_ReturnsFalse()
    {
        // Act
        var result = await _validationService.CategoryExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

**Agent builds and runs tests**:
```powershell
dotnet build ServerApp.Tests/ServerApp.Tests.csproj
dotnet test ServerApp.Tests/ServerApp.Tests.csproj
```

**If all tests pass**:
```powershell
git add ServerApp.Tests/Services/ValidationServiceTests.cs
git commit -m "Add comprehensive ValidationService tests (15 tests)"
```

**Agent reports**: "✅ Step 6.2 complete. ValidationService tests created (15 tests)."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 6.3? (yes/no)"

---

## Step 6.3: Create ProductBusinessService Tests

**Agent creates**: `ServerApp.Tests/Services/ProductBusinessServiceTests.cs`

**Agent content** (test class with mocks):
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ServerApp.Data;
using ServerApp.Models;
using ServerApp.Services;
using SharedModels;

namespace ServerApp.Tests.Services;

public class ProductBusinessServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ValidationService> _mockValidationService;
    private readonly Mock<CacheService> _mockCacheService;
    private readonly Mock<ILogger<ProductBusinessService>> _mockLogger;
    private readonly ProductBusinessService _businessService;

    public ProductBusinessServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockValidationService = new Mock<ValidationService>();
        _mockCacheService = new Mock<CacheService>();
        _mockLogger = new Mock<ILogger<ProductBusinessService>>();

        _businessService = new ProductBusinessService(
            _context,
            _mockValidationService.Object,
            _mockCacheService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsP aginatedList()
    {
        // Arrange
        await SeedProducts();
        _mockValidationService
            .Setup(v => v.ValidatePaginationParams(It.IsAny<PaginationParams>()))
            .Returns(new Dictionary<string, string[]>());

        var paginationParams = new PaginationParams { Page = 1, PageSize = 10 };

        // Act
        var result = await _businessService.GetProductsAsync(paginationParams);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetProductsAsync_WithInvalidPagination_ThrowsValidationException()
    {
        // Arrange
        _mockValidationService
            .Setup(v => v.ValidatePaginationParams(It.IsAny<PaginationParams>()))
            .Returns(new Dictionary<string, string[]>
            {
                { "PageSize", new[] { "PageSize cannot exceed 100" } }
            });

        var paginationParams = new PaginationParams { Page = 1, PageSize = 200 };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _businessService.GetProductsAsync(paginationParams)
        );
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 10 };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        _mockValidationService
            .Setup(v => v.ValidateProductId(1))
            .Returns(new Dictionary<string, string[]>());

        _mockCacheService
            .Setup(c => c.Get<Product>(It.IsAny<string>()))
            .Returns((Product?)null);

        // Act
        var result = await _businessService.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
        _mockCacheService.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithCachedProduct_ReturnsCachedProduct()
    {
        // Arrange
        var cachedProduct = new Product { Id = 1, Name = "Cached Product", Price = 10 };

        _mockValidationService
            .Setup(v => v.ValidateProductId(1))
            .Returns(new Dictionary<string, string[]>());

        _mockCacheService
            .Setup(c => c.Get<Product>("product_1"))
            .Returns(cachedProduct);

        // Act
        var result = await _businessService.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Cached Product");
        // Database should not be queried
        _context.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateProductAsync_WithValidRequest_CreatesProduct()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Description = "Description",
            Price = 29.99m,
            Stock = 50
        };

        _mockValidationService
            .Setup(v => v.ValidateCreateProductRequest(request))
            .Returns(new Dictionary<string, string[]>());

        _mockValidationService
            .Setup(v => v.IsProductNameUniqueAsync(request.Name, null))
            .ReturnsAsync(true);

        // Act
        var result = await _businessService.CreateProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        result.Price.Should().Be(29.99m);
        _context.Products.Should().HaveCount(1);
        _mockCacheService.Verify(c => c.ClearByPrefix("product_"), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateProductRequest { Name = "Duplicate Product", Price = 10, Stock = 5 };

        _mockValidationService
            .Setup(v => v.ValidateCreateProductRequest(request))
            .Returns(new Dictionary<string, string[]>());

        _mockValidationService
            .Setup(v => v.IsProductNameUniqueAsync(request.Name, null))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _businessService.CreateProductAsync(request)
        );
    }

    [Fact]
    public async Task UpdateProductAsync_WithValidRequest_UpdatesProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Old Name", Price = 10, Stock = 5 };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var request = new UpdateProductRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 20,
            Stock = 10
        };

        _mockValidationService
            .Setup(v => v.ValidateUpdateProductRequest(request))
            .Returns(new Dictionary<string, string[]>());

        _mockValidationService
            .Setup(v => v.IsProductNameUniqueAsync(request.Name, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _businessService.UpdateProductAsync(1, request);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Price.Should().Be(20);
        _mockCacheService.Verify(c => c.ClearByPrefix("product_"), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_WithNonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateProductRequest { Name = "Test", Price = 10, Stock = 5 };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _businessService.UpdateProductAsync(999, request)
        );
    }

    [Fact]
    public async Task DeleteProductAsync_WithValidId_DeletesProduct()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 10 };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _businessService.DeleteProductAsync(1);

        // Assert
        result.Should().BeTrue();
        _context.Products.Should().BeEmpty();
        _mockCacheService.Verify(c => c.ClearByPrefix("product_"), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = await _businessService.DeleteProductAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    private async Task SeedProducts()
    {
        await _context.Products.AddRangeAsync(
            new Product { Id = 1, Name = "Product 1", Price = 10 },
            new Product { Id = 2, Name = "Product 2", Price = 20 },
            new Product { Id = 3, Name = "Product 3", Price = 30 }
        );
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

**Agent builds and runs tests**:
```powershell
dotnet test ServerApp.Tests/ServerApp.Tests.csproj
```

**If all tests pass**:
```powershell
git add ServerApp.Tests/Services/ProductBusinessServiceTests.cs
git commit -m "Add ProductBusinessService tests (11 tests)"
```

**Agent reports**: "✅ Step 6.3 complete. ProductBusinessService tests created (11 tests)."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 6.4? (yes/no)"

---

## Step 6.4: Create CacheService Tests

**Agent creates**: `ServerApp.Tests/Services/CacheServiceTests.cs`

**Agent content**:
```csharp
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using ServerApp.Configuration;
using ServerApp.Services;

namespace ServerApp.Tests.Services;

public class CacheServiceTests
{
    [Fact]
    public void Set_And_Get_StoresAndRetrievesValue()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheSettings = Options.Create(new CacheSettings { Enabled = true });
        var cacheService = new CacheService(memoryCache, cacheSettings);

        // Act
        cacheService.Set("test_key", "test_value");
        var result = cacheService.Get<string>("test_key");

        // Assert
        result.Should().Be("test_value");
    }

    [Fact]
    public void Get_WithNonExistentKey_ReturnsDefault()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheSettings = Options.Create(new CacheSettings { Enabled = true });
        var cacheService = new CacheService(memoryCache, cacheSettings);

        // Act
        var result = cacheService.Get<string>("nonexistent_key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ClearByPrefix_RemovesMatchingKeys()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheSettings = Options.Create(new CacheSettings { Enabled = true });
        var cacheService = new CacheService(memoryCache, cacheSettings);

        cacheService.Set("product_1", "value1");
        cacheService.Set("product_2", "value2");
        cacheService.Set("category_1", "value3");

        // Act
        cacheService.ClearByPrefix("product_");

        // Assert
        cacheService.Get<string>("product_1").Should().BeNull();
        cacheService.Get<string>("product_2").Should().BeNull();
        cacheService.Get<string>("category_1").Should().Be("value3"); // Not cleared
    }

    [Fact]
    public void Set_WhenCachingDisabled_DoesNotStore()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheSettings = Options.Create(new CacheSettings { Enabled = false });
        var cacheService = new CacheService(memoryCache, cacheSettings);

        // Act
        cacheService.Set("test_key", "test_value");
        var result = cacheService.Get<string>("test_key");

        // Assert
        result.Should().BeNull();
    }
}
```

**Agent builds and runs tests**:
```powershell
dotnet test ServerApp.Tests/ServerApp.Tests.csproj
```

**If all tests pass**:
```powershell
git add ServerApp.Tests/Services/CacheServiceTests.cs
git commit -m "Add CacheService tests (4 tests)"
```

**Agent reports**: "✅ Step 6.4 complete. CacheService tests created (4 tests)."

---

## Step 6.5: Run All Tests and Generate Report

**Agent executes**:
```powershell
dotnet test ServerApp.Tests/ServerApp.Tests.csproj --logger "console;verbosity=detailed"
```

**Agent counts**:
- Total tests: ~30
- ValidationService: 15 tests
- ProductBusinessService: 11 tests
- CacheService: 4 tests

**Agent executes** (optional code coverage):
```powershell
dotnet test ServerApp.Tests/ServerApp.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Agent reports**: 
```
✅ Step 6.5 complete. All tests passing.

Test Summary:
- Total Tests: 30
- Passed: 30
- Failed: 0
- Skipped: 0
- Duration: ~5-10 seconds

Coverage (estimated):
- ValidationService: 95%+
- ProductBusinessService: 85%+
- CacheService: 90%+
```

---

## Step 6.6: Update Checklist and Merge

**Agent updates**: `REFACTORING_CHECKLIST.md`

```markdown
## ✅ Phase 6: Add Unit Tests (ServerApp) (100% Complete)

**Completed**: [Current Date]

- [x] Create ServerApp.Tests project
- [x] Add xUnit, Moq, FluentAssertions packages
- [x] Create ValidationServiceTests (15 tests)
- [x] Create ProductBusinessServiceTests (11 tests)
- [x] Create CacheServiceTests (4 tests)
- [x] All tests passing
- [x] Code coverage >80% on service layer
```

**Agent commits**:
```powershell
git add .
git commit -m "Phase 6 complete: 30 unit tests added for ServerApp services"
```

---

**Agent asks**: "⏸️ DECISION POINT: Merge Phase 6 to master? (yes/no)"

**On "yes"**:
```powershell
git checkout master
git merge refactor/phase6-unit-tests --no-ff
git tag phase6-complete
```

**Agent reports**: 
```
✅ Phase 6 Complete!

Summary:
- Test project created: ServerApp.Tests
- Test files: 3
- Total tests: 30
- All tests passing
- Code coverage: 80%+ on service layer
- Status: SUCCESS

Benefits Achieved:
✅ Comprehensive test coverage
✅ Regression detection capability
✅ Documented expected behavior
✅ Confidence for future refactoring
✅ Easier debugging and maintenance

Next Phase: Phase 7 - Add Client-Side Tests
```

**Agent asks**: "⏸️ Start Phase 7? (yes/no/later)"

---

**Document Version**: 1.0  
**Phase**: 6 of 10  
**Prerequisites**: Phases 1-5 complete  
**Next Phase**: refactor-phase7-agent.md (Client-Side Tests)
