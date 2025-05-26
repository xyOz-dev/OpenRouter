# Feature: Authentication

Authenticating with the OpenRouter API is a critical step to use its services. The OpenRouter.NET SDK primarily uses API key-based authentication, which is straightforward to configure.

## API Key Authentication

The most common method for authenticating with OpenRouter is by using an API key. This key is sent as an HTTP header (`Authorization: Bearer YOUR_API_KEY`) with each request to the API.

**How the SDK Handles It:**
The SDK automates this process. When you configure [`OpenRouterOptions`](../core-concepts/client-and-options.md) with your API key, the internal [`OpenRouterHttpClient`](../../OpenRouter/Http/OpenRouterHttpClient.cs:1) (via an [`ApiKeyProvider`](../../OpenRouter/Authentication/ApiKeyProvider.cs:1) or similar mechanism) ensures that the `Authorization` header is correctly added to every outgoing request.

### Configuration

As covered in the [Getting Started Guide](../getting-started.md), you provide your API key through `OpenRouterOptions`:

**1. In `ConfigureServices` (e.g., `Program.cs` or `Startup.cs`):**
```csharp
services.AddOpenRouter(options =>
{
    options.ApiKey = "sk-or-v1-your_actual_openrouter_api_key"; 
    // ... other options
});
```

**2. From `appsettings.json`:**
Ensure your `appsettings.json` contains:
```json
{
  "OpenRouter": {
    "ApiKey": "sk-or-v1-your_actual_openrouter_api_key" 
    // ...
  }
}
```
And then bind it during service registration:
```csharp
services.AddOpenRouter(hostContext.Configuration.GetSection("OpenRouter"));
// or manually map properties:
// services.AddOpenRouter(options => 
// {
//     options.ApiKey = hostContext.Configuration["OpenRouter:ApiKey"];
// });
```

**3. From Environment Variables:**
The examples also show reading from an environment variable like `OPENROUTER_API_KEY`:
```csharp
var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? 
             configuration["OpenRouter:ApiKey"]; // Fallback to config
services.AddOpenRouter(options => options.ApiKey = apiKey);
```
Reference: [`OpenRouter.Examples/Program.cs`](../../OpenRouter.Examples/Program.cs:228)

**Security Note:**
*   **Never hardcode API keys directly in source code that will be publicly shared or committed to public repositories.**
*   Use `appsettings.json` (with appropriate environment-specific overrides like `appsettings.Development.json` or user secrets for development) or environment variables for storing sensitive keys.
*   Ensure your production environment securely manages API keys.

## Other Headers for Identification

OpenRouter encourages sending additional headers to help identify your application traffic. While not strictly for "authentication" in the sense of granting access, they are important for API usage tracking and diagnostics.

*   `HTTP-Referer`: Your application's website or a unique identifier for your app.
    *   Configured via `OpenRouterOptions.HttpReferer`. Recommended by OpenRouter.
*   `X-Title`: The name of your application or project.
    *   Configured via `OpenRouterOptions.XTitle`. Recommended by OpenRouter.
*   `User-Agent`: Standard User-Agent string identifying your client.
    *   Can be set via `OpenRouterOptions.DefaultHeaders["User-Agent"]`.

**Example Configuration (`OpenRouterOptions`):**
```csharp
options.HttpReferer = "https://myawesomeservice.com/integration";
options.XTitle = "My Awesome Service AI Integration";
options.DefaultHeaders["User-Agent"] = "MyAwesomeService/1.0.0 (OpenRouter.NET)";
```
Reference: [`OpenRouter.Examples/appsettings.json`](../../OpenRouter.Examples/appsettings.json:5)

## `IAuthenticationProvider` Interface

The SDK includes an [`IAuthenticationProvider`](../../OpenRouter/Authentication/IAuthenticationProvider.cs:1) interface. This suggests a level of abstraction for different authentication strategies.
*   [`ApiKeyProvider`](../../OpenRouter/Authentication/ApiKeyProvider.cs:1): Implements API key authentication. This is the default used by the client.
*   [`BearerTokenProvider`](../../OpenRouter/Authentication/BearerTokenProvider.cs:1): Could be used if OpenRouter supports bearer tokens obtained through a different flow (e.g., OAuth) for certain operations, although API key is standard.
*   [`OAuthProvider`](../../OpenRouter/Authentication/OAuthProvider.cs:1): Indicates potential support or extensibility for OAuth, should OpenRouter offer it.

For typical usage with API keys, you don't interact with these providers directly; the `OpenRouterClient` manages it internally based on the configured `OpenRouterOptions`.

## `IAuthService`

The SDK provides an [`IAuthService`](../../OpenRouter/Services/Auth/IAuthService.cs:1). The purpose of this service might be for:
*   Interacting with specific OpenRouter API endpoints related to authentication, if they exist (e.g., validating tokens, managing authentication sessions in a non-API key scenario).
*   This is distinct from the client *using* an API key to authenticate its general API calls.

As of the current understanding based on the `OpenRouter.Examples` and common API patterns, the primary authentication concern for developers is correctly setting the `ApiKey` in `OpenRouterOptions`.

## Verifying Configuration

The [`OpenRouter.Examples/Program.cs`](../../OpenRouter.Examples/Program.cs:109) includes a `ValidateConfiguration` method that checks if the API key is present. This is a good practice to include in your application's startup to catch configuration errors early.

```csharp
// Simplified from Program.cs
private static bool ValidateConfiguration(IServiceProvider serviceProvider)
{
    var client = serviceProvider.GetRequiredService<IOpenRouterClient>();
    var apiKey = client.Options.ApiKey; 
    
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "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") // Example placeholder
    {
        // Log error, guide user
        return false;
    }
    return true;
}
```

## Next Steps
*   [Error Handling](error-handling.md)
*   Review the [Getting Started Guide](../getting-started.md) for initial setup.
*   Consult official OpenRouter API documentation for the latest on authentication best practices and any new methods.