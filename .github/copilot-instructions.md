# AI Agent Instructions for InventoryHub

## Related Instruction Files

- [Application Guidelines](./blazorwasm-webapi.instructions.md) - Detailed instructions for application-building conventions
- [Comment Guidelines](./dotnet-comments.prompt.md) - Detailed instructions for code commenting conventions

## Project Overview
InventoryHub is a full-stack product inventory management application built with Blazor WebAssembly (client) and ASP.NET Core Minimal API (server). The application follows a clean separation between client and server components with optimized data flow patterns.

## Architecture Patterns

### Client-Side (Blazor WebAssembly)
- Entry point: `ClientApp/Program.cs` configures DI and root components
- Services follow scoped lifetime pattern (one instance per user session)
- Components live in `ClientApp/Pages/` with corresponding route patterns
- State management uses client-side caching with 5-minute duration

### Server-Side (ASP.NET Core Minimal API)
- Entry point: `ServerApp/Program.cs` defines API endpoints and middleware
- Uses in-memory caching with 5-minute expiration for performance
- Product data structure defined in `ServerApp/Models/Product.cs`
- Implements performance monitoring and structured error handling

## Key Development Workflows

### Running the Application

Running components separately (preferred for development):
```powershell
dotnet run --project ServerApp  # Terminal 1
dotnet run --project ClientApp  # Terminal 2
```

### Data Flow Patterns
- Client-server communication uses HTTP client with base address configuration
- Products include nested category information for efficient data transfer
- Caching implemented at both client and server levels for performance
- Error states should be handled and displayed to users

## Project-Specific Conventions

### Code Organization
- Backend models in `ServerApp/Models/`
- Frontend services in `ClientApp/Services/`
- Blazor components in `ClientApp/Pages/`
- Shared layout components in `ClientApp/Layout/`

### Performance Optimizations
- Server implements in-memory caching for product data
- Client uses scoped services for session management
- Performance monitoring integrated in server endpoints
- Categories are nested within product data to minimize requests

## Integration Points
- Client-server boundary defined by HTTP API endpoints in `ServerApp/Program.cs`
- CORS configured for local development communication
- Client configuration injected through `ClientApp/Program.cs`
- Bootstrap styling integrated through `wwwroot/lib/bootstrap/`