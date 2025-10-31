# Copilot Instructions for InventoryHub

## Related Instruction Files

- [Application Guidelines](./blazorwasm-webapi.instructions.md) - Detailed instructions for application-building conventions
- [Comment Guidelines](./dotnet-comments.prompt.md) - Detailed instructions for code commenting conventions

## Project Overview
InventoryHub is a full-stack solution with a Blazor WebAssembly client (`ClientApp`) and an ASP.NET Core minimal API server (`ServerApp`). The solution is organized for clear separation of concerns and maintainability.

## Architecture & Major Components
- **ClientApp/**: Blazor WebAssembly front-end. Handles UI, state management, and API calls.
  - `Pages/`: Razor pages for product CRUD, details, and navigation.
  - `Services/`: State and API communication (e.g., `ProductService`, `ProductsStateService`).
  - `Models/`: DTOs and error handling types for client-side logic.
  - `Shared/`: Reusable UI components (e.g., `ProductCard`, `ToastContainer`).
- **ServerApp/**: ASP.NET Core minimal API backend.
  - `Endpoints/`: Defines API endpoints (e.g., `ProductEndpoints`).
  - `Services/`: Business logic, validation, caching, and database seeding.
  - `Data/`: Entity Framework Core `AppDbContext` for data access.
  - `Models/`: Shared DTOs and domain models.
  - `Middleware/`: Custom middleware (e.g., `PerformanceMiddleware`).

## Developer Workflows
- **Build**: Use Visual Studio or run `dotnet build InventoryHub.sln` from the solution root.
- **Run**: Use Visual Studio F5 or `dotnet run --project ServerApp/ServerApp.csproj` (server) and serve `ClientApp` via the built-in Blazor dev server.
- **Test**: No test projects detected; add tests in `ServerApp.Tests` or `ClientApp.Tests` if needed.
- **Debug**: Use Visual Studio's debugger for both client and server. For API debugging, use `ServerApp.http` for sample requests.

## Project-Specific Patterns & Conventions
- **API Communication**: Client uses `ProductService` to call server endpoints defined in `ProductEndpoints`. Data flows via DTOs in `Models/`.
- **Error Handling**: Centralized via `ErrorHandlerService` (client) and `ValidationService` (server). Errors are surfaced in UI via `ToastService` and `ErrorAlert`.
- **Pagination**: Implemented using `PaginatedList` and `PaginationParams` models on both client and server.
- **Seeding & Initialization**: `DbInitializerService` and `SeedingService` handle initial data setup on server startup.
- **Styling**: Uses Bootstrap (see `wwwroot/lib/bootstrap/` and custom CSS in `wwwroot/css/app.css`).
- **Shared Models**: Many DTOs are duplicated in both `ClientApp/Models` and `ServerApp/Models` for decoupling; keep them in sync when updating contracts.

## Integration Points
- **EF Core**: Data access via `AppDbContext`.
- **Blazor**: UI logic and state management in `ClientApp`.
- **API**: Minimal API endpoints in `ServerApp/Endpoints`.
- **Static Assets**: Served from `wwwroot/` in both client and server.

## Examples
- To add a new product feature:
  1. Update/create DTOs in both `ClientApp/Models` and `ServerApp/Models`.
  2. Add endpoint logic in `ServerApp/Endpoints/ProductEndpoints.cs`.
  3. Update client service (`ProductService.cs`) and relevant Razor pages/components.

## Key Files & Directories
- `InventoryHub.sln`: Solution entry point.
- `ClientApp/Services/ProductService.cs`: API communication pattern.
- `ServerApp/Endpoints/ProductEndpoints.cs`: Example of minimal API endpoint.
- `ServerApp/Data/AppDbContext.cs`: EF Core setup.
- `ClientApp/Shared/ToastContainer.razor`: UI error/success notification pattern.
- `SharedModels/`: Shared DTOs for client-server contract consistency.
- `REFACTORING.md`: Comprehensive refactoring recommendations and improvement opportunities.

## Known Technical Debt
- DTOs duplicated in ClientApp/Models and ServerApp/Models (migration to SharedModels in progress)
- Business logic mixed with HTTP handling in ProductEndpoints.cs (consider extracting to service layer)
- Validation logic scattered across codebase (consolidation recommended)
- CORS policy set to AllowAnyOrigin (should be restricted in production)

---
_For detailed refactoring recommendations, see `REFACTORING.md`. If any section is unclear or missing important project-specific details, please provide feedback to improve these instructions._
