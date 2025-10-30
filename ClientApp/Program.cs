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

// Register ProductsStateService as Singleton to maintain state across navigation
builder.Services.AddSingleton<ProductsStateService>();

// Register ErrorHandlerService for centralized error handling
builder.Services.AddScoped<ErrorHandlerService>();

// Register ToastService for success notifications
builder.Services.AddScoped<ToastService>();

await builder.Build().RunAsync();
