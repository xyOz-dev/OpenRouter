# Advanced Configuration

The OpenRouter .NET library provides extensive configuration options to customize behavior, optimize performance, and integrate with various application architectures. This document covers advanced configuration patterns, performance tuning, and environment-specific settings.

## [`OpenRouterOptions`](OpenRouter/Core/IOpenRouterClient.cs:21) Overview

### [`OpenRouterOptions`](OpenRouter/Core/IOpenRouterClient.cs:21) Properties

The [`OpenRouterOptions`](OpenRouter/Core/IOpenRouterClient.cs:21) class serves as the central configuration point for the OpenRouter client:

- **Base URI Configuration**: Custom endpoint URLs for different environments
- **Authentication Settings**: API key management and authentication schemes
- **Timeout Configuration**: Request and connection timeout values
- **Retry Policy Settings**: Automatic retry behavior configuration
- **Logging Options**: Debug and diagnostic logging levels
- **Performance Settings**: Connection pooling and resource management

### Configuration Binding Patterns

**appsettings.json Integration**
```json
{
  "OpenRouter": {
    "ApiKey": "your-api-key",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "Timeout": "00:02:00",
    "MaxRetries": 3,
    "EnableLogging": true,
    "HttpTimeout": "00:01:30"
  }
}
```

**Options Pattern Implementation**
```csharp
services.Configure<OpenRouterOptions>(
    configuration.GetSection("OpenRouter"));

services.AddSingleton<IOpenRouterClient>(provider =>
{
    var options = provider.GetRequiredService<IOptions<OpenRouterOptions>>().Value;
    return new OpenRouterClient(options);
});
```

<!-- C# Code Example: Fluent configuration builder pattern -->

## HTTP Client Configuration

### Custom HttpClient Setup

**Basic HttpClient Configuration**
```csharp
services.AddHttpClient<IOpenRouterClient, OpenRouterClient>(client =>
{
    client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
    client.Timeout = TimeSpan.FromMinutes(2);
});
```

**Advanced HttpClient Configuration**
```csharp
services.AddHttpClient<IOpenRouterClient, OpenRouterClient>()
    .ConfigureHttpClient((services, client) =>
    {
        var options = services.GetRequiredService<IOptions<OpenRouterOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = options.HttpTimeout;
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        MaxConnectionsPerServer = 10,
        ConnectionTimeout = TimeSpan.FromSeconds(30),
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
    });
```

### Timeout and Retry Policies

**Polly Integration for Resilience**
```csharp
services.AddHttpClient<IOpenRouterClient, OpenRouterClient>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(GetTimeoutPolicy());

private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .Or<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
            });
}
```

<!-- C# Code Example: Custom timeout policies for different operation types -->

### Proxy Configuration

**Corporate Proxy Setup**
```csharp
services.AddHttpClient<IOpenRouterClient, OpenRouterClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseProxy = true,
        Proxy = new WebProxy("http://corporate-proxy:8080")
        {
            Credentials = CredentialCache.DefaultNetworkCredentials
        }
    });
```

**Conditional Proxy Configuration**
```csharp
public class ProxyConfigurationService
{
    public HttpClientHandler CreateHandler(IConfiguration configuration)
    {
        var handler = new HttpClientHandler();
        
        var proxyUrl = configuration["Proxy:Url"];
        if (!string.IsNullOrEmpty(proxyUrl))
        {
            handler.UseProxy = true;
            handler.Proxy = new WebProxy(proxyUrl)
            {
                Credentials = GetProxyCredentials(configuration)
            };
        }
        
        return handler;
    }
}
```

<!-- C# Code Example: Environment-specific proxy configuration -->

## Authentication Configuration

### Multiple Authentication Schemes

**API Key Provider Pattern**
```csharp
public interface IApiKeyProvider
{
    Task<string> GetApiKeyAsync();
    Task RefreshApiKeyAsync();
}

public class EnvironmentApiKeyProvider : IApiKeyProvider
{
    public Task<string> GetApiKeyAsync()
    {
        return Task.FromResult(Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"));
    }
    
    public Task RefreshApiKeyAsync()
    {
        // Implement key refresh logic
        return Task.CompletedTask;
    }
}

public class DatabaseApiKeyProvider : IApiKeyProvider
{
    private readonly IUserContext userContext;
    private readonly IApiKeyRepository repository;
    
    public async Task<string> GetApiKeyAsync()
    {
        var userId = userContext.GetCurrentUserId();
        return await repository.GetApiKeyAsync(userId);
    }
}
```

### Token Refresh Configurations

**Automatic Token Refresh**
```csharp
public class RefreshableApiKeyProvider : IApiKeyProvider
{
    private string cachedKey;
    private DateTime keyExpiry;
    private readonly SemaphoreSlim refreshSemaphore = new(1, 1);
    
    public async Task<string> GetApiKeyAsync()
    {
        if (string.IsNullOrEmpty(cachedKey) || DateTime.UtcNow >= keyExpiry)
        {
            await refreshSemaphore.WaitAsync();
            try
            {
                if (string.IsNullOrEmpty(cachedKey) || DateTime.UtcNow >= keyExpiry)
                {
                    await RefreshApiKeyAsync();
                }
            }
            finally
            {
                refreshSemaphore.Release();
            }
        }
        
        return cachedKey;
    }
}
```

<!-- C# Code Example: OAuth2 integration for enterprise authentication -->

## Performance Tuning

### Connection Pooling Settings

**HTTP Connection Pool Optimization**
```csharp
services.Configure<SocketsHttpHandlerOptions>(options =>
{
    options.PooledConnectionLifetime = TimeSpan.FromMinutes(2);
    options.MaxConnectionsPerServer = 20;
    options.KeepAlivePingTimeout = TimeSpan.FromSeconds(30);
    options.KeepAlivePingDelay = TimeSpan.FromSeconds(15);
});

services.AddHttpClient<IOpenRouterClient, OpenRouterClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        MaxConnectionsPerServer = 20,
        EnableMultipleHttp2Connections = true
    });
```

### Request Timeout Optimization

**Operation-Specific Timeouts**
```csharp
public class TimeoutConfiguration
{
    public TimeSpan ChatCompletion { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan StreamingChat { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan ModelList { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan FileUploads { get; set; } = TimeSpan.FromMinutes(10);
}

public class ConfigurableOpenRouterClient : IOpenRouterClient
{
    private readonly TimeoutConfiguration timeouts;
    
    public async Task<ChatCompletionResponse> GetChatCompletionAsync(
        ChatCompletionRequest request, 
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(timeouts.ChatCompletion);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);
            
        return await base.GetChatCompletionAsync(request, combinedCts.Token);
    }
}
```

### Memory Management Options

**Streaming Response Buffering**
```csharp
public class StreamingConfiguration
{
    public int BufferSize { get; set; } = 8192;
    public int MaxBufferedChunks { get; set; } = 100;
    public bool EnableBackpressure { get; set; } = true;
    public TimeSpan ChunkTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
```

<!-- C# Code Example: Memory-efficient streaming with configurable buffer sizes -->

## Environment-Specific Config

### Development vs Production Settings

**Environment-Based Configuration**
```csharp
public class EnvironmentSpecificOptions
{
    public string Environment { get; set; }
    public bool EnableDetailedLogging { get; set; }
    public bool EnableMetrics { get; set; }
    public string LogLevel { get; set; }
    public Dictionary<string, object> EnvironmentSettings { get; set; }
}

public void ConfigureOpenRouter(IServiceCollection services, IConfiguration configuration)
{
    var environment = configuration["Environment"] ?? "Production";
    
    if (environment == "Development")
    {
        services.Configure<OpenRouterOptions>(options =>
        {
            options.EnableLogging = true;
            options.LogLevel = LogLevel.Debug;
            options.BaseUrl = "https://openrouter.ai/api/v1";
        });
    }
    else
    {
        services.Configure<OpenRouterOptions>(options =>
        {
            options.EnableLogging = false;
            options.LogLevel = LogLevel.Warning;
            options.BaseUrl = configuration["OpenRouter:ProductionUrl"];
        });
    }
}
```

### Configuration Validation

**Startup Configuration Validation**
```csharp
public class OpenRouterOptionsValidator : IValidateOptions<OpenRouterOptions>
{
    public ValidateOptionsResult Validate(string name, OpenRouterOptions options)
    {
        var failures = new List<string>();
        
        if (string.IsNullOrEmpty(options.ApiKey))
            failures.Add("API Key is required");
            
        if (string.IsNullOrEmpty(options.BaseUrl))
            failures.Add("Base URL is required");
            
        if (options.Timeout <= TimeSpan.Zero)
            failures.Add("Timeout must be positive");
            
        if (failures.Count > 0)
            return ValidateOptionsResult.Fail(failures);
            
        return ValidateOptionsResult.Success;
    }
}

services.AddSingleton<IValidateOptions<OpenRouterOptions>, OpenRouterOptionsValidator>();
```

<!-- C# Code Example: Configuration hot-reloading for dynamic settings -->

## Monitoring and Telemetry

### Logging Configuration

**Structured Logging Setup**
```csharp
services.AddLogging(builder =>
{
    builder.AddSerilog(new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("logs/openrouter-.txt", rollingInterval: RollingInterval.Day)
        .Filter.ByIncludingOnly(Matching.FromSource("OpenRouter"))
        .CreateLogger());
});

public class OpenRouterLoggingOptions
{
    public bool LogRequests { get; set; } = false;
    public bool LogResponses { get; set; } = false;
    public bool LogMetrics { get; set; } = true;
    public string[] SensitiveFields { get; set; } = { "api_key", "authorization" };
}
```

### Performance Counter Setup

**Metrics Collection Configuration**
```csharp
public class OpenRouterMetrics
{
    private readonly Counter<long> requestCounter;
    private readonly Histogram<double> requestDuration;
    private readonly Counter<long> errorCounter;
    
    public OpenRouterMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("OpenRouter");
        requestCounter = meter.CreateCounter<long>("openrouter.requests", "request");
        requestDuration = meter.CreateHistogram<double>("openrouter.request_duration", "ms");
        errorCounter = meter.CreateCounter<long>("openrouter.errors", "error");
    }
}
```

<!-- C# Code Example: Integration with Application Insights telemetry -->
<!-- C# Code Example: Custom health checks for OpenRouter service availability -->

## Code Examples

### Advanced Configuration Scenarios

<!-- C# Code Example: Multi-tenant configuration with tenant-specific settings -->
<!-- C# Code Example: Configuration encryption and secure key management -->
<!-- C# Code Example: Dynamic configuration updates without application restart -->
<!-- C# Code Example: Load balancing configuration across multiple OpenRouter endpoints -->
<!-- C# Code Example: Integration testing with configuration mocking -->

The advanced configuration system enables fine-tuned control over all aspects of the OpenRouter client behavior, from basic connectivity settings to complex enterprise integration scenarios.