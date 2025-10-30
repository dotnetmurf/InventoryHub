namespace ServerApp.Models;

/// <summary>
/// Parameters for paginated requests
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 12;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page (defaults to 10, max 50)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}