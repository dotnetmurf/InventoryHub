# Retry Mechanism Implementation Guide

## Overview
The retry mechanism allows users to easily retry failed operations without reloading the page or re-entering data. It integrates seamlessly with the ErrorHandlerService and ErrorAlert component.

## How It Works

### 1. Store the Failed Operation
When an operation fails, store a reference to it for retry:

```csharp
private Func<Task>? lastFailedOperation;

private async Task LoadProductsAsync()
{
    lastFailedOperation = LoadProductsAsync; // Store for retry
    
    try
    {
        currentError = null;
        products = await ProductService.GetProductsAsync(...);
    }
    catch (Exception ex)
    {
        currentError = ErrorHandler.HandleException(ex, "loading products");
    }
}
```

### 2. Implement Retry Handler
Create a simple retry method that executes the stored operation:

```csharp
/// <summary>
/// Retries the last failed operation
/// </summary>
private async Task RetryLastOperation()
{
    if (lastFailedOperation != null)
    {
        await lastFailedOperation();
    }
}
```

### 3. Wire Up to ErrorAlert
Pass the retry handler to the ErrorAlert component:

```razor
<ErrorAlert Error="@currentError" 
            OnDismiss="() => currentError = null" 
            OnRetry="RetryLastOperation" />
```

## Complete Example

```razor
@page "/products"
@inject ProductService ProductService
@inject ErrorHandlerService ErrorHandler

<h1>Products</h1>

<!-- Error display with retry button -->
<ErrorAlert Error="@currentError" 
            OnDismiss="() => currentError = null" 
            OnRetry="RetryLastOperation" />

@if (isLoading)
{
    <p>Loading...</p>
}
else if (products != null)
{
    <div>
        @foreach (var product in products)
        {
            <div>@product.Name</div>
        }
    </div>
}

@code {
    private List<Product>? products;
    private bool isLoading;
    private UserError? currentError;
    private Func<Task>? lastFailedOperation;

    protected override async Task OnInitializedAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        lastFailedOperation = LoadProductsAsync; // Store for retry
        
        try
        {
            isLoading = true;
            currentError = null; // Clear previous errors
            products = await ProductService.GetProductsAsync();
        }
        catch (Exception ex)
        {
            currentError = ErrorHandler.HandleException(ex, "loading products");
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task RetryLastOperation()
    {
        if (lastFailedOperation != null)
        {
            await lastFailedOperation();
        }
    }
}
```

## User Experience Flow

### Normal Operation
1. User visits page
2. Data loads successfully
3. No error shown

### With Error and Retry
1. User visits page
2. Network error occurs
3. ErrorAlert shows: **"❌ Connection Error"** with message
4. Retry button appears (because error is retryable)
5. User clicks **"🔄 Try Again"**
6. `RetryLastOperation()` is called
7. `LoadProductsAsync()` executes again
8. If successful, error clears and data displays
9. If fails again, error updates with new context

## Best Practices

### ✅ DO

**Clear errors before retrying:**
```csharp
private async Task LoadData()
{
    lastFailedOperation = LoadData;
    
    try
    {
        currentError = null; // ✅ Clear old error
        data = await Service.GetDataAsync();
    }
    catch (Exception ex)
    {
        currentError = ErrorHandler.HandleException(ex, "loading data");
    }
}
```

**Store operation before try block:**
```csharp
private async Task SaveProduct(Product product)
{
    lastFailedOperation = async () => await SaveProduct(product); // ✅ Capture parameters
    
    try
    {
        await ProductService.SaveAsync(product);
    }
    catch (Exception ex)
    {
        currentError = ErrorHandler.HandleException(ex, "saving product");
    }
}
```

**Use specific contexts:**
```csharp
// ✅ Good - helps user understand what failed
ErrorHandler.HandleException(ex, "loading products");
ErrorHandler.HandleException(ex, "saving changes");
ErrorHandler.HandleException(ex, "deleting product");

// ❌ Bad - not helpful
ErrorHandler.HandleException(ex, "error");
ErrorHandler.HandleException(ex, "operation");
```

### ❌ DON'T

**Don't store operations after try block:**
```csharp
try
{
    data = await Service.GetDataAsync();
    lastFailedOperation = LoadData; // ❌ Too late - already succeeded
}
```

**Don't forget to clear errors:**
```csharp
try
{
    // ❌ Missing: currentError = null;
    data = await Service.GetDataAsync();
}
```

**Don't retry non-idempotent operations without user confirmation:**
```csharp
// ❌ Bad - could create duplicate data
private async Task CreateProduct()
{
    lastFailedOperation = CreateProduct; // Dangerous without extra checks
    await ProductService.CreateAsync(product);
}

// ✅ Better - check if operation completed
private async Task CreateProduct()
{
    if (productCreated) return; // Don't retry if already created
    
    lastFailedOperation = CreateProduct;
    await ProductService.CreateAsync(product);
    productCreated = true;
}
```

## Advanced Patterns

### Multiple Operations Per Page
Track which operation failed:

```csharp
private Func<Task>? lastFailedOperation;
private string? lastOperationName;

private async Task LoadProducts()
{
    lastFailedOperation = LoadProducts;
    lastOperationName = "Load Products";
    // ... operation
}

private async Task LoadCategories()
{
    lastFailedOperation = LoadCategories;
    lastOperationName = "Load Categories";
    // ... operation
}
```

### Retry with Parameters
Capture parameters in lambda:

```csharp
private async Task DeleteProduct(int productId)
{
    lastFailedOperation = async () => await DeleteProduct(productId);
    
    try
    {
        currentError = null;
        await ProductService.DeleteAsync(productId);
    }
    catch (Exception ex)
    {
        currentError = ErrorHandler.HandleException(ex, "deleting product");
    }
}
```

### Retry Counter
Limit retry attempts:

```csharp
private int retryCount = 0;
private const int MaxRetries = 3;

private async Task LoadProductsAsync()
{
    if (retryCount >= MaxRetries)
    {
        currentError = new UserError
        {
            Title = "Maximum Retries Exceeded",
            Message = "Unable to load products after multiple attempts.",
            ActionMessage = "Please refresh the page or contact support.",
            Severity = ErrorSeverity.Error,
            IsRetryable = false
        };
        return;
    }

    lastFailedOperation = LoadProductsAsync;
    retryCount++;
    
    try
    {
        currentError = null;
        products = await ProductService.GetProductsAsync();
        retryCount = 0; // Reset on success
    }
    catch (Exception ex)
    {
        currentError = ErrorHandler.HandleException(ex, "loading products");
    }
}
```

### Exponential Backoff
Add delay between retries:

```csharp
private int retryCount = 0;

private async Task RetryLastOperation()
{
    if (lastFailedOperation != null)
    {
        // Wait longer with each retry: 1s, 2s, 4s, 8s...
        if (retryCount > 0)
        {
            var delayMs = Math.Pow(2, retryCount - 1) * 1000;
            await Task.Delay((int)delayMs);
        }
        
        await lastFailedOperation();
    }
}
```

## Testing Retry Behavior

### Manual Testing Steps

1. **Test Connection Error:**
   - Stop the server
   - Try to load products
   - Verify error appears with retry button
   - Start server
   - Click retry
   - Verify products load

2. **Test Multiple Retries:**
   - Stop server
   - Try operation, click retry (fails again)
   - Verify error updates
   - Start server, click retry
   - Verify success

3. **Test Non-Retryable Errors:**
   - Navigate to non-existent product ID
   - Verify 404 error shows WITHOUT retry button

## Integration with Existing Code

### Before (Old Error Handling)
```razor
@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger">
        @errorMessage
        <button class="btn-close" @onclick="() => errorMessage = string.Empty"></button>
    </div>
}

@code {
    private string errorMessage = string.Empty;
    
    private async Task LoadData()
    {
        try
        {
            data = await Service.GetDataAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
    }
}
```

### After (With Retry Mechanism)
```razor
<ErrorAlert Error="@currentError" 
            OnDismiss="() => currentError = null" 
            OnRetry="RetryLastOperation" />

@code {
    private UserError? currentError;
    private Func<Task>? lastFailedOperation;
    
    private async Task LoadData()
    {
        lastFailedOperation = LoadData; // Add this line
        
        try
        {
            currentError = null; // Change: clear error
            data = await Service.GetDataAsync();
        }
        catch (Exception ex)
        {
            currentError = ErrorHandler.HandleException(ex, "loading data"); // Change: use handler
        }
    }
    
    // Add this method
    private async Task RetryLastOperation()
    {
        if (lastFailedOperation != null)
        {
            await lastFailedOperation();
        }
    }
}
```

## Summary

The retry mechanism provides:
- ✅ **One-click retry** for failed operations
- ✅ **User-friendly** error messages with guidance
- ✅ **Automatic retry eligibility** based on error type
- ✅ **Simple implementation** with minimal code
- ✅ **Consistent UX** across all pages
- ✅ **Parameter preservation** for operations that need data

Users can now recover from transient errors (network issues, timeouts) without refreshing the page or losing their work!
