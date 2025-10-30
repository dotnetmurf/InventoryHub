using Microsoft.EntityFrameworkCore;
using ServerApp.Data;
using ServerApp.Models;
using ServerApp.Services;

namespace ServerApp.Endpoints;

/// <summary>
/// Defines all product-related API endpoints
/// </summary>
/// <remarks>
/// Contains CRUD operations for products with caching, pagination, search, and filtering.
/// All endpoints include performance monitoring and comprehensive error handling.
/// Cache invalidation occurs automatically after mutations (POST, PUT, DELETE).
/// </remarks>
public static class ProductEndpoints
{
    /// <summary>
    /// Maps all product endpoints to the application
    /// </summary>
    /// <param name="app">WebApplication instance</param>
    /// <returns>WebApplication for method chaining</returns>
    public static WebApplication MapProductEndpoints(this WebApplication app)
    {
        var logger = app.Logger;

        // GET /api/products - Get paginated list of products with optional search and category filter
        app.MapGet("/api/products", GetProducts)
            .WithName("GetProducts")
            .WithOpenApi();

        // GET /api/product/{id} - Get single product by ID
        app.MapGet("/api/product/{id:int}", GetProductById)
            .WithName("GetProductById")
            .WithOpenApi();

        // POST /api/product - Create new product
        app.MapPost("/api/product", CreateProduct)
            .WithName("CreateProduct")
            .WithOpenApi();

        // PUT /api/product/{id} - Update existing product
        app.MapPut("/api/product/{id:int}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithOpenApi();

        // DELETE /api/product/{id} - Delete product
        app.MapDelete("/api/product/{id:int}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithOpenApi();

        // POST /api/products/refresh - Refresh sample data
        app.MapPost("/api/products/refresh", (Delegate)RefreshSampleData)
            .WithName("RefreshSampleData")
            .WithOpenApi();

        return app;
    }

    /// <summary>
    /// Gets paginated list of products with optional search and category filtering
    /// </summary>
    private static async Task<IResult> GetProducts(
        HttpContext context,
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        int? categoryId = null)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        try
        {
            // Normalize search term and validate pagination parameters
            var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            logger.LogDebug(
                "Retrieving products - Page: {PageNumber}, Size: {PageSize}, Search: '{Search}', CategoryId: {CategoryId}",
                pageNumber, pageSize, normalizedSearch, categoryId);

            // Use CacheService for caching
            var cacheService = context.RequestServices.GetRequiredService<CacheService>();
            var cacheKey = cacheService.BuildProductCacheKey(pageNumber, pageSize, normalizedSearch, categoryId);

            var paginatedList = await cacheService.GetOrCreateProductCacheAsync(cacheKey, async () =>
            {
                logger.LogInformation("Cache miss - retrieving products from database");
                var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
                var query = dbContext.Products.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                {
                    query = query.Where(p => EF.Functions.Like(p.Name, $"%{normalizedSearch}%"));
                }

                // Apply category filter
                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                query = query.OrderBy(p => p.Name);
                var totalCount = await query.CountAsync();
                var products = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PaginatedList<Product>
                {
                    Items = products,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };
            });

            logger.LogDebug("Retrieved {Count} products from cache/database", paginatedList.TotalCount);
            return Results.Ok(paginatedList);
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

    /// <summary>
    /// Gets a single product by ID
    /// </summary>
    private static async Task<IResult> GetProductById(HttpContext context, int id)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogDebug("Retrieving product {Id}", id);

            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
            var product = await dbContext.Products.FindAsync(id);

            if (product == null)
            {
                logger.LogWarning("Product {Id} not found", id);
                return Results.NotFound(new { message = $"Product with ID {id} not found" });
            }

            logger.LogDebug("Product {Id} retrieved successfully", id);
            return Results.Ok(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving product {Id}", id);
            return Results.Problem(
                title: "Error retrieving product",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    private static async Task<IResult> CreateProduct(HttpContext context, Product product)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogDebug("Creating product: {ProductName}", product.Name);

            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            // Invalidate product caches
            var cacheService = context.RequestServices.GetRequiredService<CacheService>();
            cacheService.InvalidateProductCaches();

            logger.LogInformation("Product created with ID {ProductId}", product.Id);

            return Results.Created($"/api/product/{product.Id}", product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product");
            return Results.Problem(
                title: "Error creating product",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    private static async Task<IResult> UpdateProduct(HttpContext context, int id, Product updatedProduct)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogDebug("Updating product {Id}", id);

            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
            var existingProduct = await dbContext.Products.FindAsync(id);

            if (existingProduct == null)
            {
                logger.LogWarning("Product {Id} not found for update", id);
                return Results.NotFound(new { message = $"Product with ID {id} not found" });
            }

            // Update properties
            existingProduct.Name = updatedProduct.Name;
            existingProduct.Description = updatedProduct.Description;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Stock = updatedProduct.Stock;
            existingProduct.CategoryId = updatedProduct.CategoryId;
            existingProduct.Category = updatedProduct.Category;

            await dbContext.SaveChangesAsync();

            // Invalidate product caches
            var cacheService = context.RequestServices.GetRequiredService<CacheService>();
            cacheService.InvalidateProductCaches();

            logger.LogInformation("Product {Id} updated successfully", id);

            return Results.Ok(existingProduct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product {Id}", id);
            return Results.Problem(
                title: "Error updating product",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    private static async Task<IResult> DeleteProduct(HttpContext context, int id)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogDebug("Deleting product {Id}", id);

            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
            var product = await dbContext.Products.FindAsync(id);

            if (product == null)
            {
                logger.LogWarning("Product {Id} not found for deletion", id);
                return Results.NotFound(new { message = $"Product with ID {id} not found" });
            }

            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();

            // Invalidate product caches
            var cacheService = context.RequestServices.GetRequiredService<CacheService>();
            cacheService.InvalidateProductCaches();

            logger.LogInformation("Product {Id} deleted successfully", id);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product {Id}", id);
            return Results.Problem(
                title: "Error deleting product",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Refreshes sample data by clearing and reseeding
    /// </summary>
    private static async Task<IResult> RefreshSampleData(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogDebug("Refreshing sample data");

            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

            // Clear existing products
            var existingProducts = await dbContext.Products.ToListAsync();
            dbContext.Products.RemoveRange(existingProducts);
            await dbContext.SaveChangesAsync();

            // Get fresh sample data and add new products
            var sampleProducts = SeedingService.GetSampleProducts();
            await dbContext.Products.AddRangeAsync(sampleProducts);
            await dbContext.SaveChangesAsync();

            // Invalidate product caches
            var cacheService = context.RequestServices.GetRequiredService<CacheService>();
            cacheService.InvalidateProductCaches();

            logger.LogInformation("Sample data refreshed successfully - {Count} products", sampleProducts.Length);

            return Results.Ok(new { message = "Sample data refreshed successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing sample data");
            return Results.Problem(
                title: "Error refreshing sample data",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
