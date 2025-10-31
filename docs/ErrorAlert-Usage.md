# ErrorAlert Component Usage Guide

## Overview
The `ErrorAlert` component provides a consistent, user-friendly way to display errors throughout the application. It works seamlessly with the `ErrorHandlerService` to translate technical errors into actionable user messages.

## Features
- **Severity-Based Styling** - Different colors for Info, Warning, Error, and Critical
- **Dismissible** - Users can close the alert
- **Retry Support** - Optional retry button for retryable errors
- **Actionable Guidance** - Shows users what they can do to resolve the error
- **Icon Indicators** - Visual icons for quick severity recognition

## Basic Usage

### 1. Inject Dependencies
```razor
@inject ErrorHandlerService ErrorHandler
```

### 2. Add Component to Page
```razor
<ErrorAlert Error="@currentError" 
            OnDismiss="() => currentError = null" 
            OnRetry="RetryOperation" />
```

### 3. Handle Errors in Code
```razor
@code {
    private UserError? currentError;

    private async Task LoadData()
    {
        try
        {
            currentError = null;
            // Your API call here
            var data = await SomeService.GetDataAsync();
        }
        catch (Exception ex)
        {
            currentError = ErrorHandler.HandleException(ex, "loading data");
        }
    }

    private async Task RetryOperation()
    {
        await LoadData();
    }
}
```

## Complete Example

```razor
@page "/example"
@inject ProductService ProductService
@inject ErrorHandlerService ErrorHandler

<h3>Error Handling Example</h3>

<!-- Display error if one exists -->
<ErrorAlert Error="@currentError" 
            OnDismiss="() => currentError = null" 
            OnRetry="LoadProducts" />

@if (products != null)
{
    <div class="alert alert-success">Loaded @products.Count products</div>
}

<button class="btn btn-primary" @onclick="LoadProducts">Load Products</button>

@code {
    private UserError? currentError;
    private List<Product>? products;

    private async Task LoadProducts()
    {
        try
        {
            currentError = null;
            products = await ProductService.GetProductsAsync();
        }
        catch (Exception ex)
        {
            currentError = ErrorHandler.HandleException(ex, "loading products");
        }
    }
}
```

## Error Types and Their Display

### Connection Error (Red/Danger)
```
❌ Connection Error
Unable to connect to the server.

What you can do:
• The server is running (http://localhost:5132)
• Your internet connection is active
• No firewall is blocking the connection

[🔄 Try Again]
```

### Not Found (Blue/Info)
```
ℹ️ Not Found
The requested product was not found.

What you can do:
The item may have been deleted or the link may be incorrect.
```

### Timeout (Yellow/Warning)
```
⚠️ Request Timeout
The operation took too long to complete.

What you can do:
Please try again. If the problem persists, check your internet connection.

[🔄 Try Again]
```

## Component Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `Error` | `UserError?` | Yes | The error to display (null hides the alert) |
| `OnDismiss` | `EventCallback` | No | Called when user clicks the close button |
| `OnRetry` | `EventCallback` | No | Called when user clicks the retry button (only shown if error is retryable) |

## Styling

The component uses Bootstrap 5 alert classes:
- **Info** → `alert-info` (blue)
- **Warning** → `alert-warning` (yellow)
- **Error** → `alert-danger` (red)
- **Critical** → `alert-danger` (red)

## Best Practices

1. **Always clear errors before retrying**
   ```csharp
   private async Task RetryOperation()
   {
       currentError = null; // Clear the error
       await LoadData();    // Try again
   }
   ```

2. **Provide context to ErrorHandler**
   ```csharp
   // Good - specific context
   ErrorHandler.HandleException(ex, "loading products");
   
   // Less helpful
   ErrorHandler.HandleException(ex, "error");
   ```

3. **Use appropriate placement**
   - Place near the top of the page for page-level errors
   - Place near the affected component for local errors

4. **Don't show multiple errors simultaneously**
   - Clear previous errors before showing new ones
   - Consider using only one error state per page

## Integration with Existing Code

To update existing error handling:

**Before:**
```razor
@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger">@errorMessage</div>
}

@code {
    private string errorMessage = string.Empty;
    
    catch (Exception ex)
    {
        errorMessage = $"An error occurred: {ex.Message}";
    }
}
```

**After:**
```razor
<ErrorAlert Error="@currentError" 
            OnDismiss="() => currentError = null" 
            OnRetry="RetryLastOperation" />

@code {
    private UserError? currentError;
    private Func<Task>? lastFailedOperation;
    
    catch (Exception ex)
    {
        currentError = ErrorHandler.HandleException(ex, "loading data");
        lastFailedOperation = LoadData; // Store for retry
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
