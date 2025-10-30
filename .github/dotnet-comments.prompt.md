# .NET Code Documentation Prompt Guide

## Overview
Use this prompt guide when asking Copilot to add documentation and comments to .NET application files. Follow the established patterns and XML documentation standards while maintaining clarity and usefulness.

## General Prompts

### Basic Documentation Request
```
Please add appropriate XML documentation and inline comments to this [controller/component/service/model] following .NET best practices. Include:
- XML documentation for public members
- Summary descriptions
- Parameter documentation
- Return value documentation
- Exception documentation where applicable
- Inline comments for complex logic
```

### Framework-Specific Prompts

#### ASP.NET Core Controllers
```
Please document this controller following REST API best practices. Include:
- Controller purpose and responsibility
- Endpoint descriptions
- Request/response models
- Authentication requirements
- Rate limiting details (if any)
- Possible response codes
```

#### Blazor Components
```
Please add documentation to this Blazor component including:
- Component purpose and usage
- Parameter descriptions
- Event callback explanations
- State management details
- Parent-child component relationships
- Lifecycle method purposes
```

#### Entity Framework Core
```
Please document this entity/DbContext/configuration including:
- Entity relationships
- Navigation properties
- Key configurations
- Index definitions
- Any special database constraints
- Soft delete or audit property purposes
```

#### Razor Pages/MVC Views
```
Please add comments to this Razor Page/View including:
- Page model binding
- Form handling
- View component usage
- Client-side validation
- JavaScript interop details
```

## Example Patterns

### Controller Documentation
```csharp
/// <summary>
/// Manages product-related operations including inventory and pricing
/// </summary>
/// <remarks>
/// Supports CRUD operations with authentication required for modifications
/// Rate limited to 100 requests per minute
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of active products
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page (default 20)</param>
    /// <returns>PagedResponse of ProductDto items</returns>
    /// <response code="200">Returns the product list</response>
    /// <response code="400">If pagination parameters are invalid</response>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
```

### Blazor Component Documentation
```csharp
/// <summary>
/// Displays an editable product card with real-time inventory updates
/// </summary>
/// <remarks>
/// Uses SignalR for live inventory tracking
/// Supports both read-only and edit modes
/// </remarks>
public partial class ProductCard : ComponentBase
{
    /// <summary>
    /// The product to display/edit
    /// </summary>
    [Parameter]
    public ProductDto Product { get; set; }

    /// <summary>
    /// Callback fired when product details are updated
    /// </summary>
    [Parameter]
    public EventCallback<ProductDto> OnProductUpdated { get; set; }
```

### Entity Documentation
```csharp
/// <summary>
/// Represents a product in the inventory system
/// </summary>
/// <remarks>
/// Products can be active or archived
/// Prices are stored in cents to avoid decimal precision issues
/// </remarks>
public class Product
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User-friendly product name, must be unique
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }
```

## Best Practices

### DO Document
1. Public APIs and interfaces
2. Complex business logic
3. Non-obvious implementation details
4. Security requirements
5. Performance considerations
6. Database constraints
7. Integration points
8. Validation rules
9. State management approaches
10. Error handling strategies

### DON'T Document
1. Obvious code
2. Implementation details likely to change
3. Information better expressed in code
4. Redundant information
5. Auto-implemented properties without special behavior

## Example Prompts

### For New Code
```
Please add comprehensive XML documentation to this new [file type]. Focus on:
- Public API surface
- Usage examples
- Integration points
- Security considerations
```

### For Existing Code
```
Please analyze this existing [file type] and add appropriate documentation while preserving any existing comments. Focus on:
- Missing XML documentation
- Complex logic explanation
- Business rule documentation
- Cross-component interactions
```

### For Generated Code
```
Please add documentation to this generated code focusing on:
- Usage patterns
- Customization points
- Generated vs manual sections
- Integration requirements
```

## Special Cases

### Middleware Documentation
```
Please document this middleware component including:
- Pipeline position requirements
- Configuration options
- Dependencies
- Error handling
- Performance impact
```

### Service Documentation
```
Please document this service implementation including:
- Dependency injection lifecycle
- External service dependencies
- Retry policies
- Cache behavior
- Error handling strategies
```

## Reference Links
- [Microsoft XML Documentation Guide](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)
- [ASP.NET Core API Documentation](https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)
- [Blazor Component Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/)