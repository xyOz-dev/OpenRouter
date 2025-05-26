# Advanced Topic: Dependency Injection Setup

The OpenRouter.NET SDK is designed with Dependency Injection (DI) in mind, making it easy to integrate into applications that use `Microsoft.Extensions.DependencyInjection` (common in ASP.NET Core, .NET Generic Host, and other modern .NET architectures).

## Core Registration: `AddOpenRouter()`

The primary extension method for registering the SDK's services is [`AddOpenRouter()`](../../OpenRouter/Extensions/ServiceCollectionExtensions.cs:1). This method is available on `IServiceCollection`.

**What it does:**
1.  **Registers `OpenRouterOptions`**: Configures and registers `OpenRouterOptions` within the DI container. You typically provide an `Action<OpenRouterOptions>` delegate or bind to `IConfiguration`.
2.  **Registers `IOpenRouterClient` / `OpenRouterClient`**: Registers the main client ([`OpenRouterClient`](../../OpenRouter/Core/OpenRouterClient.cs:1)) with a scoped lifetime by default. This means a new instance is created per scope (e.g., per HTTP request in ASP.NET Core).
3.  **Registers HTTP Client Dependencies**:
    *   Sets up an `HttpClient` specifically for OpenRouter using `IHttpClientFactory`. This `HttpClient` is configured with:
        *   The `BaseUrl` from `OpenRouterOptions`.
        *   Default headers (like `Authorization` for API key, `User-Agent`, `HttpReferer`, `XTitle`).
        *   A Polly retry policy for transient fault handling.
    *   Registers `IHttpClientProvider` ([`OpenRouterHttpClient`](../../OpenRouter/Http/OpenRouterHttpClient.cs:1)).
4.  **Registers Individual Services**: Registers all the core services (like [`IChatService`](../../OpenRouter/Services/Chat/IChatService.cs:1), [`IModelsService`](../../OpenRouter/Services/Models/IModelsService.cs:1), etc.) with a scoped lifetime. These services will resolve the `IOpenRouterClient` and its configured `HttpClient`.
5.  **Registers Authentication Providers**: Registers [`IAuthenticationProvider`](../../OpenRouter/Authentication/IAuthenticationProvider.cs:1) implementations (e.g., [`ApiKeyProvider`](../../OpenRouter/Authentication/ApiKeyProvider.cs:1)).

## Usage Examples

### Basic Setup with Options Action

This is the most common way to configure the client if you are setting options programmatically or from mixed sources.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenRouter.Extensions; // Crucial using directive

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Retrieve API Key from environment or configuration
                var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ??
                             hostContext.Configuration["OpenRouter:ApiKey"];

                services.AddOpenRouter(options =>
                {
                    options.ApiKey = apiKey;
                    options.HttpReferer = hostContext.Configuration["OpenRouter:HttpReferer"] ?? "https://myapp.com/docs";
                    options.XTitle = hostContext.Configuration["OpenRouter:XTitle"] ?? "My Application";
                    options.DefaultHeaders["User-Agent"] = "MyApp/1.0 (via OpenRouter.NET)";
                    
                    // Configure retry policy if needed
                    // options.RetryPolicyOptions.MaxRetries = 5;
                });

                // Register your application services that depend on IOpenRouterClient or its services
                services.AddTransient<MyApplicationService>();
            });
}
```

### Setup by Binding to `IConfiguration`

If all your `OpenRouterOptions` are present in an `IConfiguration` section (e.g., from `appsettings.json`), you can bind directly.

**`appsettings.json`:**
```json
{
  "OpenRouter": {
    "ApiKey": "your_api_key_here",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "HttpReferer": "https://myotherapp.com",
    "XTitle": "My Other App",
    "DefaultHeaders": {
        "User-Agent": "MyOtherApp/2.0"
    }
    // RetryPolicyOptions can also be configured here if the JSON structure matches
  }
}
```

**`Program.cs` / `Startup.cs`:**
```csharp
services.AddOpenRouter(hostContext.Configuration.GetSection("OpenRouter"));
```
The SDK's `AddOpenRouter` overload that takes `IConfiguration` will handle binding the section to `OpenRouterOptions`.

### Using the Registered Services

Once registered, you can inject `IOpenRouterClient` or any of the specific services (e.g., `IChatService`, `IModelsService`) into your application's constructors.

```csharp
using OpenRouter.Core;
using OpenRouter.Services.Chat;
using Microsoft.Extensions.Logging;

public class MyApplicationService
{
    private readonly IOpenRouterClient _openRouterClient;
    private readonly IChatService _chatService; // Can inject specific service too
    private readonly ILogger<MyApplicationService> _logger;

    public MyApplicationService(
        IOpenRouterClient openRouterClient, 
        IChatService chatService, 
        ILogger<MyApplicationService> logger)
    {
        _openRouterClient = openRouterClient;
        _chatService = chatService;
        _logger = logger;
    }

    public async Task DoSomethingWithOpenRouter()
    {
        _logger.LogInformation("Fetching models using IOpenRouterClient...");
        var modelsResponse = await _openRouterClient.Models.GetModelsAsync();
        // ... process models

        _logger.LogInformation("Sending chat message using IChatService...");
        var chatRequest = new OpenRouter.Models.Requests.ChatCompletionRequest { /* ... */ };
        var chatResponse = await _chatService.CreateChatCompletionAsync(chatRequest);
        // ... process chat response
    }
}
```

## Lifetime Management

*   **`IOpenRouterClient` and its services (`IChatService`, etc.) are registered as Scoped.** This is suitable for most applications, especially web applications where a scope maps to a request.
*   The underlying `HttpClient` instances managed by `IHttpClientFactory` are long-lived and reused efficiently.

If you have specific lifetime requirements (e.g., needing a transient `IOpenRouterClient` for some reason, though not typically recommended), you would need to manually register the components or adjust their lifetimes after the initial `AddOpenRouter()` call, but this is an advanced scenario.

## Benefits of DI with OpenRouter.NET

*   **Decoupling**: Your application code depends on abstractions (`IOpenRouterClient`, `IChatService`) rather than concrete implementations.
*   **Configuration Management**: Centralized configuration of options.
*   **Testability**: Easier to mock SDK services for unit testing your application logic.
*   **Lifecycle Management**: DI container manages the lifetime of SDK components.
*   **Leverages `IHttpClientFactory`**: Benefits from optimized `HttpClient` usage, including connection pooling and resilience.

Refer to the [`OpenRouter.Examples/Program.cs`](../../OpenRouter.Examples/Program.cs:231) for a complete, runnable example of DI setup in a .NET Generic Host console application.

## Next Steps
*   [HTTP Client Customization](http-client.md)
*   Review the [Getting Started Guide](../getting-started.md) for basic installation and configuration.