using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace ServerApp.Services;

/// <summary>
/// Centralized cache management service for product data
/// </summary>
/// <remarks>
/// Manages in-memory caching with automatic key tracking and invalidation.
/// Uses a registry pattern to track all cache keys for efficient bulk invalidation.
/// Cache duration: 5 minutes absolute, 2 minutes sliding expiration.
/// </remarks>
public class CacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly ConcurrentBag<string> _productCacheKeys = new();
    
    // Cache configuration constants
    private static readonly TimeSpan AbsoluteCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SlidingCacheDuration = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Initializes a new instance of the CacheService
    /// </summary>
    /// <param name="cache">Memory cache instance</param>
    /// <param name="logger">Logger for cache operations</param>
    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Builds a consistent cache key for product queries
    /// </summary>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Optional search term for filtering</param>
    /// <param name="categoryId">Optional category ID for filtering</param>
    /// <returns>Formatted cache key string</returns>
    public string BuildProductCacheKey(int pageNumber, int pageSize, string? searchTerm, int? categoryId)
    {
        var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
        return $"products_page{pageNumber}_size{pageSize}_search{normalizedSearch}_cat{categoryId}";
    }

    /// <summary>
    /// Attempts to get a cached value
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Retrieved value if found</param>
    /// <returns>True if value was found in cache, false otherwise</returns>
    public bool TryGetValue<T>(string key, out T? value)
    {
        var found = _cache.TryGetValue(key, out value);
        
        if (found)
        {
            _logger.LogDebug("Cache HIT for key: {CacheKey}", key);
        }
        else
        {
            _logger.LogDebug("Cache MISS for key: {CacheKey}", key);
        }
        
        return found;
    }

    /// <summary>
    /// Sets a value in cache with product-specific expiration settings
    /// </summary>
    /// <typeparam name="T">Type of value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    public void SetProductCache<T>(string key, T value)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = AbsoluteCacheDuration,
            SlidingExpiration = SlidingCacheDuration
        };

        _productCacheKeys.Add(key);
        _cache.Set(key, value, cacheOptions);
        
        _logger.LogDebug("Cached value with key: {CacheKey} (Total product keys: {KeyCount})", 
            key, _productCacheKeys.Count);
    }

    /// <summary>
    /// Gets a cached value or creates it using the provided factory function
    /// </summary>
    /// <typeparam name="T">Type of value to retrieve or create</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Function to create value if not cached</param>
    /// <returns>Cached or newly created value</returns>
    public async Task<T> GetOrCreateProductCacheAsync<T>(string key, Func<Task<T>> factory)
    {
        if (TryGetValue<T>(key, out var cachedValue) && cachedValue != null)
        {
            return cachedValue;
        }

        _logger.LogInformation("Cache miss - executing factory function for key: {CacheKey}", key);
        var value = await factory();
        SetProductCache(key, value);
        
        return value;
    }

    /// <summary>
    /// Invalidates all product-related cache entries
    /// </summary>
    /// <remarks>
    /// Removes all cached product data and clears the key registry.
    /// Should be called after any product modifications (create, update, delete).
    /// </remarks>
    public void InvalidateProductCaches()
    {
        var keysToRemove = _productCacheKeys.ToList();
        var removedCount = 0;

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            removedCount++;
        }

        _productCacheKeys.Clear();
        
        _logger.LogInformation("Invalidated {Count} product cache entries", removedCount);
    }

    /// <summary>
    /// Gets the current count of tracked product cache keys
    /// </summary>
    /// <returns>Number of active product cache keys</returns>
    public int GetProductCacheKeyCount()
    {
        return _productCacheKeys.Count;
    }
}
