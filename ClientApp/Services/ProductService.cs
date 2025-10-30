using System.Net;
using System.Net.Http.Json;
using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Service for managing product-related API communications
/// </summary>
/// <remarks>
/// Handles all CRUD operations for products with the ServerApp API.
/// Implements proper error handling and HTTP status code management.
/// Uses dependency injection for HttpClient and logging services.
/// </remarks>
public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the ProductService
    /// </summary>
    /// <param name="httpClient">HTTP client configured with ServerApp base address</param>
    /// <param name="logger">Logger for tracking service operations</param>
    public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of products from the API
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Optional search term to filter products by name</param>
    /// <param name="categoryId">Optional category ID to filter products by category</param>
    /// <returns>Paginated list of products</returns>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    public async Task<PaginatedList<Product>> GetProductsAsync(int pageNumber = 1, int pageSize = 12, string? searchTerm = null, int? categoryId = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            }

            if (categoryId.HasValue)
            {
                queryParams.Add($"categoryId={categoryId.Value}");
            }

            var queryString = string.Join("&", queryParams);
            var response = await _httpClient.GetFromJsonAsync<PaginatedList<Product>>($"/api/products?{queryString}");
            return response ?? new PaginatedList<Product>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching products from API");
            
            var context = new Dictionary<string, object>
            {
                ["PageNumber"] = pageNumber,
                ["PageSize"] = pageSize
            };
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
                context["SearchTerm"] = searchTerm;
            
            if (categoryId.HasValue)
                context["CategoryId"] = categoryId.Value;
            
            // Use appropriate constructor based on whether we have a status code
            if (ex.StatusCode.HasValue)
            {
                throw new ProductServiceException(
                    "Failed to retrieve products from the server.",
                    "GetProducts",
                    ex.StatusCode.Value,
                    ex,
                    null,
                    context
                );
            }
            else
            {
                throw new ProductServiceException(
                    "Failed to retrieve products from the server.",
                    "GetProducts",
                    ex
                );
            }
        }
    }

    /// <summary>
    /// Retrieves a single product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product if found</returns>
    /// <exception cref="ProductServiceException">Thrown when product not found or API request fails</exception>
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/product/{id}");
            
            // Throw ProductServiceException for 404 (product not found)
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var context = new Dictionary<string, object>
                {
                    ["ProductId"] = id
                };
                
                throw new ProductServiceException(
                    $"Product with ID {id} was not found.",
                    "GetProductById",
                    HttpStatusCode.NotFound,
                    responseBody: null,
                    context: context
                );
            }
            
            // Throw for other error status codes
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (ProductServiceException)
        {
            // Re-throw ProductServiceException as-is
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId} from API", id);
            
            var context = new Dictionary<string, object>
            {
                ["ProductId"] = id
            };
            
            if (ex.StatusCode.HasValue)
            {
                throw new ProductServiceException(
                    $"Failed to retrieve product with ID {id}.",
                    "GetProductById",
                    ex.StatusCode.Value,
                    ex,
                    null,
                    context
                );
            }
            else
            {
                throw new ProductServiceException(
                    $"Failed to retrieve product with ID {id}.",
                    "GetProductById",
                    ex
                );
            }
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">Product creation request with required fields</param>
    /// <returns>Created product with assigned ID</returns>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    /// <exception cref="ValidationException">Thrown when validation fails with detailed error information</exception>
    public async Task<Product?> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/product", request);
            
            if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Try to parse validation errors
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ValidationException(errorContent);
            }
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error creating product");
            
            var context = new Dictionary<string, object>
            {
                ["ProductName"] = request.Name ?? "Unknown"
            };
            
            if (ex.StatusCode.HasValue)
            {
                throw new ProductServiceException(
                    "Failed to create product.",
                    "CreateProduct",
                    ex.StatusCode.Value,
                    ex,
                    null,
                    context
                );
            }
            else
            {
                throw new ProductServiceException(
                    "Failed to create product.",
                    "CreateProduct",
                    ex
                );
            }
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product ID to update</param>
    /// <param name="request">Product update request with modified fields</param>
    /// <returns>Updated product</returns>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    /// <exception cref="ValidationException">Thrown when validation fails with detailed error information</exception>
    public async Task<Product?> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/product/{id}", request);
            
            if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Try to parse validation errors
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ValidationException(errorContent);
            }
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            
            var context = new Dictionary<string, object>
            {
                ["ProductId"] = id,
                ["ProductName"] = request.Name ?? "Unknown"
            };
            
            if (ex.StatusCode.HasValue)
            {
                throw new ProductServiceException(
                    $"Failed to update product with ID {id}.",
                    "UpdateProduct",
                    ex.StatusCode.Value,
                    ex,
                    null,
                    context
                );
            }
            else
            {
                throw new ProductServiceException(
                    $"Failed to update product with ID {id}.",
                    "UpdateProduct",
                    ex
                );
            }
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product ID to delete</param>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    public async Task DeleteProductAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/product/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            
            var context = new Dictionary<string, object>
            {
                ["ProductId"] = id
            };
            
            if (ex.StatusCode.HasValue)
            {
                throw new ProductServiceException(
                    $"Failed to delete product with ID {id}.",
                    "DeleteProduct",
                    ex.StatusCode.Value,
                    ex,
                    null,
                    context
                );
            }
            else
            {
                throw new ProductServiceException(
                    $"Failed to delete product with ID {id}.",
                    "DeleteProduct",
                    ex
                );
            }
        }
    }

    /// <summary>
    /// Retrieves all available categories from the API
    /// </summary>
    /// <returns>Array of available categories</returns>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    public async Task<Category[]> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Category[]>("/api/categories");
            return response ?? Array.Empty<Category>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching categories from API");
            
            if (ex.StatusCode.HasValue)
            {
                throw new ProductServiceException(
                    "Failed to retrieve categories from the server.",
                    "GetCategories",
                    ex.StatusCode.Value,
                    ex
                );
            }
            else
            {
                throw new ProductServiceException(
                    "Failed to retrieve categories from the server.",
                    "GetCategories",
                    ex
                );
            }
        }
    }

    /// <summary>
    /// Refreshes the database with fresh sample data
    /// </summary>
    /// <returns>True if refresh was successful, false otherwise</returns>
    /// <exception cref="HttpRequestException">Thrown when API request fails</exception>
    public async Task<bool> RefreshSampleDataAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/products/refresh", null);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error refreshing sample data");
            
            if (ex.StatusCode.HasValue)
            {
                throw new ProductServiceException(
                    "Failed to refresh sample data.",
                    "RefreshSampleData",
                    ex.StatusCode.Value,
                    ex
                );
            }
            else
            {
                throw new ProductServiceException(
                    "Failed to refresh sample data.",
                    "RefreshSampleData",
                    ex
                );
            }
        }
    }
}
