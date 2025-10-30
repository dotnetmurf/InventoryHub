# Copilot Implementation Guide for InventoryHub Updates

## Overview
This guide provides step-by-step instructions for implementing CRUD operations in the InventoryHub Blazor WebAssembly application. Follow these steps in order to ensure proper implementation and dependency management.

## Implementation Sequence

### Phase 0: Server API Documentation

1. OpenAPI Configuration
```markdown
- Add Swagger/OpenAPI NuGet packages:
  * Swashbuckle.AspNetCore
  * Swashbuckle.AspNetCore.Annotations
- Configure Swagger in Program.cs:
  * Add SwaggerGen service
  * Configure SwaggerUI
  * Set API version and info
  * Enable XML comments
```

2. API Documentation
```markdown
- Enable XML documentation in ServerApp.csproj
- Add API endpoint documentation:
  * Operation descriptions
  * Parameter descriptions
  * Response types
  * Example requests/responses
- Configure Swagger UI theme and layout
- Add security definitions (if needed)
```

3. Response Types Configuration
```markdown
- Define standard response types
- Add ProducesResponseType attributes
- Document error responses
- Configure example responses
```

4. Custom Swagger Configuration
```markdown
- Add custom response schemas
- Configure operation filters
- Add custom UI branding
- Configure API grouping
```

5. Swagger UI Implementation
```markdown
- Create ServerApp/wwwroot folder (if not exists)
- Create index.html with Swagger UI:
  ```html
  <!DOCTYPE html>
  <html>
  <head>
      <meta charset="utf-8" />
      <title>InventoryHub API Documentation</title>
      <link rel="stylesheet" type="text/css" href="./swagger-ui.css" />
      <link rel="icon" type="image/png" href="./favicon-32x32.png" sizes="32x32" />
      <link rel="icon" type="image/png" href="./favicon-16x16.png" sizes="16x16" />
      <style>
          html { box-sizing: border-box; overflow: -moz-scrollbars-vertical; overflow-y: scroll; }
          *, *:before, *:after { box-sizing: inherit; }
          body { margin: 0; background: #fafafa; }
          .swagger-ui .topbar { background-color: #000; }
          .swagger-ui .topbar .download-url-wrapper { display: none; }
          .swagger-ui .info { margin: 30px 0; }
          .swagger-ui .scheme-container { background-color: #fff; }
      </style>
  </head>
  <body>
      <div id="swagger-ui"></div>
      <script src="./swagger-ui-bundle.js"></script>
      <script src="./swagger-ui-standalone-preset.js"></script>
      <script>
          window.onload = function() {
              const ui = SwaggerUIBundle({
                  url: "/swagger/v1/swagger.json",
                  dom_id: '#swagger-ui',
                  deepLinking: true,
                  presets: [
                      SwaggerUIBundle.presets.apis,
                      SwaggerUIStandalonePreset
                  ],
                  plugins: [
                      SwaggerUIBundle.plugins.DownloadUrl
                  ],
                  layout: "StandaloneLayout",
                  docExpansion: "list",
                  defaultModelsExpandDepth: 1,
                  defaultModelExpandDepth: 1,
                  displayRequestDuration: true,
                  filter: true
              });
              window.ui = ui;
          };
      </script>
  </body>
  </html>
  ```

- Configure static files middleware in Program.cs:
  ```csharp
  app.UseStaticFiles();
  ```

- Update launchSettings.json to open Swagger UI:
  ```json
  {
    "profiles": {
      "http": {
        "launchUrl": "swagger"
      }
    }
  }
  ```

- Add Swagger UI security (if needed):
  * Add authentication requirements
  * Configure CORS for Swagger endpoints
  * Add API key configuration
```

### Phase 1: Foundation Setup

1. Project Structure Setup
```markdown
- Create ClientApp/Models folder
- Create ClientApp/Components folder
- Create ClientApp/Shared/Components folder
- Create ClientApp/Services folder (if not exists)
```

2. Data Model Implementation
```markdown
- Move Product class from ProductService.cs to Models/Product.cs
- Update model to match server DTOs
- Add validation attributes
- Add XML documentation
- Create any supporting models (e.g., PaginatedList<T>)
```

3. Service Layer Implementation
```markdown
- Update ProductService.cs with CRUD operations:
  * GetProducts() with pagination/filtering
  * GetProduct(int id)
  * CreateProduct(Product product)
  * UpdateProduct(int id, Product product)
  * DeleteProduct(int id)
- Add caching mechanism (5-minute duration)
- Implement retry policy
- Add error handling wrapper
- Add XML documentation
```

4. Configure Dependencies
```markdown
- Update Program.cs with service registrations
- Configure HttpClient with base URL
- Set up logging services
- Register any required JS interop services
```

### Phase 2: Core Components

1. Base Components (in Shared/Components)
```markdown
a. LoadingIndicator.razor
- Implement Bootstrap spinner
- Add size variants
- Add custom styling options
- Add fade in/out animations

b. StatusModal.razor
- Create Bootstrap modal template
- Add success/error styling
- Implement auto-dismiss timer
- Add navigation callback
- Support custom content

c. ValidationMessage.razor
- Create reusable validation display
- Support field and form-level errors
- Add Bootstrap styling
- Handle different validation scenarios
```

2. Product Components (in Components)
```markdown
a. ProductCard.razor
- Create product summary display
- Add responsive layout
- Implement action buttons
- Handle event callbacks
- Add loading states

b. ProductForm.razor
- Create reusable form component
- Implement client validation
- Add server validation display
- Handle form submission
- Support create/edit modes
- Add loading states

c. ProductGrid.razor
- Implement grid layout
- Add sorting functionality
- Add filtering controls
- Implement pagination
- Use ProductCard components
- Handle loading states

d. ProductDetails.razor
- Create detailed view component
- Add action buttons
- Implement loading states
- Handle errors gracefully

e. ProductDeleteConfirm.razor
- Create confirmation dialog
- Show product summary
- Add action buttons
- Handle deletion process
- Show loading states
```

### Phase 3: Page Implementation

1. Route and Navigation Setup
```markdown
- Configure routes in Program.cs
- Update NavMenu.razor with product links
- Implement parameter handling
- Add navigation guards
```

2. Product Pages (in Pages folder)

```markdown
a. Products.razor (List Page)
- Compose using ProductGrid component
- Add page title and description
- Add Create New button
- Implement search functionality
- Handle state management
- Integrate error handling

b. Product.razor (Details Page)
- Use ProductDetails component
- Add page navigation
- Handle parameter loading
- Implement error states
- Add navigation buttons

c. EditProduct.razor
- Use ProductForm component
- Handle route parameters
- Implement save logic
- Show status messages
- Add navigation
- Handle validation states

d. DeleteProduct.razor
- Use ProductDeleteConfirm component
- Handle confirmation flow
- Show status messages
- Implement navigation
- Handle error states
```

3. Component Integration
```markdown
- Ensure consistent styling
- Implement proper event bubbling
- Handle component lifecycle
- Optimize state management
- Implement error boundaries
```

### Phase 4: Error Handling

1. Global Error Handler
```markdown
- Implement error interceptor
- Define error message templates
- Set up error logging
- Configure retry policies
```

### Phase 5: Testing

1. Unit Tests
```markdown
- Test Product model validation
- Test ProductService methods
- Test component logic
- Test error handling
```

2. Integration Tests
```markdown
- Test API communication
- Test navigation flow
- Test error scenarios
- Test loading states
```

## Implementation Notes

### Styling Guidelines
```markdown
- Use Bootstrap 5.x classes
- Follow consistent spacing
- Implement responsive design
- Use appropriate color schemes for status messages
```

### Error Handling Guidelines
```markdown
- Use try-catch blocks in service methods
- Implement user-friendly error messages
- Log detailed error information
- Handle network errors gracefully
```

### Documentation Requirements
```markdown
- Add XML comments for all public members
- Document component parameters
- Document service methods
- Include usage examples
```

### Performance Considerations
```markdown
- Implement caching where appropriate
- Use async/await properly
- Minimize state updates
- Optimize component rendering
```

## Code Patterns

### API Documentation Pattern
```csharp
/// <summary>
/// Operation description
/// </summary>
/// <param name="parameter">Parameter description</param>
/// <response code="200">Success response description</response>
/// <response code="400">Bad request description</response>
/// <response code="404">Not found description</response>
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[Produces("application/json")]
public async Task<IResult> Operation([FromRoute] int parameter)
{
    // Operation implementation
}
```

### Service Method Pattern
```csharp
public async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation)
{
    // Implement retry logic
    // Handle exceptions
    // Return result or throw appropriate exception
}
```

### Component Parameter Pattern
```csharp
@code {
    [Parameter] public Type PropertyName { get; set; }
    [Parameter] public EventCallback<Type> OnEventName { get; set; }
}
```

### Error Handling Pattern
```csharp
try
{
    // Operation code
}
catch (Exception ex)
{
    // Log error
    // Show user-friendly message
    // Update state appropriately
}
```

## Testing Patterns

### Unit Test Pattern
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

### Integration Test Pattern
```csharp
[Fact]
public async Task Component_Scenario_ExpectedBehavior()
{
    // Setup
    // Simulate user action
    // Verify result
}
```