# Toast Notifications Implementation Guide

## Overview
Toast notifications provide non-intrusive user feedback for successful operations. They appear in the top-right corner of the screen, auto-dismiss after a few seconds, and can be manually dismissed by the user.

## Architecture

### Components Created

1. **ToastMessage.cs** - Model representing a toast notification
2. **ToastService.cs** - Service managing toast lifecycle
3. **ToastContainer.razor** - UI component displaying toasts
4. **Integration** - Added to all product pages for success feedback

## ToastMessage Model

```csharp
public class ToastMessage
{
    public string Id { get; set; }              // Unique identifier
    public string Message { get; set; }         // Main message text
    public ToastType Type { get; set; }         // Success/Info/Warning/Error
    public int Duration { get; set; }           // Auto-dismiss duration (ms)
    public DateTime CreatedAt { get; set; }     // Timestamp
}

public enum ToastType
{
    Success,
    Info,
    Warning,
    Error
}
```

## ToastService

### Methods

```csharp
// Show success toast (green, 3 seconds)
ToastService.ShowSuccess("Product created successfully!");

// Show info toast (blue, 3 seconds)
ToastService.ShowInfo("Loading data...");

// Show warning toast (yellow, 4 seconds)
ToastService.ShowWarning("Operation may take longer than usual.");

// Show error toast (red, 5 seconds)
ToastService.ShowError("Failed to connect to server.");

// Remove specific toast by ID
ToastService.RemoveToast(toastId);

// Clear all toasts
ToastService.ClearAll();
```

### Service Features

- ✅ **Auto-dismiss** - Toasts automatically disappear after duration
- ✅ **Manual dismiss** - Users can click × to close immediately
- ✅ **Event-driven** - Uses OnChange event for reactive updates
- ✅ **Multiple toasts** - Supports showing multiple toasts simultaneously
- ✅ **Type-specific styling** - Different colors and durations per type

## ToastContainer Component

### Placement
Added to `MainLayout.razor` at the bottom:
```razor
<ToastContainer />
```

### Features
- Fixed position in top-right corner
- High z-index (9999) to appear above all content
- Responsive design with proper spacing
- Shows timestamp ("just now", "5s ago", etc.)
- Bootstrap toast styling with custom type colors

### Visual Design

| Type | Icon | Background | Duration |
|------|------|------------|----------|
| Success | ✅ | Green | 3000ms |
| Info | ℹ️ | Blue | 3000ms |
| Warning | ⚠️ | Yellow | 4000ms |
| Error | ❌ | Red | 5000ms |

## Integration in Product Pages

### CreateProduct.razor
```csharp
@inject ToastService ToastService

private async Task HandleCreate(Product product)
{
    try
    {
        var createdProduct = await ProductService.CreateProductAsync(request);
        ToastService.ShowSuccess($"Product '{createdProduct.Name}' created successfully!");
        Navigation.NavigateTo($"/product/{createdProduct.Id}");
    }
    catch (Exception ex)
    {
        // Error handling...
    }
}
```

### EditProduct.razor
```csharp
@inject ToastService ToastService

private async Task HandleUpdate(Product updatedProduct)
{
    try
    {
        await ProductService.UpdateProductAsync(Id, request);
        ToastService.ShowSuccess($"Product '{updatedProduct.Name}' updated successfully!");
        Navigation.NavigateTo("/products");
    }
    catch (Exception ex)
    {
        // Error handling...
    }
}
```

### DeleteProduct.razor
```csharp
@inject ToastService ToastService

private async Task HandleDelete()
{
    try
    {
        var productName = product?.Name ?? "Product";
        await ProductService.DeleteProductAsync(Id);
        ToastService.ShowSuccess($"'{productName}' deleted successfully!");
        Navigation.NavigateTo("/products");
    }
    catch (Exception ex)
    {
        // Error handling...
    }
}
```

### Products.razor (Refresh Sample Data)
```csharp
@inject ToastService ToastService

private async Task RefreshSampleData()
{
    try
    {
        await ProductService.RefreshSampleDataAsync();
        ToastService.ShowSuccess("Sample data refreshed successfully!");
        await LoadProductsAsync();
    }
    catch (Exception ex)
    {
        // Error handling...
    }
}
```

## Usage Examples

### Basic Success Message
```csharp
ToastService.ShowSuccess("Operation completed!");
```

### Success with Product Name
```csharp
ToastService.ShowSuccess($"Product '{product.Name}' created successfully!");
```

### Info Notification
```csharp
ToastService.ShowInfo("Loading additional data...");
```

### Warning Message
```csharp
ToastService.ShowWarning("Connection is slow. This may take longer than usual.");
```

### Error Message (alternative to ErrorAlert)
```csharp
ToastService.ShowError("Unable to complete operation. Please try again.");
```

### Custom Duration
```csharp
// Show for 10 seconds
ToastService.ShowSuccess("Important message!", duration: 10000);

// Show indefinitely (until manually dismissed)
ToastService.ShowSuccess("Critical update available", duration: 0);
```

## Best Practices

### ✅ DO

**Use success toasts for completed actions:**
```csharp
// ✅ Good - confirms action completed
ToastService.ShowSuccess("Product created successfully!");
ToastService.ShowSuccess("Changes saved!");
ToastService.ShowSuccess("Item deleted!");
```

**Include relevant details:**
```csharp
// ✅ Good - tells user what was affected
ToastService.ShowSuccess($"Product '{product.Name}' created successfully!");
ToastService.ShowSuccess($"Updated {count} items");
```

**Keep messages concise:**
```csharp
// ✅ Good - short and clear
ToastService.ShowSuccess("Saved!");
ToastService.ShowSuccess("Product created!");

// ❌ Too verbose
ToastService.ShowSuccess("The product has been successfully created and saved to the database. You can now view it in the products list.");
```

**Use appropriate types:**
```csharp
// ✅ Good - matches the situation
ToastService.ShowSuccess("Operation completed!");      // Completed action
ToastService.ShowInfo("Processing in background...");  // Informational
ToastService.ShowWarning("Quota limit approaching");   // Warning
ToastService.ShowError("Connection failed");           // Error occurred
```

### ❌ DON'T

**Don't use for errors that need user action:**
```csharp
// ❌ Bad - error needs attention, use ErrorAlert instead
ToastService.ShowError("Validation failed: Name is required");

// ✅ Good - use ErrorAlert with retry
currentError = ErrorHandler.HandleException(ex, "creating product");
```

**Don't overuse toasts:**
```csharp
// ❌ Bad - too many toasts
ToastService.ShowInfo("Loading...");
await LoadData();
ToastService.ShowInfo("Processing...");
await ProcessData();
ToastService.ShowInfo("Saving...");
await SaveData();
ToastService.ShowSuccess("Complete!");

// ✅ Good - single toast at end
await LoadData();
await ProcessData();
await SaveData();
ToastService.ShowSuccess("All operations completed!");
```

**Don't use for long-running operations:**
```csharp
// ❌ Bad - toast will disappear before operation finishes
ToastService.ShowInfo("Uploading large file...");
await UploadLargeFile(); // Takes 30 seconds

// ✅ Good - use loading message instead
loadingMessage = "Uploading large file...";
await UploadLargeFile();
loadingMessage = string.Empty;
ToastService.ShowSuccess("File uploaded!");
```

## Toast vs ErrorAlert

### Use Toast When:
- ✅ Confirming successful operations
- ✅ Providing informational updates
- ✅ User doesn't need to take action
- ✅ Message is temporary and non-critical

### Use ErrorAlert When:
- ✅ Operation failed and needs user attention
- ✅ User should retry the operation
- ✅ Error message contains actionable guidance
- ✅ Error persists until user dismisses or fixes

### Example Comparison

**Toast for Success:**
```csharp
try
{
    await ProductService.CreateProductAsync(request);
    ToastService.ShowSuccess("Product created!");  // ✅ Success feedback
    Navigation.NavigateTo("/products");
}
```

**ErrorAlert for Failure:**
```csharp
catch (Exception ex)
{
    currentError = ErrorHandler.HandleException(ex, "creating product");  // ✅ Error needs attention
}
```

## Styling and Customization

### Current Styling
The ToastContainer uses Bootstrap toast classes with custom type-specific backgrounds:

```csharp
private string GetToastClass(ToastType type)
{
    return type switch
    {
        ToastType.Success => "bg-success text-white",
        ToastType.Info => "bg-info text-white",
        ToastType.Warning => "bg-warning text-dark",
        ToastType.Error => "bg-danger text-white",
        _ => "bg-light"
    };
}
```

### Custom CSS (Optional)
To add animations or custom styles, add to `app.css`:

```css
/* Smooth fade-in animation */
.toast {
    animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
    from {
        transform: translateX(100%);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

/* Fade-out animation on dismiss */
.toast.hiding {
    animation: slideOut 0.3s ease-in;
}

@keyframes slideOut {
    from {
        transform: translateX(0);
        opacity: 1;
    }
    to {
        transform: translateX(100%);
        opacity: 0;
    }
}
```

## Advanced Patterns

### Toast with Action Button
```csharp
public class ToastMessage
{
    // ... existing properties
    public string? ActionText { get; set; }
    public EventCallback OnActionClick { get; set; }
}

// Usage
ToastService.ShowWithAction(
    message: "Product created!",
    actionText: "View",
    onAction: () => Navigation.NavigateTo($"/product/{productId}")
);
```

### Toast Queue Management
```csharp
// Limit to 3 toasts at once
public class ToastService
{
    private const int MaxToasts = 3;
    
    private void ShowToast(string message, ToastType type, int duration)
    {
        // Remove oldest if at limit
        if (_toasts.Count >= MaxToasts)
        {
            var oldest = _toasts.First();
            _toasts.Remove(oldest);
        }
        
        // Add new toast
        _toasts.Add(new ToastMessage { ... });
        NotifyStateChanged();
    }
}
```

### Toast with Progress Bar
```csharp
public class ToastMessage
{
    // ... existing properties
    public int Progress { get; set; } // 0-100
}

// In component
<div class="progress" style="height: 3px;">
    <div class="progress-bar" style="width: @toast.Progress%"></div>
</div>
```

## Testing

### Manual Testing Checklist

**CreateProduct Page:**
- [ ] Create product successfully
- [ ] Toast appears with product name
- [ ] Toast auto-dismisses after 3 seconds
- [ ] Can manually dismiss toast
- [ ] Toast doesn't block navigation

**EditProduct Page:**
- [ ] Update product successfully
- [ ] Toast confirms update with product name
- [ ] Toast displays before navigation

**DeleteProduct Page:**
- [ ] Delete product successfully
- [ ] Toast shows deleted product name
- [ ] Toast appears after navigation to list

**Products Page:**
- [ ] Refresh sample data
- [ ] Success toast appears
- [ ] Toast doesn't interfere with product loading

**Multiple Toasts:**
- [ ] Can show multiple toasts simultaneously
- [ ] Toasts stack properly in top-right
- [ ] Each toast can be dismissed independently
- [ ] Toasts auto-dismiss at different times

**Accessibility:**
- [ ] Screen reader announces toast messages
- [ ] Close button is keyboard accessible
- [ ] Toast has proper ARIA roles

## Summary

The toast notification system provides:
- ✅ **User-friendly feedback** - Confirms successful operations
- ✅ **Non-intrusive** - Appears in corner, doesn't block UI
- ✅ **Auto-dismiss** - Disappears automatically
- ✅ **Flexible** - Supports multiple types and durations
- ✅ **Accessible** - Screen reader compatible
- ✅ **Easy to use** - Simple service API
- ✅ **Consistent** - Same pattern across all pages

Users now receive clear confirmation that their create, update, and delete operations completed successfully!
