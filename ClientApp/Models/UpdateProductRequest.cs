using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

/// <summary>
/// Request model for updating an existing product
/// </summary>
/// <remarks>
/// Contains the data fields that can be modified for an existing product.
/// Validation rules ensure data integrity before submission to the API.
/// </remarks>
public class UpdateProductRequest
{
    /// <summary>
    /// Updated name of the product
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Updated description of the product
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Updated price of the product
    /// </summary>
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// Updated stock quantity
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    /// <summary>
    /// Foreign key reference to the updated category
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Updated category information
    /// </summary>
    [Required]
    public Category Category { get; set; } = new();
}
