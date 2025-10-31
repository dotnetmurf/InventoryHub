# Phase 9: Add Response Caching

## Overview
**Purpose**: Implement HTTP response caching to improve API performance and reduce database load  
**Time Estimate**: 1-2 hours  
**Risk Level**: Low  
**Prerequisites**: Phases 1-8 complete

## Success Criteria
- ✅ Response caching middleware configured
- ✅ GET endpoints have appropriate cache policies
- ✅ Cache invalidation on mutations (POST/PUT/DELETE)
- ✅ Cache headers visible in HTTP responses
- ✅ Measurable performance improvement on repeated requests

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
# Should show: phase1-complete through phase8-complete
```

### Create Feature Branch
```powershell
git checkout -b refactor/phase9-response-caching
```

---

## Step 9.1: Add Output Caching Package

### Add NuGet Package (if not already present)
```powershell
# .NET 7+ has built-in output caching
# Verify it's available
dotnet add ServerApp package Microsoft.AspNetCore.OutputCaching --version 8.0.0
```

**Note**: In .NET 9, output caching is built-in and doesn't require a separate package.

---

## Step 9.2: Configure Output Caching in Program.cs

### Update Program.cs

**Edit file**: `ServerApp/Program.cs`

Add output caching services and middleware:

```csharp
// Add after other service registrations
builder.Services.AddOutputCache(options =>
{
    // Default cache policy: 5 minutes
    options.AddBasePolicy(policy => 
        policy.Expire(TimeSpan.FromMinutes(5)));

    // Named policy for product lists (shorter cache)
    options.AddPolicy("ProductList", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(2));
        policy.SetVaryByQuery("page", "pageSize", "searchTerm");
        policy.Tag("products");
    });

    // Named policy for individual products (longer cache)
    options.AddPolicy("ProductDetail", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(10));
        policy.SetVaryByRouteValue("id");
        policy.Tag("products", "product-detail");
    });
});

// After app.Build() and middleware registration
var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();

// Add output caching middleware BEFORE endpoint mapping
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseStaticFiles();

// Map endpoints...
```

### Build to Verify
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Expected**: Build succeeds

---

## Step 9.3: Add Caching to GET Endpoints

### Update ProductEndpoints.cs

**Edit file**: `ServerApp/Endpoints/ProductEndpoints.cs`

Add cache output policies to GET endpoints:

```csharp
// GetAll endpoint with caching
products.MapGet("/", async (
    [AsParameters] PaginationParams paginationParams,
    IProductBusinessService productService) =>
{
    var paginatedProducts = await productService.GetAllAsync(
        paginationParams.Page,
        paginationParams.PageSize,
        paginationParams.SearchTerm);

    return Results.Ok(paginatedProducts);
})
.WithName("GetAllProducts")
.CacheOutput("ProductList") // Apply ProductList cache policy
.Produces<PaginatedList<Product>>(StatusCodes.Status200OK);

// GetById endpoint with caching
products.MapGet("/{id}", async (int id, IProductBusinessService productService) =>
{
    var product = await productService.GetByIdAsync(id);
    return Results.Ok(product);
})
.WithName("GetProductById")
.CacheOutput("ProductDetail") // Apply ProductDetail cache policy
.Produces<Product>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);
```

### Build to Verify
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Expected**: Build succeeds

---

## Step 9.4: Implement Cache Invalidation

### Update Mutation Endpoints

**Edit file**: `ServerApp/Endpoints/ProductEndpoints.cs`

Add cache eviction to POST/PUT/DELETE endpoints:

```csharp
// Create endpoint with cache invalidation
products.MapPost("/", async (
    CreateProductRequest request,
    IProductBusinessService productService,
    IOutputCacheStore cacheStore,
    CancellationToken cancellationToken) =>
{
    var product = await productService.CreateAsync(request);
    
    // Invalidate product caches
    await cacheStore.EvictByTagAsync("products", cancellationToken);
    
    return Results.Created($"/api/products/{product.Id}", product);
})
.WithName("CreateProduct")
.Produces<Product>(StatusCodes.Status201Created)
.Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status409Conflict);

// Update endpoint with cache invalidation
products.MapPut("/{id}", async (
    int id,
    UpdateProductRequest request,
    IProductBusinessService productService,
    IOutputCacheStore cacheStore,
    CancellationToken cancellationToken) =>
{
    var product = await productService.UpdateAsync(id, request);
    
    // Invalidate product caches
    await cacheStore.EvictByTagAsync("products", cancellationToken);
    
    return Results.Ok(product);
})
.WithName("UpdateProduct")
.Produces<Product>(StatusCodes.Status200OK)
.Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status409Conflict);

// Delete endpoint with cache invalidation
products.MapDelete("/{id}", async (
    int id,
    IProductBusinessService productService,
    IOutputCacheStore cacheStore,
    CancellationToken cancellationToken) =>
{
    await productService.DeleteAsync(id);
    
    // Invalidate product caches
    await cacheStore.EvictByTagAsync("products", cancellationToken);
    
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

## Step 9.5: Add Cache-Control Headers

### Create Cache Header Configuration

**Edit file**: `ServerApp/Program.cs`

Add custom cache headers for better client-side caching:

```csharp
// Update the ProductList policy
options.AddPolicy("ProductList", policy =>
{
    policy.Expire(TimeSpan.FromMinutes(2));
    policy.SetVaryByQuery("page", "pageSize", "searchTerm");
    policy.Tag("products");
    // Add custom headers
    policy.SetCacheability(ResponseCacheability.Public);
});

// Update the ProductDetail policy
options.AddPolicy("ProductDetail", policy =>
{
    policy.Expire(TimeSpan.FromMinutes(10));
    policy.SetVaryByRouteValue("id");
    policy.Tag("products", "product-detail");
    // Add custom headers
    policy.SetCacheability(ResponseCacheability.Public);
});
```

### Build to Verify
```powershell
dotnet build ServerApp/ServerApp.csproj
```

**Expected**: Build succeeds

---

## Step 9.6: Commit Changes

### Stage and Commit
```powershell
git add .
git commit -m "Phase 9: Add response caching

- Configured output caching middleware with custom policies
- Added ProductList cache policy (2 min, varies by query params)
- Added ProductDetail cache policy (10 min, varies by route id)
- Implemented cache invalidation on POST/PUT/DELETE operations
- Added cache tags for granular cache eviction
- Set cache-control headers for client-side caching
- Performance improvement: ~90% reduction in DB queries for cached responses"
```

---

## Manual Testing Checkpoint ⏸️

### Test Response Caching

1. **Start the ServerApp**:
```powershell
cd ServerApp
dotnet run
```

2. **Test GET Endpoint Caching (First Request)**:
```powershell
# In another terminal, measure first request time
Measure-Command {
    curl -X GET "http://localhost:5000/api/products?page=1&pageSize=10" -UseBasicParsing
}
```

Expected: ~50-100ms response time (database query)

3. **Test Cached Response (Second Request)**:
```powershell
# Immediately repeat the same request
Measure-Command {
    curl -X GET "http://localhost:5000/api/products?page=1&pageSize=10" -UseBasicParsing
}
```

Expected: ~1-5ms response time (served from cache) ⚡

4. **Verify Cache Headers**:
```powershell
curl -X GET "http://localhost:5000/api/products/1" -i
```

Expected response headers:
```
HTTP/1.1 200 OK
Content-Type: application/json
Cache-Control: public, max-age=600
X-Correlation-Id: <guid>
Age: 0
```

5. **Test Cache Invalidation**:
```powershell
# Create a new product (should invalidate cache)
curl -X POST "http://localhost:5000/api/products" `
  -H "Content-Type: application/json" `
  -d '{"name":"Cache Test Product","description":"Testing cache invalidation","price":25.00,"stock":10,"categoryId":1}'

# Request product list again (should be slow - cache was invalidated)
Measure-Command {
    curl -X GET "http://localhost:5000/api/products?page=1&pageSize=10" -UseBasicParsing
}
```

Expected: ~50-100ms (database query after cache invalidation)

6. **Test VaryByQuery**:
```powershell
# Request with different query params (should not use cached response)
curl -X GET "http://localhost:5000/api/products?page=2&pageSize=10" -UseBasicParsing
# Different params = different cache entry
```

7. **Test Product Detail Caching**:
```powershell
# First request for product detail
Measure-Command {
    curl -X GET "http://localhost:5000/api/products/1" -UseBasicParsing
}

# Second request (should be cached for 10 minutes)
Measure-Command {
    curl -X GET "http://localhost:5000/api/products/1" -UseBasicParsing
}
```

Expected: Second request ~1-5ms (10x+ faster)

8. **Check Performance Logs**:
Check ServerApp console output for request durations:
```
Request completed: GET /api/products | Status: 200 | Duration: 85ms | CorrelationId: <guid>
Request completed: GET /api/products | Status: 200 | Duration: 2ms | CorrelationId: <guid>
```

**Performance Metrics to Verify**:
- ✅ Cached GET requests are 10-50x faster than uncached
- ✅ Cache headers present in responses
- ✅ Cache invalidated after POST/PUT/DELETE
- ✅ Different query parameters create separate cache entries
- ✅ Age header increments on subsequent cached requests

**Human Response Required**:
- Type `pass` if caching is working and performance is improved
- Type `fail` if cache is not working correctly

---

## Merge to Master

### After Manual Testing Passes
```powershell
# Stop the running server (Ctrl+C)

# Switch to master
git checkout master

# Merge feature branch
git merge refactor/phase9-response-caching --no-ff -m "Merge Phase 9: Response Caching"

# Tag completion
git tag -a phase9-complete -m "Phase 9: HTTP response caching with cache invalidation complete"

# Push to remote (if applicable)
git push origin master --tags
```

---

## Phase 9 Complete! ✅

### Summary of Changes
- **Files Modified**: 2
  - `ServerApp/Program.cs` (output caching configuration)
  - `ServerApp/Endpoints/ProductEndpoints.cs` (cache policies and invalidation)

### Key Improvements
1. **Performance**: 10-50x faster response times for cached endpoints
2. **Database Load**: ~90% reduction in database queries for cached data
3. **Scalability**: Can handle much higher request volumes
4. **Cache Invalidation**: Automatic cache clearing on data mutations
5. **Smart Caching**: Varies cache by query parameters and route values
6. **Client-Side Caching**: Cache-Control headers enable browser/CDN caching

### Cache Configuration Summary
| Endpoint | Cache Duration | Varies By | Tags |
|----------|---------------|-----------|------|
| GET /api/products | 2 minutes | page, pageSize, searchTerm | products |
| GET /api/products/{id} | 10 minutes | id | products, product-detail |
| POST/PUT/DELETE | N/A | Invalidates: products | - |

### Performance Impact
- **Before**: Every request hits database (~50-100ms)
- **After**: Cached requests served from memory (~1-5ms)
- **Improvement**: 90-99% latency reduction for cached responses

### Next Phase
Proceed to **Phase 10: Enhanced Request Logging** (refactor-phase10-agent.md)

---

## Error Recovery

### If Caching Not Working
```powershell
# Check if output caching is registered
# In Program.cs, verify:
# builder.Services.AddOutputCache(...);
# app.UseOutputCache(); // MUST be before MapEndpoints

# Clear cache and restart
# Stop app with Ctrl+C
dotnet clean ServerApp/ServerApp.csproj
dotnet build ServerApp/ServerApp.csproj
dotnet run --project ServerApp/ServerApp.csproj
```

### If Cache Not Invalidating
```powershell
# Verify IOutputCacheStore is injected in mutation endpoints
# Check that EvictByTagAsync is called with correct tag
# Example:
# await cacheStore.EvictByTagAsync("products", cancellationToken);

# Check logs for any exceptions during cache eviction
```

### If Cache Headers Missing
```powershell
# Verify ResponseCacheability.Public is set in policies
# Check that UseOutputCache() is called in middleware pipeline
# Verify endpoint has .CacheOutput() method called
```

### Performance Not Improved
```powershell
# Use browser dev tools Network tab
# Check response headers for Age header (increases on cached responses)
# Compare request timing with and without cache
# Verify no errors in console logs
```

---

## Additional Configuration Options

### Cache by User (Authentication)
If you add authentication later, vary cache by user:

```csharp
options.AddPolicy("UserSpecific", policy =>
{
    policy.Expire(TimeSpan.FromMinutes(5));
    policy.SetVaryByHeader("Authorization");
});
```

### Disable Caching in Development
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseOutputCache();
}
```

### Custom Cache Key
```csharp
policy.SetVaryByQuery("customParam");
policy.SetVaryByRouteValue("customRoute");
policy.SetVaryByHeader("Accept-Language");
```

---

**Document Version**: 1.0  
**Last Updated**: Phase 9 detailed instructions  
**Estimated Completion Time**: 1-2 hours  
**Risk Level**: Low
