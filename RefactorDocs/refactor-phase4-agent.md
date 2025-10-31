# Phase 4: Extract Magic Numbers and Strings - Agent Instructions

## Overview
This phase eliminates magic numbers and strings by extracting them into centralized constant classes.

**Estimated Time**: 1-2 hours  
**Risk Level**: Low  
**Prerequisites**: Phases 1-3 complete

---

## Pre-Phase Setup

```powershell
cd "s:\Microsoft Full-Stack Developer Professional Certificate\07 Full-Stack Integration\Final Project\InventoryHub"
git checkout master
git checkout -b refactor/phase4-constants
```

---

## Step 4.1: Create Constants Classes

### Create ServerApp/Constants/AppConstants.cs

**Agent creates**:
```csharp
namespace ServerApp.Constants;

public static class AppConstants
{
    public static class Cache
    {
        public const string ProductPrefix = "product_";
        public const int DefaultExpirationMinutes = 10;
    }

    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 10;
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;
    }

    public static class Validation
    {
        public const int MinProductId = 1;
        public const decimal MinPrice = 0;
        public const decimal MaxPrice = 1000000;
        public const int MinStock = 0;
        public const int MaxStock = 100000;
        public const int MaxProductNameLength = 200;
        public const int MaxProductDescriptionLength = 1000;
    }

    public static class Api
    {
        public const string BaseRoute = "/api/products";
        public const string SwaggerTitle = "InventoryHub API";
        public const string SwaggerVersion = "v1";
    }
}
```

**Build and commit**:
```powershell
dotnet build ServerApp/ServerApp.csproj
git add ServerApp/Constants/AppConstants.cs
git commit -m "Add AppConstants class"
```

---

### Create ClientApp/Constants/AppConstants.cs

**Agent creates**:
```csharp
namespace ClientApp.Constants;

public static class AppConstants
{
    public static class Api
    {
        public const string BaseUrl = "http://localhost:5132";
        public const string ProductsEndpoint = "/api/products";
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 10;
        public const int[] PageSizeOptions = new[] { 5, 10, 20, 50 };
    }

    public static class Validation
    {
        public const int MaxProductNameLength = 200;
        public const int MaxProductDescriptionLength = 1000;
    }

    public static class Toast
    {
        public const int DefaultDurationSeconds = 5;
        public const int SuccessDurationSeconds = 3;
        public const int ErrorDurationSeconds = 10;
    }
}
```

**Build and commit**:
```powershell
dotnet build ClientApp/ClientApp.csproj
git add ClientApp/Constants/AppConstants.cs
git commit -m "Add ClientApp AppConstants class"
```

---

## Step 4.2: Update ServerApp Files

### Update ValidationService.cs

**Replace magic numbers** with constants:

```csharp
using ServerApp.Constants;

// Example replacements:
// OLD: if (paginationParams.PageSize > 100)
// NEW: if (paginationParams.PageSize > AppConstants.Pagination.MaxPageSize)

// OLD: if (id <= 0)
// NEW: if (id < AppConstants.Validation.MinProductId)

// OLD: if (price > 1000000)
// NEW: if (price > AppConstants.Validation.MaxPrice)
```

**Build and commit** after each file.

---

### Update CacheService.cs

**Replace magic strings**:
```csharp
using ServerApp.Constants;

// OLD: _cache.Set(key, value, TimeSpan.FromMinutes(10));
// NEW: _cache.Set(key, value, TimeSpan.FromMinutes(AppConstants.Cache.DefaultExpirationMinutes));
```

---

### Update ProductEndpoints.cs

**Replace default values**:
```csharp
using ServerApp.Constants;

// OLD: [FromQuery] int page = 1, [FromQuery] int pageSize = 10
// NEW: [FromQuery] int page = AppConstants.Pagination.DefaultPage, 
//      [FromQuery] int pageSize = AppConstants.Pagination.DefaultPageSize
```

---

### Update Program.cs

**Replace magic strings**:
```csharp
using ServerApp.Constants;

// OLD: group.MapPost("/", ...
// NEW: Use constants for Swagger configuration
```

---

## Step 4.3: Update ClientApp Files

### Update ProductService.cs

**Replace hardcoded URL**:
```csharp
using ClientApp.Constants;

// OLD: private readonly string _baseUrl = "http://localhost:5132";
// NEW: private readonly string _baseUrl = AppConstants.Api.BaseUrl;
```

---

## Step 4.4: Full Build and Test

```powershell
dotnet build InventoryHub.sln
```

**Manual test**: Verify all functionality still works

---

## Step 4.5: Merge

```powershell
git add .
git commit -m "Phase 4 complete: Magic numbers/strings extracted to constants"
git checkout master
git merge refactor/phase4-constants --no-ff
git tag phase4-complete
```

---

**Benefits**: Easier maintenance, no magic numbers, centralized configuration

**Next Phase**: refactor-phase5-agent.md (Environment-Specific Configuration)

---

**Document Version**: 1.0  
**Phase**: 4 of 10
