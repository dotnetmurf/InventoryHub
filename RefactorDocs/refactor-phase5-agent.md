# Phase 5: Environment-Specific Configuration - Agent Instructions

## Overview
This phase implements proper configuration management using appsettings.json, environment-specific settings, and the Options pattern for strongly-typed configuration.

**Estimated Time**: 1-2 hours  
**Risk Level**: Low (configuration changes)  
**Rollback Strategy**: Git branch allows complete rollback  
**Prerequisites**: Phases 1-4 complete

---

## Pre-Phase Checklist

**Agent verifies**:
- [ ] On master branch
- [ ] Working directory clean
- [ ] Phases 1-4 complete
- [ ] Solution builds successfully

**Agent actions**:
```powershell
cd "s:\Microsoft Full-Stack Developer Professional Certificate\07 Full-Stack Integration\Final Project\InventoryHub"
git status
git checkout master
git checkout -b refactor/phase5-configuration
```

**Agent reports**: "✅ Branch 'refactor/phase5-configuration' created"

---

## Step 5.1: Create Configuration Models

### Create ServerApp/Configuration/CacheSettings.cs

**Agent creates**: `ServerApp/Configuration/CacheSettings.cs`

**Agent content**:
```csharp
namespace ServerApp.Configuration;

/// <summary>
/// Configuration settings for caching
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public const string SectionName = "CacheSettings";

    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 10;

    /// <summary>
    /// Product cache key prefix
    /// </summary>
    public string ProductCachePrefix { get; set; } = "product_";

    /// <summary>
    /// Enable or disable caching
    /// </summary>
    public bool Enabled { get; set; } = true;
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Configuration/CacheSettings.cs
git commit -m "Add CacheSettings configuration model"
```

**Agent reports**: "✅ CacheSettings created"

---

### Create ServerApp/Configuration/PaginationSettings.cs

**Agent creates**: `ServerApp/Configuration/PaginationSettings.cs`

**Agent content**:
```csharp
namespace ServerApp.Configuration;

/// <summary>
/// Configuration settings for pagination
/// </summary>
public class PaginationSettings
{
    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public const string SectionName = "PaginationSettings";

    /// <summary>
    /// Default page number
    /// </summary>
    public int DefaultPage { get; set; } = 1;

    /// <summary>
    /// Default page size
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    /// Minimum allowed page size
    /// </summary>
    public int MinPageSize { get; set; } = 1;

    /// <summary>
    /// Maximum allowed page size
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Configuration/PaginationSettings.cs
git commit -m "Add PaginationSettings configuration model"
```

**Agent reports**: "✅ PaginationSettings created"

---

### Create ServerApp/Configuration/CorsSettings.cs

**Agent creates**: `ServerApp/Configuration/CorsSettings.cs`

**Agent content**:
```csharp
namespace ServerApp.Configuration;

/// <summary>
/// Configuration settings for CORS policy
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// Section name in appsettings.json
    /// </summary>
    public const string SectionName = "CorsSettings";

    /// <summary>
    /// Allowed origins for CORS
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Allow credentials in CORS requests
    /// </summary>
    public bool AllowCredentials { get; set; } = false;

    /// <summary>
    /// Policy name
    /// </summary>
    public string PolicyName { get; set; } = "DefaultCorsPolicy";
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Configuration/CorsSettings.cs
git commit -m "Add CorsSettings configuration model"
```

**Agent reports**: "✅ CorsSettings created"

**Agent reports**: "✅ Step 5.1 complete. Configuration models created."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 5.2? (yes/no)"

---

## Step 5.2: Update appsettings.json

**Agent reads**: `ServerApp/appsettings.json`

**Agent adds** new configuration sections after existing content:

**Find the end of the file** (before the final closing brace):

**OLD**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**NEW**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "CacheSettings": {
    "DefaultExpirationMinutes": 10,
    "ProductCachePrefix": "product_",
    "Enabled": true
  },
  "PaginationSettings": {
    "DefaultPage": 1,
    "DefaultPageSize": 10,
    "MinPageSize": 1,
    "MaxPageSize": 100
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:5058",
      "https://localhost:7058"
    ],
    "AllowCredentials": false,
    "PolicyName": "DefaultCorsPolicy"
  }
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/appsettings.json
git commit -m "Add configuration sections to appsettings.json"
```

**Agent reports**: "✅ appsettings.json updated"

---

## Step 5.3: Update appsettings.Development.json

**Agent reads**: `ServerApp/appsettings.Development.json`

**Agent adds** development-specific overrides:

**OLD**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**NEW**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "CacheSettings": {
    "DefaultExpirationMinutes": 5,
    "Enabled": true
  },
  "PaginationSettings": {
    "DefaultPageSize": 20,
    "MaxPageSize": 100
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "http://localhost:5058",
      "https://localhost:7058",
      "http://localhost:3000"
    ],
    "AllowCredentials": true,
    "PolicyName": "DevelopmentCorsPolicy"
  }
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/appsettings.Development.json
git commit -m "Add development-specific configuration overrides"
```

**Agent reports**: "✅ appsettings.Development.json updated"

**Agent reports**: "✅ Step 5.3 complete. Configuration files updated."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 5.4? (yes/no)"

---

## Step 5.4: Register Configuration in Program.cs

**Agent reads**: `ServerApp/Program.cs`

**Agent finds** service registration section (before `builder.Services.AddDbContext`)

**Agent adds** configuration registrations:

**Find**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AppDbContext>
```

**Replace with**:
```csharp
using ServerApp.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure strongly-typed settings
builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection(CacheSettings.SectionName));
builder.Services.Configure<PaginationSettings>(
    builder.Configuration.GetSection(PaginationSettings.SectionName));
builder.Services.Configure<CorsSettings>(
    builder.Configuration.GetSection(CorsSettings.SectionName));

// Add services to the container
builder.Services.AddDbContext<AppDbContext>
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Program.cs
git commit -m "Register configuration settings in DI container"
```

**Agent reports**: "✅ Configuration registered in DI"

---

### Update CORS Configuration in Program.cs

**Agent finds** CORS configuration section:

**Find**:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

**Replace with**:
```csharp
// Configure CORS from settings
var corsSettings = builder.Configuration
    .GetSection(CorsSettings.SectionName)
    .Get<CorsSettings>() ?? new CorsSettings();

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSettings.PolicyName, policy =>
    {
        policy.WithOrigins(corsSettings.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();

        if (corsSettings.AllowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});
```

**Agent finds** `app.UseCors()` call:

**Find**:
```csharp
app.UseCors();
```

**Replace with**:
```csharp
app.UseCors(corsSettings.PolicyName);
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Program.cs
git commit -m "Update CORS to use configuration from appsettings"
```

**Agent reports**: "✅ CORS configuration updated"

**Agent reports**: "✅ Step 5.4 complete. Program.cs configured."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 5.5? (yes/no)"

---

## Step 5.5: Update CacheService to Use IOptions

**Agent reads**: `ServerApp/Services/CacheService.cs`

**Agent adds** IOptions injection:

**Find** the constructor:
```csharp
using Microsoft.Extensions.Caching.Memory;

namespace ServerApp.Services;

public class CacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }
```

**Replace with**:
```csharp
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ServerApp.Configuration;

namespace ServerApp.Services;

public class CacheService
{
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _cacheSettings;

    public CacheService(IMemoryCache cache, IOptions<CacheSettings> cacheSettings)
    {
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
    }
```

**Agent finds** `Set` method with hardcoded expiration:

**Find**:
```csharp
public void Set<T>(string key, T value)
{
    var cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };
    _cache.Set(key, value, cacheOptions);
}
```

**Replace with**:
```csharp
public void Set<T>(string key, T value)
{
    if (!_cacheSettings.Enabled)
    {
        return;
    }

    var cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.DefaultExpirationMinutes)
    };
    _cache.Set(key, value, cacheOptions);
}
```

**Agent finds** `Get` method:

**Find**:
```csharp
public T? Get<T>(string key)
{
    return _cache.TryGetValue(key, out T? value) ? value : default;
}
```

**Replace with**:
```csharp
public T? Get<T>(string key)
{
    if (!_cacheSettings.Enabled)
    {
        return default;
    }

    return _cache.TryGetValue(key, out T? value) ? value : default;
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Services/CacheService.cs
git commit -m "Update CacheService to use IOptions<CacheSettings>"
```

**Agent reports**: "✅ CacheService updated to use configuration"

**Agent reports**: "✅ Step 5.5 complete. Services updated."

---

## Step 5.6: Update ValidationService to Use IOptions

**Agent reads**: `ServerApp/Services/ValidationService.cs`

**Agent adds** IOptions injection:

**Find** the constructor and any hardcoded limits:

**Find**:
```csharp
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
```

**Replace with** (add field and update constructor first):

**Add at top of class**:
```csharp
using Microsoft.Extensions.Options;
using ServerApp.Configuration;

private readonly PaginationSettings _paginationSettings;

public ValidationService(AppDbContext context, IOptions<PaginationSettings> paginationSettings)
{
    _context = context;
    _paginationSettings = paginationSettings.Value;
}
```

**Then update validation method**:
```csharp
public Dictionary<string, string[]> ValidatePaginationParams(PaginationParams paginationParams)
{
    var errors = new Dictionary<string, string[]>();

    if (paginationParams.Page < _paginationSettings.DefaultPage)
    {
        errors.Add(nameof(paginationParams.Page), 
            new[] { $"Page must be greater than or equal to {_paginationSettings.DefaultPage}" });
    }

    if (paginationParams.PageSize < _paginationSettings.MinPageSize)
    {
        errors.Add(nameof(paginationParams.PageSize), 
            new[] { $"PageSize must be greater than or equal to {_paginationSettings.MinPageSize}" });
    }

    if (paginationParams.PageSize > _paginationSettings.MaxPageSize)
    {
        errors.Add(nameof(paginationParams.PageSize), 
            new[] { $"PageSize cannot exceed {_paginationSettings.MaxPageSize}" });
    }

    return errors;
}
```

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Services/ValidationService.cs
git commit -m "Update ValidationService to use IOptions<PaginationSettings>"
```

**Agent reports**: "✅ ValidationService updated to use configuration"

**Agent reports**: "✅ Step 5.6 complete. Validation configured."

**Agent asks**: "⏸️ CHECKPOINT: Continue to Step 5.7? (yes/no)"

---

## Step 5.7: Update ProductEndpoints Default Values

**Agent reads**: `ServerApp/Endpoints/ProductEndpoints.cs`

**Agent finds** GET /products endpoint with hardcoded defaults:

**Find**:
```csharp
products.MapGet("/", async (
    IProductBusinessService businessService,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10) =>
```

**Replace with** (inject IOptions):
```csharp
using Microsoft.Extensions.Options;
using ServerApp.Configuration;

products.MapGet("/", async (
    IProductBusinessService businessService,
    IOptions<PaginationSettings> paginationOptions,
    [FromQuery] int? page,
    [FromQuery] int? pageSize) =>
{
    var settings = paginationOptions.Value;
    var paginationParams = new PaginationParams 
    { 
        Page = page ?? settings.DefaultPage, 
        PageSize = pageSize ?? settings.DefaultPageSize 
    };
```

**Note**: Need to update the rest of the method to use paginationParams

**Agent executes**: `dotnet build ServerApp/ServerApp.csproj`

**If build succeeds**:
```powershell
git add ServerApp/Endpoints/ProductEndpoints.cs
git commit -m "Update ProductEndpoints to use configuration defaults"
```

**Agent reports**: "✅ ProductEndpoints updated"

**Agent reports**: "✅ Step 5.7 complete. Endpoints configured."

---

## Step 5.8: Full Solution Build

**Agent executes**:
```powershell
dotnet clean InventoryHub.sln
dotnet build InventoryHub.sln
```

**Agent verifies**: 0 errors

**If build succeeds**:
```powershell
git add .
git commit -m "Phase 5 complete: Environment-specific configuration implemented"
```

**Agent reports**: "✅ Step 5.8 complete. Solution builds successfully."

---

## Step 5.9: Integration Testing (MANUAL CHECKPOINT)

**Agent reports**: "⏸️ CONFIGURATION TESTING REQUIRED"

**Agent provides instructions**:

```
==========================================
CHECKPOINT: Configuration Testing
==========================================

1. START SERVERAPP IN DEVELOPMENT MODE:
   - Terminal: cd ServerApp
   - Run: $env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run
   - Verify: Development settings loaded (check logs)

2. TEST CORS CONFIGURATION:
   - Navigate to: http://localhost:5132/swagger
   - Open browser console (F12)
   - Try API call from different origin
   - Expected: CORS headers present

3. TEST PAGINATION DEFAULTS:
   - GET /api/products (no parameters)
   - Expected: Uses DefaultPageSize from config (20 in dev)
   - GET /api/products?pageSize=150
   - Expected: 400 Bad Request (exceeds MaxPageSize)

4. TEST CACHE CONFIGURATION:
   - GET /api/products/1 (first call)
   - GET /api/products/1 (second call)
   - Check ServerApp logs for cache hit
   - Expected: Cache working with configured expiration

5. TEST PRODUCTION CONFIGURATION:
   - Stop server
   - Run: $env:ASPNETCORE_ENVIRONMENT="Production"; dotnet run
   - Test same scenarios
   - Expected: Production settings (pageSize=10, shorter cache)

6. VERIFY CLIENT APP STILL WORKS:
   - Run ClientApp: dotnet run (in ClientApp folder)
   - Test all CRUD operations
   - Expected: No CORS errors, all operations work

==========================================
Please report results:
- Type "pass" if all configuration tests successful
- Type "fail: [description]" if issues found
==========================================
```

**Agent waits for**: Human response

---

## Step 5.10: Update Checklist and Merge

**Agent updates**: `REFACTORING_CHECKLIST.md`

```markdown
## ✅ Phase 5: Environment-Specific Configuration (100% Complete)

**Completed**: [Current Date]

- [x] Create CacheSettings configuration model
- [x] Create PaginationSettings configuration model
- [x] Create CorsSettings configuration model
- [x] Update appsettings.json with new sections
- [x] Update appsettings.Development.json with overrides
- [x] Register configurations in Program.cs
- [x] Update CORS to use configuration
- [x] Update CacheService to use IOptions
- [x] Update ValidationService to use IOptions
- [x] Update ProductEndpoints to use configuration
- [x] Test development configuration
- [x] Test production configuration
- [x] Verify CORS restrictions work
```

**Agent commits**:
```powershell
git add REFACTORING_CHECKLIST.md
git commit -m "Update checklist: Phase 5 complete"
```

---

**Agent asks**: "⏸️ DECISION POINT: Merge Phase 5 to master? (yes/no)"

**On "yes"**:
```powershell
git checkout master
git merge refactor/phase5-configuration --no-ff
git tag phase5-complete
```

**Agent reports**: 
```
✅ Phase 5 Complete!

Summary:
- Branch: refactor/phase5-configuration merged to master
- Tag: phase5-complete created
- Files created: 3 (configuration models)
- Files modified: 6 (Program.cs, appsettings, services, endpoints)
- Status: SUCCESS

Benefits Achieved:
✅ Strongly-typed configuration with IOptions pattern
✅ Environment-specific settings (dev vs production)
✅ CORS properly restricted (no more AllowAnyOrigin)
✅ Configurable pagination limits
✅ Configurable cache behavior
✅ Easy to modify settings without code changes

Next Phase: Phase 6 - Add Unit Tests
```

**Agent asks**: "⏸️ Start Phase 6? (yes/no/later)"

---

## Error Recovery

### If IOptions Not Working

**Agent checks**:
- Configuration sections spelled correctly in appsettings.json
- Section names match constants in configuration classes
- Services properly registered in Program.cs

### If CORS Issues

**Agent verifies**:
- AllowedOrigins array has correct URLs
- No trailing slashes in URLs
- Port numbers match actual app ports

---

## Success Metrics

**Phase 5 is successful when**:
- ✅ All configuration models created
- ✅ IOptions pattern implemented throughout
- ✅ CORS restricted to specific origins
- ✅ Development vs production settings work
- ✅ Configuration read from appsettings.json
- ✅ No hardcoded configuration in code
- ✅ All existing functionality preserved

---

## Files Modified Summary

**Created** (3 files):
- ServerApp/Configuration/CacheSettings.cs
- ServerApp/Configuration/PaginationSettings.cs
- ServerApp/Configuration/CorsSettings.cs

**Modified** (6 files):
- ServerApp/appsettings.json
- ServerApp/appsettings.Development.json
- ServerApp/Program.cs
- ServerApp/Services/CacheService.cs
- ServerApp/Services/ValidationService.cs
- ServerApp/Endpoints/ProductEndpoints.cs
- REFACTORING_CHECKLIST.md

**Total**: 9 files affected

---

**Document Version**: 1.0  
**Phase**: 5 of 10  
**Prerequisites**: Phases 1-4 complete  
**Next Phase**: refactor-phase6-agent.md (Add Unit Tests)
