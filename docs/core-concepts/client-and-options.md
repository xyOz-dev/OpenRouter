# Core Concepts: Client and Options

This section delves into two fundamental components of the OpenRouter.NET SDK: the `OpenRouterClient` and its configuration via `OpenRouterOptions`.

## `IOpenRouterClient` / `OpenRouterClient`

The [`OpenRouterClient`](../../OpenRouter/Core/OpenRouterClient.cs:1) is the primary entry point for interacting with the OpenRouter API. It acts as a facade, providing access to various services (like Chat, Models, etc.) that correspond to different API functionalities.

While you can instantiate `OpenRouterClient` directly, the recommended approach is to use Dependency Injection (DI) and work with the [`IOpenRouterClient`](../../OpenRouter/Core/IOpenRouterClient.cs:1) interface. This promotes loosely coupled and testable code.

**Key Responsibilities:**
*   Manages the underlying `HttpClient` used for API communication.
*   Provides access to specific services (e.g., `ChatService`, `ModelsService`).
*   Holds the configured `OpenRouterOptions`.

**Accessing Services:**
Once you have an `IOpenRouterClient` instance (typically via DI), you can access its services:

```csharp
using OpenRouter.Core;
using OpenRouter.Services.Chat;
using OpenRouter.Services.Models;

public class ExampleService
{
    private readonly IOpenRouterClient _client;

    public ExampleService(IOpenRouterClient client)
    {
        _client = client;
    }

    public async Task UseServicesAsync()
    {
        // Access the Chat service
        IChatService chatService = _client.Chat;
        // Use chatService to make chat completion requests...

        // Access the Models service
        IModelsService modelsService = _client.Models;
        // Use modelsService to list models, get details, etc...
    }
}
```
The client also directly exposes some common service methods for convenience. For example, `_client.GetModelsAsync()` is a shortcut to `_client.Models.GetModelsAsync()`.

## `OpenRouterOptions`

The [`OpenRouterOptions`](../../OpenRouter/Core/OpenRouterOptions.cs:1) class is used to configure the behavior of the `OpenRouterClient`. These options are typically set up during DI registration.

**Key Properties:**

*   `ApiKey` (string): **Required**. Your OpenRouter API key.
    *   Example: `"sk-or-v1-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"`
*   `BaseUrl` (string): The base URL for the OpenRouter API.
    *   Default: `"https://openrouter.ai/api/v1"`
    *   Can be overridden if you are using a proxy or a different API endpoint.
*   `HttpReferer` (string): An optional HTTP Referer header. OpenRouter uses this to identify the source of requests, which can be helpful for analytics and tracking.
    *   Example: `"https://your-application-url.com"`
    *   See [`appsettings.json`](../../OpenRouter.Examples/appsettings.json:6) in examples.
*   `XTitle` (string): An optional custom `X-Title` header. This can be your application's name or any identifier.
    *   Example: `"My Awesome AI App"`
    *   See [`appsettings.json`](../../OpenRouter.Examples/appsettings.json:7) in examples.
*   `DefaultHeaders` (Dictionary<string, string>): Allows you to specify additional HTTP headers that will be included in every request made by the client.
    *   The `User-Agent` header is commonly set here.
    *   Example in [`Program.cs`](../../OpenRouter.Examples/Program.cs:239): `options.DefaultHeaders["User-Agent"] = "MyApplication/1.2.3";`
*   `RetryPolicyOptions` (RetryPolicyOptions): Configuration for the built-in retry mechanism (uses Polly).
    *   `MaxRetries` (int): Maximum number of retry attempts for transient failures. Default: 3.
    *   `InitialDelay` (TimeSpan): Initial delay before the first retry. Default: 2 seconds.
    *   `MaxDelay` (TimeSpan): Maximum delay between retries. Default: 30 seconds.
*   `HttpClientName` (string): The name used for the `HttpClient` when registered with `IHttpClientFactory`.
    *   Default: `"OpenRouterHttpClient"`.
*   `JsonSerializerOptions` (JsonSerializerOptions): Custom `System.Text.Json` settings for serializing requests and deserializing responses. Typically, you won't need to modify this unless you have very specific JSON handling requirements.

**Configuration:**

As shown in the [Getting Started Guide](getting-started.md), options are configured when adding OpenRouter services:

```csharp
services.AddOpenRouter(options =>
{
    options.ApiKey = "your_api_key";
    options.HttpReferer = "https://my-app.com";
    options.XTitle = "My App";
    options.DefaultHeaders["User-Agent"] = "MyApp/1.0.0";
});
```

Or, by binding to an `IConfiguration` section:

```csharp
// Assuming "OpenRouter" section in appsettings.json
services.AddOpenRouter(hostContext.Configuration.GetSection("OpenRouter"));
```

Correctly configuring these options is crucial for the SDK to communicate effectively with the OpenRouter API. Always ensure your `ApiKey` is set and kept secure.

## Next Steps
*   [Available Services](services.md)
*   [Request and Response Models](data-models.md)