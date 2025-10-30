using System.ComponentModel.DataAnnotations;

namespace ServerApp.Models;

/// <summary>
/// Request model for creating a new product with validation rules
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// Name of the product to create
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Price of the product
    /// </summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    public decimal Price { get; set; }

    /// <summary>
    /// Initial stock quantity
    /// </summary>
    [Required(ErrorMessage = "Stock is required")]
    [Range(0, 999999, ErrorMessage = "Stock must be between 0 and 999,999")]
    public int Stock { get; set; }

    /// <summary>
    /// Foreign key reference to the category
    /// </summary>
    [Required(ErrorMessage = "Category ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Category information for the product
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    public Category Category { get; set; } = new();
    
    /// <summary>
    /// Validates that a valid category is selected
    /// </summary>
    public bool IsValid()
    {
        return Category != null && Category.Id > 0 && CategoryId > 0;
    }
}
