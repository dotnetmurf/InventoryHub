using System;
using System.Collections.Generic;

namespace ServerApp.Models;

/// <summary>
/// Represents a paginated list of items
/// </summary>
/// <typeparam name="T">The type of items in the list</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// The current page number (1-based)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// The number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Whether there is a previous page available
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page available
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// The items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; init; } = new List<T>();
}