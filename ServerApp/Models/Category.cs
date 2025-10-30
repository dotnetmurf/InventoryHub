using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServerApp.Models;

/// <summary>
/// Represents a category in the inventory system
/// </summary>
/// <remarks>
/// Categories are used to organize products into logical groups
/// and facilitate product management and filtering
/// </remarks>
public class Category
{
    /// <summary>
    /// Unique identifier for the category
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the category
    /// </summary>
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-&]+$", ErrorMessage = "Category name can only contain letters, numbers, spaces, hyphens, and ampersands")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the category was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}