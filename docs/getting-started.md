# Getting Started with OpenRouter.NET SDK

This guide will walk you through the initial steps to get the OpenRouter.NET SDK up and running in your project.

## Installation

The OpenRouter.NET SDK is available as a NuGet package. You can install it using your preferred method:

**Package Manager Console:**
```powershell
Install-Package OpenRouter.NET
```

**dotnet CLI:**
```bash
dotnet add package OpenRouter.NET
```

**PackageReference in .csproj:**
```xml
<PackageReference Include="OpenRouter.NET" Version="1.0.0" /> 
```
*(Ensure you use the latest version; check NuGet.org for the most up-to-date version number.)*

## Configuration

To use the SDK, you'll primarily need an OpenRouter API key. You can obtain one from your [OpenRouter Dashboard](https://openrouter.ai/keys).

The API key and other settings can be configured in several ways:

### 1. Using `appsettings.json` (Recommended for ASP.NET Core & Generic Host)

If you're using the `Microsoft.Extensions.Hosting` pattern (common in ASP.NET Core applications or console apps using the Generic Host), you can configure the SDK via `appsettings.json`.

**Example `appsettings.json`:**
```json
{
  "OpenRouter": {
    "ApiKey": "your_openrouter_api_key_here", // Replace with your actual key
    "BaseUrl": "https://openrouter.ai/api/v1", // Default, can be overridden
    "HttpReferer": "https://your-site-or-app.com", // Recommended
    "XTitle": "Your App Name" // Recommended
  }
}
```

Then, register the `OpenRouterClient` in your `Startup.cs` or `Program.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenRouter.Extensions; // Add this using directive

public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            // ... other services

            // Add OpenRouter services
            services.AddOpenRouter(options =>
            {
                // API Key can be sourced from IConfiguration
                options.ApiKey = hostContext.Configuration["OpenRouter:ApiKey"];
                
                // Or directly if not using IConfiguration for the key
                // options.ApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "your_fallback_key";

                options.BaseUrl = hostContext.Configuration["OpenRouter:BaseUrl"];
                options.HttpReferer = hostContext.Configuration["OpenRouter:HttpReferer"];
                options.XTitle = hostContext.Configuration["OpenRouter:XTitle"];
                
                // You can also set default headers
                var userAgent = hostContext.Configuration["OpenRouter:UserAgent"] ?? "MyApp/1.0.0";
                options.DefaultHeaders["User-Agent"] = userAgent;
            });

            // Or more simply by binding from configuration
            // services.AddOpenRouter(hostContext.Configuration.GetSection("OpenRouter"));

            // ... other services
        });
```

See the [`OpenRouter.Examples/Program.cs`](../OpenRouter.Examples/Program.cs:231) for a working example of this setup.

### 2. Programmatic Configuration

You can configure the `OpenRouterOptions` directly in code when adding the services or when instantiating the client manually (though DI is preferred).

```csharp
services.AddOpenRouter(options =>
{
    options.ApiKey = "your_openrouter_api_key_here";
    options.HttpReferer = "https://your-site-or-app.com";
    options.XTitle = "Your App Name";
    // options.BaseUrl = "https://custom.openrouter.proxy/api/v1"; // If needed
});
```

### 3. Environment Variables

The API key can also be provided via an environment variable, typically `OPENROUTER_API_KEY`. The example `Program.cs` shows how to read this:

```csharp
// From OpenRouter.Examples/Program.cs
var apiKey = context.Configuration["OpenRouter:ApiKey"] ?? 
           Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

services.AddOpenRouter(options =>
{
    options.ApiKey = apiKey;
    // ... other options
});
```


## Basic Usage (with Dependency Injection)

Once configured, you can inject `IOpenRouterClient` or specific services like `IChatService` into your classes.

```csharp
using OpenRouter.Core;
using OpenRouter.Services.Chat;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Common;

public class MyService
{
    private readonly IOpenRouterClient _openRouterClient;
    private readonly IChatService _chatService;
    private readonly ILogger<MyService> _logger;

    public MyService(IOpenRouterClient openRouterClient, IChatService chatService, ILogger<MyService> logger)
    {
        _openRouterClient = openRouterClient;
        _chatService = chatService;
        _logger = logger;
    }

    public async Task SendSimpleChatMessageAsync()
    {
        _logger.LogInformation("Attempting to send a chat message...");
        try
        {
            var request = new ChatCompletionRequest
            {
                Model = "openai/gpt-3.5-turbo", // Choose a model
                Messages = new List<Message>
                {
                    new Message { Role = "user", Content = "Hello, OpenRouter!" }
                }
            };

            var response = await _chatService.CreateChatCompletionAsync(request);

            if (response.Choices != null && response.Choices.Any())
            {
                _logger.LogInformation("Received response: {Content}", response.Choices.First().Message?.Content);
            }
            else
            {
                _logger.LogWarning("No choices returned in the response.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending chat message");
        }
    }
}
```

## Next Steps

With the SDK installed and configured, you can explore:
*   [Core Concepts](core-concepts/client-and-options.md) to understand the client, options, and services.
*   [Key Functionalities](features/chat-completions.md) like Chat Completions, Model Management, etc.
*   The `OpenRouter.Examples` project for more detailed use cases.

Make sure to check your API key is correctly set and is not the placeholder from `appsettings.json` (e.g., `sk-or-v1-67b...`). See [`OpenRouter.Examples/Program.cs`](../OpenRouter.Examples/Program.cs:109) for configuration validation.