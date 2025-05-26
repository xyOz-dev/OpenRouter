# Advanced Topic: HTTP Client Customization

The OpenRouter.NET SDK uses `IHttpClientFactory` to create and manage `HttpClient` instances for communicating with the OpenRouter API. While the default configuration is suitable for most use cases, there might be scenarios where you need to customize the `HttpClient`.

## Default HTTP Client Behavior

When you call `services.AddOpenRouter(...)`, the SDK sets up a named `HttpClient` (default name: "OpenRouterHttpClient", configurable via `OpenRouterOptions.HttpClientName`). This client is pre-configured with:

1.  **Base Address**: Set to `OpenRouterOptions.BaseUrl`.
2.  **Default Request Headers**:
    *   `Authorization`: Bearer token for your API key.
    *   `User-Agent`: (Configurable via `OpenRouterOptions.DefaultHeaders`).
    *   `HTTP-Referer`: (Configurable via `OpenRouterOptions.HttpReferer`).
    *   `X-Title`: (Configurable via `OpenRouterOptions.XTitle`).
    *   Any other headers specified in `OpenRouterOptions.DefaultHeaders`.
3.  **Polly Retry Policy**: A retry policy is added to handle transient network errors and certain HTTP status codes (e.g., 5xx). This includes exponential backoff.
    *   Configurable via `OpenRouterOptions.RetryPolicyOptions`.

## Customizing via `IHttpClientBuilder`

The `AddOpenRouter()` extension method returns an `IHttpClientBuilder`. You can chain standard `IHttpClientFactory` extension methods onto this builder to further customize the `HttpClient`. The OpenRouter SDK applies its own configurations to this builder, so your customizations will typically augment or modify its setup.

**Example: Adding a Custom Delegating Handler**

Let's say you want to add a custom `DelegatingHandler` to log all outgoing requests and incoming responses for the OpenRouter client.

First, define your custom handler:
```csharp
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> _logger;

    public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending request to OpenRouter: {Method} {Uri}", request.Method, request.RequestUri);
        // Add more detailed logging for headers or content if necessary,
        // but be extremely careful with sensitive data like API keys in request content for other APIs.
        // The Authorization header for OpenRouter is handled by the SDK.

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation("Received response from OpenRouter: {StatusCode}", response.StatusCode);
        // Log response headers or content if needed for debugging.
        // For example, to see rate limit headers:
        // if (response.Headers.TryGetValues("x-ratelimit-remaining", out var remaining))
        // {
        //    _logger.LogInformation("Rate limit remaining: {Remaining}", string.Join(",", remaining));
        // }

        return response;
    }
}
```

**In `ConfigureServices` (e.g., `Program.cs` or `Startup.cs`):**
```csharp
// At the top of your file or within your namespace
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // For ILogger
// ... other usings for OpenRouter types

// In your ConfigureServices method:
services.AddTransient<LoggingDelegatingHandler>(); // Register your handler with DI

services.AddOpenRouter(options =>
{
    // Configure OpenRouterOptions as usual
    options.ApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "your_fallback_api_key";
    options.HttpReferer = "https://yourapp.com/example";
    options.XTitle = "MyApplication";
    options.DefaultHeaders["User-Agent"] = "MyApplication/1.0 OpenRouter.NET";
})
.ConfigureHttpClient((serviceProvider, client) => // Access IServiceProvider for more complex setups
{
    // Direct HttpClient configuration (e.g., Timeout)
    // This client is the one configured for OpenRouter by AddOpenRouter
    client.Timeout = TimeSpan.FromSeconds(100); // Default is often 100s, adjust as needed
})
.AddHttpMessageHandler<LoggingDelegatingHandler>() // Add your custom handler to the pipeline
// You can chain other IHttpClientBuilder methods:
// .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Example: Configure handler lifetime
.ConfigurePrimaryHttpMessageHandler(serviceProvider => // Configure the innermost handler
{
    // Example: Customizing HttpClientHandler settings
    return new HttpClientHandler
    {
        AllowAutoRedirect = false,
        UseProxy = true,
        Proxy = new System.Net.WebProxy("http://localhost:8888") // Example proxy
        // AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
});
```

**Key `IHttpClientBuilder` Methods for Customization:**

*   **`ConfigureHttpClient(Action<IServiceProvider, HttpClient> configureClient)` or `ConfigureHttpClient(Action<HttpClient> configureClient)`**: Allows direct configuration of the `HttpClient` instance (e.g., setting `Timeout`).
*   **`AddHttpMessageHandler<THandler>()` where `THandler` is `DelegatingHandler`**: Adds a `DelegatingHandler` to the request pipeline. Handlers are executed in the order they are added. Ensure `THandler` is registered in DI if it has dependencies.
*   **`ConfigurePrimaryHttpMessageHandler<THandler>()` or `ConfigurePrimaryHttpMessageHandler(Func<IServiceProvider, THandler> configureHandler)`**: Allows configuring or replacing the primary `HttpMessageHandler` (e.g., `HttpClientHandler` or `SocketsHttpHandler`). This is where you'd set proxy settings, SSL validation callbacks, etc.
*   **`SetHandlerLifetime(TimeSpan handlerLifetime)`**: Configures how long a handler pipeline (and its underlying `HttpClientHandler`) can be reused. Defaults to 2 minutes.
*   **RedactHeader()**: Adds a redaction policy for specific headers if you're using ASP.NET Core's built-in HTTP client logging (which is separate from custom handlers like `LoggingDelegatingHandler`).

Refer to [Microsoft's documentation on IHttpClientFactory and HttpClient customization](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests) for a comprehensive list of customization options.

## Important Considerations:

*   **Order of Configuration**: Customizations chained on the `IHttpClientBuilder` returned by `AddOpenRouter()` will apply to the HttpClient instance configured by the SDK.
*   **Polly Policy**: The SDK adds its own Polly retry policy. If you add another Polly policy using methods available on `IHttpClientBuilder` (like `AddTransientHttpErrorPolicy`), be mindful of how they interact. It's often better to adjust the SDK's policy via `OpenRouterOptions.RetryPolicyOptions` if possible, or ensure your custom policy complements it.
*   **Authentication Headers**: The SDK is responsible for adding the `Authorization` header based on `OpenRouterOptions.ApiKey`. Avoid explicitly setting or modifying this header in your customizations unless you intend to entirely replace the SDK's authentication mechanism, which is an advanced and generally unnecessary step.
*   **`OpenRouterOptions.JsonSerializerOptions`**: For customizing JSON serialization behavior for OpenRouter requests/responses, use `OpenRouterOptions.JsonSerializerOptions`. The SDK's services use these options internally when creating `JsonContent` or deserializing responses. Modifying `HttpClient` defaults for JSON won't affect this.

Customizing the `HttpClient` provides powerful flexibility but should be done with a clear understanding of `IHttpClientFactory`, `DelegatingHandler` pipelines, and how the OpenRouter.NET SDK configures its client. For most common needs, the settings within `OpenRouterOptions` should suffice.

## Next Steps
*   Review [Dependency Injection Setup](dependency-injection.md).
*   Explore the [`OpenRouter.Examples`](../../OpenRouter.Examples/) project to see standard client setup.
*   Consult the official [OpenRouter API Documentation](https://openrouter.ai/docs) for any API-specific considerations regarding HTTP behavior or rate limiting.