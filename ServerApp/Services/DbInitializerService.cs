using Microsoft.EntityFrameworkCore;
using ServerApp.Data;

namespace ServerApp.Services;

/// <summary>
/// Service for initializing the database with seed data
/// </summary>
public static class DbInitializerService
{
    /// <summary>
    /// Initializes the database with seed data if it's empty
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Create database if it doesn't exist (not needed for InMemory, but good practice)
        await context.Database.EnsureCreatedAsync();
        
        // Check if database is empty
        if (!await context.Products.AnyAsync())
        {
            // Get seed data from SeedingService
            var products = SeedingService.GetSampleProducts();
            
            // Add products to context
            await context.Products.AddRangeAsync(products);
            
            // Save changes to database
            await context.SaveChangesAsync();
        }
    }
}