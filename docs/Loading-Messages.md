# Context-Aware Loading Messages Implementation Guide

## Overview
Context-aware loading messages provide users with specific feedback about what operation is in progress, improving the user experience by setting clear expectations and reducing perceived wait time.

## Implementation Pattern

### Replace Boolean with String
Instead of a simple `isLoading` boolean, use a `loadingMessage` string:

```csharp
// ❌ Before: Generic loading state
private bool isLoading = true;

// ✅ After: Context-aware loading message
private string loadingMessage = string.Empty;
```

### Display the Message
Show both a spinner and the contextual message:

```razor
@if (!string.IsNullOrEmpty(loadingMessage))
{
    <div class="text-center">
        <div class="spinner-border" role="status">
            <span class="visually-hidden">@loadingMessage</span>
        </div>
        <p class="mt-2 text-muted">@loadingMessage</p>
    </div>
}
```

### Set Context-Specific Messages
Update your async operations to set appropriate messages:

```csharp
private async Task LoadDataAsync()
{
    try
    {
        loadingMessage = "Loading products...";  // Set before operation
        data = await Service.GetDataAsync();
    }
    catch (Exception ex)
    {
        // Handle error
    }
    finally
    {
        loadingMessage = string.Empty;  // Clear after operation
    }
}
```

## Products.razor Implementation

### Context-Aware Message Logic

The `GetLoadingMessage()` helper method provides intelligent messages based on user actions:

```csharp
/// <summary>
/// Gets context-aware loading message based on active filters
/// </summary>
private string GetLoadingMessage()
{
    var hasSearch = !string.IsNullOrWhiteSpace(StateService.SearchTerm);
    var hasCategory = StateService.CategoryId.HasValue;
    
    if (hasSearch && hasCategory && StateService.CategoryId.HasValue)
    {
        var categoryName = categories?.FirstOrDefault(c => c.Id == StateService.CategoryId.Value)?.Name ?? "category";
        return $"Searching '{StateService.SearchTerm}' in {categoryName}...";
    }
    else if (hasSearch)
    {
        return $"Searching for '{StateService.SearchTerm}'...";
    }
    else if (hasCategory && StateService.CategoryId.HasValue)
    {
        var categoryName = categories?.FirstOrDefault(c => c.Id == StateService.CategoryId.Value)?.Name ?? "category";
        return $"Loading {categoryName} products...";
    }
    else
    {
        return "Loading products...";
    }
}
```

### Usage Scenarios

| User Action | Loading Message Displayed |
|-------------|--------------------------|
| Initial page load | "Initializing..." |
| Load all products | "Loading products..." |
| Search by name | "Searching for 'laptop'..." |
| Filter by category | "Loading Electronics products..." |
| Search + filter | "Searching 'laptop' in Electronics..." |
| Refresh data | "Refreshing sample data..." |

## Complete Example

```razor
@page "/products"
@inject ProductService ProductService

<PageTitle>Products</PageTitle>

<h1>Products</h1>

@if (!string.IsNullOrEmpty(loadingMessage))
{
    <div class="text-center">
        <div class="spinner-border" role="status">
            <span class="visually-hidden">@loadingMessage</span>
        </div>
        <p class="mt-2 text-muted">@loadingMessage</p>
    </div>
}
else if (products != null)
{
    <!-- Display products -->
}

@code {
    private List<Product>? products;
    private string loadingMessage = string.Empty;
    private string searchTerm = string.Empty;
    private int? categoryId;

    protected override async Task OnInitializedAsync()
    {
        loadingMessage = "Initializing...";
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            // Set context-aware message
            loadingMessage = GetLoadingMessage();
            
            products = await ProductService.GetProductsAsync(searchTerm, categoryId);
        }
        catch (Exception ex)
        {
            // Handle error
        }
        finally
        {
            loadingMessage = string.Empty;
        }
    }

    private string GetLoadingMessage()
    {
        if (!string.IsNullOrWhiteSpace(searchTerm) && categoryId.HasValue)
        {
            return $"Searching '{searchTerm}' in category {categoryId}...";
        }
        else if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            return $"Searching for '{searchTerm}'...";
        }
        else if (categoryId.HasValue)
        {
            return $"Loading category {categoryId} products...";
        }
        else
        {
            return "Loading products...";
        }
    }
}
```

## Best Practices

### ✅ DO

**Use specific, action-oriented messages:**
```csharp
// ✅ Good - tells user exactly what's happening
loadingMessage = "Searching for 'laptop'...";
loadingMessage = "Loading Electronics products...";
loadingMessage = "Refreshing sample data...";
loadingMessage = "Saving changes...";
loadingMessage = "Deleting product...";
```

**Clear message after operation:**
```csharp
try
{
    loadingMessage = "Loading products...";
    products = await ProductService.GetProductsAsync();
}
finally
{
    loadingMessage = string.Empty; // ✅ Always clear
}
```

**Show both spinner and text:**
```razor
<!-- ✅ Good - visual + textual feedback -->
<div class="text-center">
    <div class="spinner-border" role="status">
        <span class="visually-hidden">@loadingMessage</span>
    </div>
    <p class="mt-2 text-muted">@loadingMessage</p>
</div>
```

**Include search terms and filters in message:**
```csharp
// ✅ Good - helps user understand what they'll see
if (hasSearch && hasCategory)
{
    return $"Searching '{searchTerm}' in {categoryName}...";
}
```

### ❌ DON'T

**Don't use generic messages:**
```csharp
// ❌ Bad - not helpful
loadingMessage = "Loading...";
loadingMessage = "Please wait...";
loadingMessage = "Processing...";
```

**Don't forget to clear the message:**
```csharp
try
{
    loadingMessage = "Loading products...";
    products = await ProductService.GetProductsAsync();
    // ❌ Missing: loadingMessage = string.Empty;
}
```

**Don't set messages for instant operations:**
```csharp
// ❌ Bad - causes unnecessary flicker
private void NavigateToDetails(int id)
{
    loadingMessage = "Navigating..."; // Too fast to see
    Navigation.NavigateTo($"/product/{id}");
}
```

**Don't use technical terms:**
```csharp
// ❌ Bad - confusing for users
loadingMessage = "Executing HTTP GET request...";
loadingMessage = "Deserializing JSON response...";
loadingMessage = "Awaiting async task completion...";

// ✅ Good - user-friendly
loadingMessage = "Loading products...";
loadingMessage = "Searching products...";
loadingMessage = "Saving changes...";
```

## Advanced Patterns

### Progress Indicators
For long operations, show progress:

```csharp
private string loadingMessage = string.Empty;
private int currentProgress = 0;
private int totalProgress = 0;

private async Task ProcessItemsAsync()
{
    totalProgress = items.Count;
    
    for (int i = 0; i < items.Count; i++)
    {
        currentProgress = i + 1;
        loadingMessage = $"Processing item {currentProgress} of {totalProgress}...";
        await ProcessItem(items[i]);
    }
    
    loadingMessage = string.Empty;
}
```

### Elapsed Time Display
Show duration for long operations:

```csharp
private string loadingMessage = string.Empty;
private DateTime? loadingStartTime;
private Timer? elapsedTimer;

private async Task LoadLargeDatasetAsync()
{
    try
    {
        loadingStartTime = DateTime.Now;
        loadingMessage = "Loading large dataset...";
        
        // Update elapsed time every second
        elapsedTimer = new Timer(_ => 
        {
            var elapsed = DateTime.Now - loadingStartTime;
            loadingMessage = $"Loading large dataset... ({elapsed.Value.Seconds}s)";
            InvokeAsync(StateHasChanged);
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        
        data = await Service.GetLargeDatasetAsync();
    }
    finally
    {
        elapsedTimer?.Dispose();
        loadingMessage = string.Empty;
        loadingStartTime = null;
    }
}
```

### Multiple Concurrent Operations
Track multiple loading states:

```csharp
private Dictionary<string, string> loadingMessages = new();

private string GetCombinedLoadingMessage()
{
    if (loadingMessages.Any())
    {
        return string.Join(" | ", loadingMessages.Values);
    }
    return string.Empty;
}

private async Task LoadProducts()
{
    try
    {
        loadingMessages["products"] = "Loading products";
        await ProductService.GetProductsAsync();
    }
    finally
    {
        loadingMessages.Remove("products");
    }
}

private async Task LoadCategories()
{
    try
    {
        loadingMessages["categories"] = "Loading categories";
        await ProductService.GetCategoriesAsync();
    }
    finally
    {
        loadingMessages.Remove("categories");
    }
}
```

## Accessibility Considerations

### Screen Reader Support
The loading message is included in the spinner's `visually-hidden` span for screen readers:

```razor
<div class="spinner-border" role="status">
    <span class="visually-hidden">@loadingMessage</span>
</div>
```

### ARIA Live Regions
For dynamic updates, use ARIA live regions:

```razor
<div aria-live="polite" aria-atomic="true">
    @if (!string.IsNullOrEmpty(loadingMessage))
    {
        <p class="text-muted">@loadingMessage</p>
    }
</div>
```

### Focus Management
Don't trap focus during loading:

```razor
<!-- ✅ Good - no focus trap -->
@if (!string.IsNullOrEmpty(loadingMessage))
{
    <div class="text-center">
        <div class="spinner-border" role="status">
            <span class="visually-hidden">@loadingMessage</span>
        </div>
        <p class="mt-2 text-muted">@loadingMessage</p>
    </div>
}

<!-- ❌ Bad - blocks interaction -->
@if (!string.IsNullOrEmpty(loadingMessage))
{
    <div class="modal-backdrop show">
        <div class="spinner-border"></div>
    </div>
}
```

## Testing Loading Messages

### Manual Testing Checklist

- [ ] Initial page load shows "Initializing..."
- [ ] Loading all products shows "Loading products..."
- [ ] Searching shows "Searching for '[term]'..."
- [ ] Filtering by category shows "Loading [Category] products..."
- [ ] Combined search + filter shows both terms
- [ ] Refresh shows "Refreshing sample data..."
- [ ] Messages clear after operation completes
- [ ] Messages update when filters change
- [ ] Screen reader announces messages
- [ ] Loading state doesn't block navigation

### Simulating Slow Networks

Test with throttled network to see messages clearly:

1. Open browser DevTools
2. Go to Network tab
3. Select "Slow 3G" or "Fast 3G" throttling
4. Perform operations and verify messages appear
5. Verify messages are helpful and accurate

## Migration Guide

### Before (Boolean Loading State)

```razor
@if (isLoading)
{
    <div class="spinner-border"></div>
}

@code {
    private bool isLoading = true;
    
    private async Task LoadData()
    {
        try
        {
            isLoading = true;
            data = await Service.GetDataAsync();
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

### After (Context-Aware Messages)

```razor
@if (!string.IsNullOrEmpty(loadingMessage))
{
    <div class="text-center">
        <div class="spinner-border" role="status">
            <span class="visually-hidden">@loadingMessage</span>
        </div>
        <p class="mt-2 text-muted">@loadingMessage</p>
    </div>
}

@code {
    private string loadingMessage = string.Empty;
    
    private async Task LoadData()
    {
        try
        {
            loadingMessage = "Loading data...";
            data = await Service.GetDataAsync();
        }
        finally
        {
            loadingMessage = string.Empty;
        }
    }
}
```

## Summary

Context-aware loading messages provide:
- ✅ **Better UX** - Users know exactly what's happening
- ✅ **Reduced anxiety** - Clear feedback reduces perceived wait time
- ✅ **Debugging aid** - Easier to identify slow operations
- ✅ **Accessibility** - Screen reader compatible
- ✅ **Professionalism** - More polished user experience

Users appreciate knowing:
- **What** is loading ("products", "categories", "search results")
- **Why** it's loading (their search term, selected filter)
- **Progress** if possible (1 of 10 items processed)

This creates a more transparent and user-friendly application!
