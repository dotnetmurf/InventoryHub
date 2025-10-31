using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using SharedModels;

namespace ServerApp.Data;

/// <summary>
/// Database context for the InventoryHub application
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the AppDbContext
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the products in the database
    /// </summary>
    public DbSet<SharedModels.Product> Products { get; set; }

    /// <summary>
    /// Configures the database model
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<SharedModels.Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.OwnsOne(e => e.Category);
        });
    }
}


