# Client Implementation Prompt for GitHub Copilot

## Project Context
You are implementing CRUD operations in a Blazor WebAssembly client application that communicates with an ASP.NET Core Minimal API backend. The application is called **InventoryHub** and manages product inventory.

### Server Information
- **Base URL:** `http://localhost:5132` (HTTP) or `https://localhost:7222` (HTTPS)
- **API Endpoints:**
  - `GET /api/products` - Returns paginated list of products
  - `GET /api/product/{id}` - Returns single product by ID
  - `POST /api/product` - Creates new product
  - `PUT /api/product/{id}` - Updates existing product
  - `DELETE /api/product/{id}` - Deletes product

### Available Server Models
- **Product:** Id, Name, Description, Price, Stock, Category, CategoryId, CreatedAt
- **Category:** Id, Name, CreatedAt
- **PaginatedList<T>:** Items, PageNumber, PageSize, TotalCount, TotalPages
- **CreateProductRequest:** Name, Price, Stock, Category
- **UpdateProductRequest:** Name, Price, Stock, Category

---

## Implementation Phases

### Phase 0: Prerequisites (COMPLETE FIRST - Required for Build Success)

**⚠️ IMPORTANT: Complete this phase before Phase 1 to ensure successful builds at each step.**

#### 0.1 Update _Imports.razor
Update `ClientApp/_Imports.razor` to add required namespace imports:

Add these lines at the end of the file:
```razor
@using ClientApp.Models
@using ClientApp.Services
@using ClientApp.Shared
```

The complete file should look like:
```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using ClientApp
@using ClientApp.Layout
@using ClientApp.Models
@using ClientApp.Services
@using ClientApp.Shared
```

#### 0.2 Update Program.cs with Logging and Services
Update `ClientApp/Program.cs` to configure logging and services:

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ClientApp;
using ClientApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add logging (required for ILogger<T> in services)
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure HttpClient to point to ServerApp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5132") 
});

// Register ProductService (will be added in Phase 1)
builder.Services.AddScoped<ProductService>();

await builder.Build().RunAsync();
```

**✅ Build Check:** After completing Phase 0.1, run `dotnet build` - it should succeed (ProductService not yet created, but namespace imports are ready).

---

### Phase 1: Foundation Setup

#### 1.1 Create Client-Side Models
Create the following model files in `ClientApp/Models/`:

**Product.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

public class Product
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }
    
    public Category Category { get; set; } = new();
    
    public int CategoryId { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

**Category.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
}
```

**PaginatedList.cs**
```csharp
namespace ClientApp.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

**CreateProductRequest.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

public class CreateProductRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    [Required]
    public Category Category { get; set; } = new();
}
```

**UpdateProductRequest.cs**
```csharp
using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models;

public class UpdateProductRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    [Required]
    public Category Category { get; set; } = new();
}
```

#### 1.2 Create ProductService
Create `ClientApp/Services/ProductService.cs`:

**✅ Build Check:** After creating all models in step 1.1, run `dotnet build` - it should succeed.

---

#### 1.3 Create ProductService

```csharp
using System.Net.Http.Json;
using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Service for managing product-related API communications
/// </summary>
/// <remarks>
/// Handles all CRUD operations for products with the ServerApp API.
/// Implements proper error handling and HTTP status code management.
/// </remarks>
public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductService> _logger;

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
    /// <returns>Paginated list of products</returns>
    public async Task<PaginatedList<Product>> GetProductsAsync(int pageNumber = 1, int pageSize = 12)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<PaginatedList<Product>>(
                $"/api/products?pageNumber={pageNumber}&pageSize={pageSize}");
            return response ?? new PaginatedList<Product>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching products from API");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a single product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product if found, null otherwise</returns>
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Product>($"/api/product/{id}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId} from API", id);
            throw;
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <returns>Created product</returns>
    public async Task<Product?> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/product", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">Product ID to update</param>
    /// <param name="request">Product update request</param>
    /// <returns>Updated product</returns>
    public async Task<Product?> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/product/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product ID to delete</param>
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
            throw;
        }
    }
}
```

**✅ Build Check:** After creating ProductService in step 1.3, run `dotnet build` - it should succeed. The service is now properly registered and all dependencies are available.

---

### Phase 2: Shared Components

**✅ Build Check:** Before starting Phase 2, verify Phase 1 builds successfully.

#### 2.1 Create ProductCard Component
Create `ClientApp/Shared/ProductCard.razor`:

```razor
@using ClientApp.Models

<div class="card mb-3 shadow-sm">
    <div class="card-body">
        <h5 class="card-title">@Product.Name</h5>
        <p class="card-text text-muted">@Product.Description</p>
        <div class="mb-2">
            <span class="badge bg-secondary">@Product.Category.Name</span>
        </div>
        <div class="d-flex justify-content-between align-items-center">
            <div>
                <strong class="text-success">$@Product.Price.ToString("F2")</strong>
                <span class="ms-2">Stock: @Product.Stock</span>
            </div>
            <button class="btn btn-primary btn-sm" @onclick="OnViewDetails">
                View Details
            </button>
        </div>
    </div>
</div>

@code {
    [Parameter]
    public Product Product { get; set; } = new();

    [Parameter]
    public EventCallback<int> OnDetailsClicked { get; set; }

    private async Task OnViewDetails()
    {
        await OnDetailsClicked.InvokeAsync(Product.Id);
    }
}
```

#### 2.2 Create ProductForm Component
Create `ClientApp/Shared/ProductForm.razor`:

```razor
@using ClientApp.Models

<EditForm Model="@CurrentProduct" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary class="alert alert-danger" />

    <div class="mb-3">
        <label for="name" class="form-label">Product Name</label>
        <InputText id="name" class="form-control" @bind-Value="CurrentProduct.Name" />
        <ValidationMessage For="@(() => CurrentProduct.Name)" />
    </div>

    <div class="mb-3">
        <label for="description" class="form-label">Description</label>
        <InputTextArea id="description" class="form-control" @bind-Value="CurrentProduct.Description" rows="3" />
        <ValidationMessage For="@(() => CurrentProduct.Description)" />
    </div>

    <div class="row">
        <div class="col-md-6 mb-3">
            <label for="price" class="form-label">Price</label>
            <InputNumber id="price" class="form-control" @bind-Value="CurrentProduct.Price" />
            <ValidationMessage For="@(() => CurrentProduct.Price)" />
        </div>

        <div class="col-md-6 mb-3">
            <label for="stock" class="form-label">Stock</label>
            <InputNumber id="stock" class="form-control" @bind-Value="CurrentProduct.Stock" />
            <ValidationMessage For="@(() => CurrentProduct.Stock)" />
        </div>
    </div>

    <div class="row">
        <div class="col-md-6 mb-3">
            <label for="categoryName" class="form-label">Category Name</label>
            <InputText id="categoryName" class="form-control" @bind-Value="CurrentProduct.Category.Name" />
            <ValidationMessage For="@(() => CurrentProduct.Category.Name)" />
        </div>

        <div class="col-md-6 mb-3">
            <label for="categoryId" class="form-label">Category ID</label>
            <InputNumber id="categoryId" class="form-control" @bind-Value="CurrentProduct.CategoryId" />
        </div>
    </div>

    <div class="mt-4">
        <button type="submit" class="btn btn-primary me-2">
            @(IsEditMode ? "Update Product" : "Create Product")
        </button>
        <button type="button" class="btn btn-secondary" @onclick="HandleCancel">
            Cancel
        </button>
    </div>
</EditForm>

@code {
    [Parameter]
    public Product Product { get; set; } = new();

    [Parameter]
    public EventCallback<Product> OnSubmit { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Parameter]
    public bool IsEditMode { get; set; }

    private Product CurrentProduct { get; set; } = new();

    protected override void OnParametersSet()
    {
        // Create a copy to avoid modifying the original
        CurrentProduct = new Product
        {
            Id = Product.Id,
            Name = Product.Name,
            Description = Product.Description,
            Price = Product.Price,
            Stock = Product.Stock,
            CategoryId = Product.CategoryId,
            Category = new Category 
            { 
                Id = Product.Category.Id, 
                Name = Product.Category.Name 
            }
        };
    }

    private async Task HandleSubmit()
    {
        await OnSubmit.InvokeAsync(CurrentProduct);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }
}
```

**✅ Build Check:** After creating both shared components (ProductCard and ProductForm), run `dotnet build` - it should succeed. All namespaces are properly imported via `_Imports.razor`.

---

### Phase 3: CRUD Pages

**✅ Build Check:** Before starting Phase 3, verify Phase 2 builds successfully.

#### 3.1 Create Products List Page
Create `ClientApp/Pages/Products.razor`:

```razor
@page "/products"
@using ClientApp.Models
@using ClientApp.Services
@inject ProductService ProductService
@inject NavigationManager Navigation

<PageTitle>Products - InventoryHub</PageTitle>

<div class="container mt-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1>Products</h1>
        <button class="btn btn-success" @onclick="NavigateToCreate">
            <i class="oi oi-plus"></i> Add New Product
        </button>
    </div>

    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            @errorMessage
            <button type="button" class="btn-close" @onclick="() => errorMessage = string.Empty"></button>
        </div>
    }

    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    }
    else if (products != null && products.Items.Any())
    {
        <div class="row">
            @foreach (var product in products.Items)
            {
                <div class="col-md-6 col-lg-4">
                    <ProductCard Product="@product" OnDetailsClicked="NavigateToDetails" />
                </div>
            }
        </div>

        <div class="mt-4">
            <p class="text-muted">
                Showing page @products.PageNumber of @products.TotalPages 
                (@products.TotalCount total products)
            </p>
        </div>
    }
    else
    {
        <div class="alert alert-info">
            No products found. Click "Add New Product" to create one.
        </div>
    }
</div>

@code {
    private PaginatedList<Product>? products;
    private bool isLoading = true;
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;
            products = await ProductService.GetProductsAsync(pageNumber: 1, pageSize: 20);
        }
        catch (HttpRequestException)
        {
            errorMessage = "Unable to connect to the server. Please ensure the server is running.";
        }
        catch (Exception ex)
        {
            errorMessage = $"An error occurred while loading products: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void NavigateToDetails(int productId)
    {
        Navigation.NavigateTo($"/product/{productId}");
    }

    private void NavigateToCreate()
    {
        Navigation.NavigateTo("/product/create");
    }
}
```

#### 3.2 Create Product Details Page
Create `ClientApp/Pages/ProductDetails.razor`:

```razor
@page "/product/{id:int}"
@using ClientApp.Models
@using ClientApp.Services
@inject ProductService ProductService
@inject NavigationManager Navigation

<PageTitle>Product Details - InventoryHub</PageTitle>

<div class="container mt-4">
    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger">@errorMessage</div>
    }

    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    }
    else if (product != null)
    {
        <div class="card">
            <div class="card-header">
                <h2>@product.Name</h2>
            </div>
            <div class="card-body">
                <dl class="row">
                    <dt class="col-sm-3">Description:</dt>
                    <dd class="col-sm-9">@product.Description</dd>

                    <dt class="col-sm-3">Price:</dt>
                    <dd class="col-sm-9 text-success">$@product.Price.ToString("F2")</dd>

                    <dt class="col-sm-3">Stock:</dt>
                    <dd class="col-sm-9">@product.Stock units</dd>

                    <dt class="col-sm-3">Category:</dt>
                    <dd class="col-sm-9">
                        <span class="badge bg-secondary">@product.Category.Name</span>
                    </dd>

                    <dt class="col-sm-3">Category ID:</dt>
                    <dd class="col-sm-9">@product.CategoryId</dd>

                    <dt class="col-sm-3">Created:</dt>
                    <dd class="col-sm-9">@product.CreatedAt.ToLocalTime().ToString("f")</dd>
                </dl>

                <div class="mt-4">
                    <button class="btn btn-primary me-2" @onclick="NavigateToEdit">
                        <i class="oi oi-pencil"></i> Edit
                    </button>
                    <button class="btn btn-danger me-2" @onclick="NavigateToDelete">
                        <i class="oi oi-trash"></i> Delete
                    </button>
                    <button class="btn btn-secondary" @onclick="NavigateToList">
                        <i class="oi oi-arrow-left"></i> Back to List
                    </button>
                </div>
            </div>
        </div>
    }
    else
    {
        <div class="alert alert-warning">
            Product not found.
        </div>
    }
</div>

@code {
    [Parameter]
    public int Id { get; set; }

    private Product? product;
    private bool isLoading = true;
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadProductAsync();
    }

    private async Task LoadProductAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;
            product = await ProductService.GetProductByIdAsync(Id);
        }
        catch (HttpRequestException)
        {
            errorMessage = "Unable to connect to the server. Please ensure the server is running.";
        }
        catch (Exception ex)
        {
            errorMessage = $"An error occurred while loading the product: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void NavigateToEdit()
    {
        Navigation.NavigateTo($"/product/edit/{Id}");
    }

    private void NavigateToDelete()
    {
        Navigation.NavigateTo($"/product/delete/{Id}");
    }

    private void NavigateToList()
    {
        Navigation.NavigateTo("/products");
    }
}
```

#### 3.3 Create Edit Product Page
Create `ClientApp/Pages/EditProduct.razor`:

```razor
@page "/product/edit/{id:int}"
@using ClientApp.Models
@using ClientApp.Services
@inject ProductService ProductService
@inject NavigationManager Navigation

<PageTitle>Edit Product - InventoryHub</PageTitle>

<div class="container mt-4">
    <h2>Edit Product</h2>

    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger">@errorMessage</div>
    }

    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    }
    else if (product != null)
    {
        <div class="card">
            <div class="card-body">
                <ProductForm Product="@product" 
                            OnSubmit="HandleUpdate" 
                            OnCancel="HandleCancel"
                            IsEditMode="true" />
            </div>
        </div>
    }
    else
    {
        <div class="alert alert-warning">
            Product not found.
        </div>
    }
</div>

@code {
    [Parameter]
    public int Id { get; set; }

    private Product? product;
    private bool isLoading = true;
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadProductAsync();
    }

    private async Task LoadProductAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;
            product = await ProductService.GetProductByIdAsync(Id);
        }
        catch (HttpRequestException)
        {
            errorMessage = "Unable to connect to the server. Please ensure the server is running.";
        }
        catch (Exception ex)
        {
            errorMessage = $"An error occurred while loading the product: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleUpdate(Product updatedProduct)
    {
        try
        {
            var request = new UpdateProductRequest
            {
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                Stock = updatedProduct.Stock,
                Category = updatedProduct.Category
            };

            await ProductService.UpdateProductAsync(Id, request);
            Navigation.NavigateTo("/products");
        }
        catch (HttpRequestException)
        {
            errorMessage = "Unable to update product. Please ensure the server is running.";
        }
        catch (Exception ex)
        {
            errorMessage = $"An error occurred while updating the product: {ex.Message}";
        }
    }

    private void HandleCancel()
    {
        Navigation.NavigateTo("/products");
    }
}
```

#### 3.4 Create Delete Product Page
Create `ClientApp/Pages/DeleteProduct.razor`:

```razor
@page "/product/delete/{id:int}"
@using ClientApp.Models
@using ClientApp.Services
@inject ProductService ProductService
@inject NavigationManager Navigation

<PageTitle>Delete Product - InventoryHub</PageTitle>

<div class="container mt-4">
    <div class="alert alert-warning">
        <h2><i class="oi oi-warning"></i> Delete Product</h2>
        <p>Are you sure you want to delete this product? This action cannot be undone.</p>
    </div>

    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger">@errorMessage</div>
    }

    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    }
    else if (product != null)
    {
        <div class="card">
            <div class="card-header">
                <h3>@product.Name</h3>
            </div>
            <div class="card-body">
                <dl class="row">
                    <dt class="col-sm-3">Description:</dt>
                    <dd class="col-sm-9">@product.Description</dd>

                    <dt class="col-sm-3">Price:</dt>
                    <dd class="col-sm-9">$@product.Price.ToString("F2")</dd>

                    <dt class="col-sm-3">Stock:</dt>
                    <dd class="col-sm-9">@product.Stock</dd>

                    <dt class="col-sm-3">Category:</dt>
                    <dd class="col-sm-9">@product.Category.Name</dd>
                </dl>

                <div class="mt-4">
                    <button class="btn btn-danger me-2" @onclick="HandleDelete" disabled="@isDeleting">
                        @if (isDeleting)
                        {
                            <span class="spinner-border spinner-border-sm me-1"></span>
                        }
                        <i class="oi oi-trash"></i> Confirm Delete
                    </button>
                    <button class="btn btn-secondary" @onclick="HandleCancel" disabled="@isDeleting">
                        Cancel
                    </button>
                </div>
            </div>
        </div>
    }
    else
    {
        <div class="alert alert-warning">
            Product not found.
        </div>
    }
</div>

@code {
    [Parameter]
    public int Id { get; set; }

    private Product? product;
    private bool isLoading = true;
    private bool isDeleting = false;
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadProductAsync();
    }

    private async Task LoadProductAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;
            product = await ProductService.GetProductByIdAsync(Id);
        }
        catch (HttpRequestException)
        {
            errorMessage = "Unable to connect to the server. Please ensure the server is running.";
        }
        catch (Exception ex)
        {
            errorMessage = $"An error occurred while loading the product: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleDelete()
    {
        try
        {
            isDeleting = true;
            errorMessage = string.Empty;
            await ProductService.DeleteProductAsync(Id);
            Navigation.NavigateTo("/products");
        }
        catch (HttpRequestException)
        {
            errorMessage = "Unable to delete product. Please ensure the server is running.";
            isDeleting = false;
        }
        catch (Exception ex)
        {
            errorMessage = $"An error occurred while deleting the product: {ex.Message}";
            isDeleting = false;
        }
    }

    private void HandleCancel()
    {
        Navigation.NavigateTo("/products");
    }
}
```

#### 3.5 Create New Product Page
Create `ClientApp/Pages/CreateProduct.razor`:

```razor
@page "/product/create"
@using ClientApp.Models
@using ClientApp.Services
@inject ProductService ProductService
@inject NavigationManager Navigation

<PageTitle>Create Product - InventoryHub</PageTitle>

<div class="container mt-4">
    <h2>Create New Product</h2>

    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger">@errorMessage</div>
    }

    <div class="card">
        <div class="card-body">
            <ProductForm Product="@newProduct" 
                        OnSubmit="HandleCreate" 
                        OnCancel="HandleCancel"
                        IsEditMode="false" />
        </div>
    </div>
</div>

@code {
    private Product newProduct = new();
    private string errorMessage = string.Empty;

    private async Task HandleCreate(Product product)
    {
        try
        {
            errorMessage = string.Empty;
            
            var request = new CreateProductRequest
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                Category = product.Category
            };

            var createdProduct = await ProductService.CreateProductAsync(request);
            
            if (createdProduct != null)
            {
                Navigation.NavigateTo($"/product/{createdProduct.Id}");
            }
            else
            {
                Navigation.NavigateTo("/products");
            }
        }
        catch (HttpRequestException)
        {
            errorMessage = "Unable to create product. Please ensure the server is running.";
        }
        catch (Exception ex)
        {
            errorMessage = $"An error occurred while creating the product: {ex.Message}";
        }
    }

    private void HandleCancel()
    {
        Navigation.NavigateTo("/products");
    }
}
```

**✅ Build Check:** After creating all 5 pages (Products, ProductDetails, EditProduct, DeleteProduct, CreateProduct), run `dotnet build` - it should succeed. All components and services are properly referenced.

---

### Phase 4: Navigation Updates

**✅ Build Check:** Before starting Phase 4, verify Phase 3 builds successfully.

#### 4.1 Update NavMenu
Update `ClientApp/Layout/NavMenu.razor` to add Products link:

Add this navigation item after the existing Home link:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="products">
        <span class="oi oi-list-rich" aria-hidden="true"></span> Products
    </NavLink>
</div>
```

**✅ Build Check:** After updating NavMenu, run `dotnet build` - it should succeed. 

**🎉 Final Verification:** Run `dotnet build` one more time to ensure the entire ClientApp builds successfully with all components integrated.

---

## Build Success Summary

After completing each phase, you should be able to build successfully:

- ✅ **After Phase 0:** Project builds (prerequisites in place)
- ✅ **After Phase 1:** Project builds (models and service created)
- ✅ **After Phase 2:** Project builds (shared components created)
- ✅ **After Phase 3:** Project builds (all pages created)
- ✅ **After Phase 4:** Project builds (navigation updated)

If any build fails, review the phase requirements and ensure:
1. All files are created in correct locations
2. Namespaces match folder structure
3. All dependencies are properly referenced
4. `_Imports.razor` has all required usings

---

## Testing Checklist

### Before Testing
1. ✅ Ensure ServerApp is running on http://localhost:5132
2. ✅ Build ClientApp without errors
3. ✅ Verify all files are created in correct locations

### Functional Tests
1. ✅ Navigate to /products - should display product list
2. ✅ Click "View Details" - should navigate to product details page
3. ✅ Click "Edit" button - should navigate to edit form with populated data
4. ✅ Update product - should save and redirect to products list
5. ✅ Click "Delete" button - should navigate to delete confirmation
6. ✅ Confirm delete - should remove product and redirect to list
7. ✅ Click "Add New Product" - should navigate to create form
8. ✅ Create product - should save and redirect to product details or list

### Error Handling Tests
1. ✅ Stop ServerApp - verify error messages display properly
2. ✅ Submit form with invalid data - verify validation messages
3. ✅ Navigate to non-existent product ID - verify 404 handling

### UI/UX Tests
1. ✅ Verify Bootstrap styling is applied correctly
2. ✅ Check loading spinners display during data fetch
3. ✅ Verify navigation links work correctly
4. ✅ Check responsive design on different screen sizes

---

## Common Issues and Solutions

### Issue: CORS Error
**Solution:** Verify ServerApp CORS policy includes ClientApp origin (http://localhost:5019 or your ClientApp port)

### Issue: HttpClient Not Configured
**Solution:** Ensure Program.cs has HttpClient with correct BaseAddress

### Issue: Models Don't Match
**Solution:** Verify client models match server DTOs exactly, especially property names (case-sensitive for JSON)

### Issue: Validation Not Working
**Solution:** Ensure DataAnnotationsValidator is added to EditForm and validation attributes match requirements

### Issue: Navigation Not Working
**Solution:** Verify @inject NavigationManager Navigation is added to pages

---

## Next Steps After Implementation

1. **Add Pagination Controls** - Allow users to navigate between pages
2. **Add Search/Filter** - Enable searching products by name or category
3. **Add Category Management** - Create CRUD pages for categories
4. **Improve Error Messages** - More specific error handling and user feedback
5. **Add Loading Skeletons** - Better loading states instead of spinners
6. **Add Toasts/Notifications** - Success messages after create/update/delete
7. **Add Confirmation Dialogs** - Modal confirmations instead of separate delete page
8. **Optimize Performance** - Add client-side caching, debouncing, etc.

---

## Code Quality Standards

- Use proper XML documentation comments on all public members
- Follow C# naming conventions (PascalCase for properties, camelCase for fields)
- Implement error handling in all API calls
- Use async/await consistently
- Keep components focused on single responsibility
- Use Bootstrap classes for consistent styling
- Implement proper parameter validation
- Use nullable reference types appropriately

