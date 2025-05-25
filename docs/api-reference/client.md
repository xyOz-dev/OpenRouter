# Client and Core Interfaces

This document provides comprehensive reference for the core client interfaces and implementation classes in the OpenRouter .NET library.

## [`IOpenRouterClient`](../../OpenRouter/Core/IOpenRouterClient.cs:9) Interface

The [`IOpenRouterClient`](../../OpenRouter/Core/IOpenRouterClient.cs:9) interface is the main entry point for interacting with the OpenRouter API. It provides access to all service endpoints and core functionality.

### Service Property Accessors

```csharp
IChatService Chat { get; }
IModelsService Models { get; }
ICreditsService Credits { get; }
IKeysService Keys { get; }
IAuthService Auth { get; }
```

Each property provides access to a specialized service for different API endpoints:

- **[`Chat`](../../OpenRouter/Core/IOpenRouterClient.cs:11)** - Chat completion and streaming operations
- **[`Models`](../../OpenRouter/Core/IOpenRouterClient.cs:12)** - Model discovery and information retrieval
- **[`Credits`](../../OpenRouter/Services/Credits/ICreditsService.cs:1)** - Credit balance and usage tracking
- **[`Keys`](../../OpenRouter/Services/Keys/IKeysService.cs:1)** - API key management operations
- **[`Auth`](../../OpenRouter/Services/Auth/IAuthService.cs:1)** - OAuth authentication flows

### Direct API Methods

```csharp
Task<T> SendAsync<T>(string endpoint, object? request = null, CancellationToken cancellationToken = default);
IAsyncEnumerable<T> StreamAsync<T>(string endpoint, object? request = null, CancellationToken cancellationToken = default);
```

These methods provide direct access to OpenRouter API endpoints for advanced scenarios:

- **[`SendAsync<T>`](../../OpenRouter/Core/IOpenRouterClient.cs:17)** - Send a request to any endpoint and deserialize the response
- **[`StreamAsync<T>`](../../OpenRouter/Core/IOpenRouterClient.cs:19)** - Stream responses from any endpoint that supports streaming

### Configuration Access

```csharp
OpenRouterOptions Options { get; }
```

The **[`Options`](../../OpenRouter/Core/IOpenRouterClient.cs:21)** property provides read-only access to the client's configuration settings.

### Interface Implementation

[`IOpenRouterClient`](../../OpenRouter/Core/IOpenRouterClient.cs:9) extends `IDisposable`, ensuring proper resource cleanup when the client is no longer needed.

## [`OpenRouterClient`](../../OpenRouter/Core/OpenRouterClient.cs:1) Implementation

The [`OpenRouterClient`](../../OpenRouter/Core/OpenRouterClient.cs:1) class is the default implementation of [`IOpenRouterClient`](../../OpenRouter/Core/IOpenRouterClient.cs:9).

### Constructor Parameters and Initialization

#### Primary Constructor

```csharp
public OpenRouterClient(
    IHttpClientProvider httpClient,
    OpenRouterOptions options,
    ILogger<OpenRouterClient>? logger = null)
```

- **httpClient** - HTTP client provider for API communication
- **options** - Configuration options for the client
- **logger** - Optional logger for diagnostics and debugging

#### Convenience Constructor

```csharp
public OpenRouterClient(
    string apiKey, 
    Action<OpenRouterOptions>? configure = null, 
    ILogger<OpenRouterClient>? logger = null)
```

- **apiKey** - Your OpenRouter API key
- **configure** - Optional configuration action
- **logger** - Optional logger for diagnostics

<!-- C# Code Example: Basic client initialization -->
```csharp
// Simple initialization
var client = new OpenRouterClient("your-api-key");

// With configuration
var client = new OpenRouterClient("your-api-key", options => 
{
    options.Timeout = TimeSpan.FromSeconds(60);
    options.MaxRetryAttempts = 3;
});
```

### Disposal Pattern Implementation

The client implements proper disposal to release underlying HTTP resources:

```csharp
public void Dispose()
```

- Disposes the underlying HTTP client provider
- Logs disposal completion (if logger is configured)
- Prevents multiple disposal calls
- Thread-safe disposal implementation

## Client Configuration

### [`OpenRouterOptions`](../../OpenRouter/Core/OpenRouterOptions.cs:5) Properties

The [`OpenRouterOptions`](../../OpenRouter/Core/OpenRouterOptions.cs:5) class provides comprehensive configuration for the OpenRouter client.

#### Authentication Settings

```csharp
string ApiKey { get; set; } = string.Empty;
bool ValidateApiKey { get; set; } = true;
```

#### Connection Settings

```csharp
string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(100);
```

#### Retry Configuration

```csharp
bool EnableRetry { get; set; } = true;
int MaxRetryAttempts { get; set; } = 3;
TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(1000);
```

#### Logging Configuration

```csharp
bool EnableLogging { get; set; } = true;
LogLevel LogLevel { get; set; } = LogLevel.Information;
```

#### HTTP Headers

```csharp
Dictionary<string, string> DefaultHeaders { get; set; } = new();
string? HttpReferer { get; set; }
string? XTitle { get; set; }
```

#### Streaming Settings

```csharp
bool EnableStreamingOptimizations { get; set; } = true;
int StreamingBufferSize { get; set; } = 8192;
```

#### Error Handling

```csharp
bool ThrowOnApiErrors { get; set; } = true;
```

### Configuration Validation

The [`Validate()`](../../OpenRouter/Core/OpenRouterOptions.cs:24) method ensures all configuration values are valid:

- API key is required and not empty
- Base URL is a valid absolute URI
- Timeout values are positive
- Retry settings are valid
- Buffer sizes are positive

<!-- C# Code Example: Configuration validation -->
```csharp
var options = new OpenRouterOptions
{
    ApiKey = "your-api-key",
    MaxRetryAttempts = 5,
    Timeout = TimeSpan.FromSeconds(45)
};

options.Validate(); // Throws if invalid
```

## Service Access Patterns

### Individual Service Usage

Services can be accessed through the client's properties and used independently:

<!-- C# Code Example: Individual service usage -->
```csharp
// Access specific services
var chatResponse = await client.Chat
    .CreateRequest()
    .WithModel("gpt-3.5-turbo")
    .WithUserMessage("Hello!")
    .ExecuteAsync();

var models = await client.Models.ListModelsAsync();
var credits = await client.Credits.GetCreditsAsync();
```

### Service Lifecycle Management

All services are lazily initialized and share the same underlying HTTP client:

- Services are created on first access
- Shared configuration and authentication
- Automatic resource management through client disposal
- Thread-safe service initialization

## Thread Safety

The [`OpenRouterClient`](../../OpenRouter/Core/OpenRouterClient.cs:1) and its services are designed for concurrent usage:

### Concurrent Usage Guidelines

- **Client instance** - Thread-safe for concurrent operations
- **Service instances** - Thread-safe and can be used across multiple threads
- **Request builders** - Not thread-safe; create new instances per request
- **Options** - Read-only after client initialization

<!-- C# Code Example: Concurrent usage -->
```csharp
// Safe concurrent usage
var tasks = Enumerable.Range(0, 10).Select(i => 
    client.Chat
        .CreateRequest()
        .WithModel("gpt-3.5-turbo")
        .WithUserMessage($"Message {i}")
        .ExecuteAsync());

var responses = await Task.WhenAll(tasks);
```

### Best Practices

1. **Single client instance** - Reuse the same client instance across your application
2. **Proper disposal** - Always dispose the client when done (use `using` statements)
3. **Configuration immutability** - Don't modify options after client creation
4. **Request builder instances** - Create new request builders for each operation