using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

/// <summary>
/// Request model for creating a new product
/// </summary>
/// <remarks>
/// Contains the required data fields for product creation.
/// Validation rules ensure data integrity before submission to the API.
/// </remarks>
public class CreateProductRequest
{
    /// <summary>
    /// Name of the product to create
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Price of the product
    /// </summary>
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// Initial stock quantity
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    /// <summary>
    /// Foreign key reference to the category
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Category information for the product
    /// </summary>
    [Required]
    public Category Category { get; set; } = new();
}
