# InventoryHub

A full-stack product inventory management application built with Blazor WebAssembly and ASP.NET Core Minimal API.

## Features

- Real-time product inventory management
- Responsive web interface with Bootstrap styling
- Client-side caching for improved performance
- Category-based product organization
- Built-in error handling and offline resilience

## Technology Stack

- **Frontend**: Blazor WebAssembly
- **Backend**: ASP.NET Core Minimal API
- **Styling**: Bootstrap
- **State Management**: Client-side caching with 5-minute duration
- **Performance**: Server-side in-memory caching and monitoring

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Visual Studio Code or Visual Studio 2022

### Installation

1. Clone the repository:
   ```powershell
   git clone https://github.com/dotnetmurf/FullStackFinalProject.git
   cd FullStackFinalProject/FullStackApp
   ```

2. Build the solution:
   ```powershell
   dotnet build
   ```

### Running the Application

You can run the application using either of these methods:

#### Method 1: Using separate terminals

```powershell
# Terminal 1 - Start the server
dotnet run --project ServerApp

# Terminal 2 - Start the client
dotnet run --project ClientApp
```

#### Method 2: Using VS Code tasks

Use the Command Palette (Ctrl+Shift+P) and select one of:
- "Run ServerApp" - Starts the server only
- "Run ClientApp" - Starts the client only
- "Run Both Server and Client" - Starts both components

### Project Structure

```
FullStackApp/
├── ClientApp/          # Blazor WebAssembly Client
│   ├── Pages/          # Razor components and pages
│   ├── Services/       # Client-side services
│   └── wwwroot/        # Static web assets
└── ServerApp/          # ASP.NET Core API
    └── Program.cs      # API endpoints and configuration
```

## Key Features

### Client-Side

- Efficient state management with caching
- Responsive UI with Bootstrap
- Error handling with user feedback
- Automatic data refresh

### Server-Side

- RESTful API endpoints
- In-memory caching
- Performance monitoring
- CORS configuration for client access

## Development

### Adding New Features

1. **New API Endpoints**
   - Add endpoints in `ServerApp/Program.cs`
   - Include proper error handling and caching

2. **New Client Features**
   - Create components in `ClientApp/Pages/`
   - Update services in `ClientApp/Services/`

### Best Practices

- Follow established documentation patterns
- Implement error handling consistently
- Maintain cache configuration
- Test features in offline scenarios

## License

This project is licensed under the MIT License - see the LICENSE file for details.
