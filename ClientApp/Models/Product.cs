using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

/// <summary>
/// Represents a product in the inventory system
/// </summary>
/// <remarks>
/// This class serves as the primary model for product data in the client,
/// containing all essential product information including category details
/// </remarks>
public class Product
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the product
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Price of the product in the system's currency
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    /// <summary>
    /// Current stock level of the product
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }

    /// <summary>
    /// Category classification of the product
    /// </summary>
    public Category Category { get; set; } = new();

    /// <summary>
    /// Foreign key reference to the category
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
    public int CategoryId { get; set; }

    /// <summary>
    /// The timestamp when the product was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
