namespace ClientApp.Models;

/// <summary>
/// Represents a paginated collection of items
/// </summary>
/// <typeparam name="T">The type of items in the list</typeparam>
/// <remarks>
/// Used for handling large datasets by breaking them into pages,
/// includes metadata about the current page and total count
/// </remarks>
public class PaginatedList<T>
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// The current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indicates whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
