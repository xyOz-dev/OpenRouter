# Dependency Injection Integration

The OpenRouter .NET library provides first-class support for Microsoft.Extensions.DependencyInjection, making it seamless to integrate into ASP.NET Core applications and other .NET applications using dependency injection.

## DI Container Registration

The library offers multiple extension methods in [`ServiceCollectionExtensions`](OpenRouter/Extensions/ServiceCollectionExtensions.cs:16) to register OpenRouter services with different configuration approaches.

### Basic Registration with API Key

Use [`AddOpenRouter(string apiKey)`](OpenRouter/Extensions/ServiceCollectionExtensions.cs:18) for simple scenarios:

```csharp
using OpenRouter.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Simple registration with API key
builder.Services.AddOpenRouter("your_api_key_here");

var app = builder.Build();
```

This method automatically configures:
- [`IOpenRouterClient`](OpenRouter/Core/IOpenRouterClient.cs:9) as Scoped
- [`IChatService`](OpenRouter/Core/IOpenRouterClient.cs:11) as Scoped
- [`IModelsService`](OpenRouter/Core/IOpenRouterClient.cs:12) as Scoped  
- [`ICreditsService`](OpenRouter/Core/IOpenRouterClient.cs:13) as Scoped
- [`IKeysService`](OpenRouter/Core/IOpenRouterClient.cs:14) as Scoped
- [`IAuthService`](OpenRouter/Core/IOpenRouterClient.cs:15) as Scoped
- [`IAuthenticationProvider`](OpenRouter/Authentication/IAuthenticationProvider.cs:1) as Singleton
- [`IHttpClientProvider`](OpenRouter/Http/IHttpClientProvider.cs:1) as Singleton

### Configuration-Based Registration

Use [`AddOpenRouter(Action<OpenRouterOptions>)`](OpenRouter/Extensions/ServiceCollectionExtensions.cs:26) for advanced configuration:

```csharp
builder.Services.AddOpenRouter(options =>
{
    options.ApiKey = builder.Configuration["OpenRouter:ApiKey"]!;
    options.BaseUrl = "https://openrouter.ai/api/v1";
    options.Timeout = TimeSpan.FromSeconds(30);
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
    options.RetryDelay = TimeSpan.FromSeconds(1);
    options.EnableLogging = true;
    options.LogLevel = LogLevel.Information;
    options.ValidateApiKey = true;
    options.ThrowOnApiErrors = true;
});
```

### HttpClient Integration

For applications that need custom HttpClient configuration, use [`AddOpenRouterWithHttpClient()`](OpenRouter/Extensions/ServiceCollectionExtensions.cs:65):

```csharp
builder.Services.AddOpenRouterWithHttpClient(
    configure: options =>
    {
        options.ApiKey = builder.Configuration["OpenRouter:ApiKey"]!;
        options.BaseUrl = "https://openrouter.ai/api/v1";
    },
    configureHttpClient: client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);
        client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
        client.DefaultRequestHeaders.Add("X-Custom-Header", "CustomValue");
    });
```

This method uses `IServiceCollection.AddHttpClient<T>()` for proper HttpClient lifecycle management and supports:
- Custom timeout settings
- Default headers configuration
- HttpClientFactory integration
- Proper HttpClient disposal

## Configuration Patterns

### OpenRouterOptions Configuration Binding

Bind configuration from `appsettings.json` to [`OpenRouterOptions`](OpenRouter/Core/OpenRouterOptions.cs:5):

```json
{
  "OpenRouter": {
    "ApiKey": "your_api_key_here",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "Timeout": "00:00:30",
    "RequestTimeout": "00:02:00",
    "MaxRetryAttempts": 3,
    "RetryDelay": "00:00:01",
    "EnableRetry": true,
    "EnableLogging": true,
    "LogLevel": "Information",
    "ValidateApiKey": true,
    "ThrowOnApiErrors": true,
    "EnableStreamingOptimizations": true,
    "StreamingBufferSize": 8192,
    "DefaultHeaders": {
      "X-Custom-Header": "CustomValue"
    },
    "HttpReferer": "https://myapp.com",
    "XTitle": "My Application"
  }
}
```

```csharp
// Bind entire configuration section
builder.Services.AddOpenRouter(options =>
    builder.Configuration.GetSection("OpenRouter").Bind(options));

// Or configure specific properties
builder.Services.AddOpenRouter(options =>
{
    builder.Configuration.GetSection("OpenRouter").Bind(options);
    
    // Override or add additional configuration
    options.EnableLogging = builder.Environment.IsDevelopment();
    options.LogLevel = builder.Environment.IsDevelopment() 
        ? LogLevel.Debug 
        : LogLevel.Warning;
});
```

### Environment-Specific Configurations

Configure different settings per environment:

```csharp
builder.Services.AddOpenRouter(options =>
{
    // Base configuration
    builder.Configuration.GetSection("OpenRouter").Bind(options);
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableLogging = true;
        options.LogLevel = LogLevel.Debug;
        options.EnableRetry = false; // Disable retry for faster debugging
        options.ValidateApiKey = true;
    }
    else if (builder.Environment.IsProduction())
    {
        options.EnableLogging = true;
        options.LogLevel = LogLevel.Warning;
        options.EnableRetry = true;
        options.MaxRetryAttempts = 5;
        options.ValidateApiKey = false; // Skip validation for performance
    }
});
```

### Secrets and Environment Variables

Use ASP.NET Core's configuration system for secure API key management:

```csharp
// Use environment variables
builder.Services.AddOpenRouter(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
        ?? throw new InvalidOperationException("OPENROUTER_API_KEY not set");
    
    // Other configuration from appsettings
    builder.Configuration.GetSection("OpenRouter").Bind(options);
});

// Use Secret Manager in development
// Run: dotnet user-secrets set "OpenRouter:ApiKey" "your_api_key"
builder.Services.AddOpenRouter(options =>
{
    options.ApiKey = builder.Configuration["OpenRouter:ApiKey"]
        ?? throw new InvalidOperationException("OpenRouter:ApiKey not configured");
});
```

## Service Lifetime Management

Understanding service lifetimes is crucial for proper dependency injection usage:

### Singleton vs Scoped Registration

The default registration uses these lifetimes:

```csharp
// Singleton (shared across application)
services.TryAddSingleton<OpenRouterOptions>();
services.TryAddSingleton<IAuthenticationProvider>();
services.TryAddSingleton<IHttpClientProvider>();

// Scoped (per request in web apps, per scope otherwise)
services.TryAddScoped<IChatService>();
services.TryAddScoped<IModelsService>();
services.TryAddScoped<ICreditsService>();
services.TryAddScoped<IKeysService>();
services.TryAddScoped<IAuthService>();
services.TryAddScoped<IOpenRouterClient>();
```

### HttpClient Lifecycle Management

When using [`AddOpenRouterWithHttpClient()`](OpenRouter/Extensions/ServiceCollectionExtensions.cs:65), HttpClient lifecycle is managed by `IHttpClientFactory`:

```csharp
// Proper HttpClient disposal and reuse
builder.Services.AddOpenRouterWithHttpClient(
    configure: options => /* configuration */,
    configureHttpClient: client => /* HttpClient setup */
);

// HttpClient will be:
// - Reused for performance
// - Properly disposed
// - DNS changes handled automatically
// - Connection pooling enabled
```

### Custom Lifetime Registration

For specific scenarios, you can register with different lifetimes:

```csharp
// Register as Singleton for console applications
services.AddSingleton<IOpenRouterClient>(provider =>
{
    var options = new OpenRouterOptions { ApiKey = "your_api_key" };
    return new OpenRouterClient("your_api_key", opt => opt = options);
});

// Register as Transient if you need new instances each time
services.AddTransient<IChatService>(provider =>
{
    var client = provider.GetRequiredService<IOpenRouterClient>();
    return client.Chat;
});
```

## Injection in Controllers and Services

### Controller Injection

Inject [`IOpenRouterClient`](OpenRouter/Core/IOpenRouterClient.cs:9) into controllers:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IOpenRouterClient _openRouterClient;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IOpenRouterClient openRouterClient,
        ILogger<ChatController> logger)
    {
        _openRouterClient = openRouterClient;
        _logger = logger;
    }

    [HttpPost("complete")]
    public async Task<ActionResult<ChatCompletionResponse>> CreateChatCompletion(
        [FromBody] ChatCompletionRequest request)
    {
        try
        {
            var response = await _openRouterClient.Chat
                .CreateChatCompletion(request.Model)
                .AddUserMessage(request.Message)
                .SendAsync();

            return Ok(response);
        }
        catch (OpenRouterException ex)
        {
            _logger.LogError(ex, "OpenRouter API error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during chat completion");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
```

### Individual Service Injection

Inject specific services when you don't need the full client:

```csharp
public class ModelService
{
    private readonly IModelsService _modelsService;
    private readonly ICreditsService _creditsService;

    public ModelService(
        IModelsService modelsService,
        ICreditsService creditsService)
    {
        _modelsService = modelsService;
        _creditsService = creditsService;
    }

    public async Task<AvailableModel[]> GetAffordableModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var credits = await _creditsService.GetCreditsAsync(cancellationToken);
        var models = await _modelsService.GetModelsAsync(cancellationToken);

        return models
            .Where(m => m.Pricing.Prompt <= credits.Balance)
            .ToArray();
    }
}
```

### Background Service Integration

Use OpenRouter in background services:

```csharp
public class ChatProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatProcessingService> _logger;

    public ChatProcessingService(
        IServiceProvider serviceProvider,
        ILogger<ChatProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var openRouterClient = scope.ServiceProvider.GetRequiredService<IOpenRouterClient>();

            try
            {
                // Process chat requests from queue
                await ProcessChatRequestsAsync(openRouterClient, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat requests");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessChatRequestsAsync(
        IOpenRouterClient client,
        CancellationToken cancellationToken)
    {
        // Implementation details...
    }
}

// Register background service
builder.Services.AddHostedService<ChatProcessingService>();
```

## Testing with Dependency Injection

### Mock Service Registration

Register mocks for unit testing:

```csharp
public class ChatControllerTests
{
    [Fact]
    public async Task CreateChatCompletion_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var services = new ServiceCollection();
        
        var mockClient = new Mock<IOpenRouterClient>();
        var mockChatService = new Mock<IChatService>();
        
        mockClient.Setup(c => c.Chat).Returns(mockChatService.Object);
        mockChatService
            .Setup(c => c.CreateChatCompletion(It.IsAny<string>()))
            .Returns(new ChatRequestBuilder(mockClient.Object, "gpt-3.5-turbo"));

        services.AddSingleton(mockClient.Object);
        services.AddSingleton<ChatController>();

        var serviceProvider = services.BuildServiceProvider();
        var controller = serviceProvider.GetRequiredService<ChatController>();

        // Act & Assert
        var result = await controller.CreateChatCompletion(new ChatCompletionRequest 
        { 
            Model = "gpt-3.5-turbo", 
            Message = "Test" 
        });

        // Verify results...
    }
}
```

### Integration Testing

Test with real OpenRouter services in integration tests:

```csharp
public class OpenRouterIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OpenRouterIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ChatCompletion_Integration_Success()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override with test API key
                services.Configure<OpenRouterOptions>(options =>
                {
                    options.ApiKey = Environment.GetEnvironmentVariable("TEST_OPENROUTER_API_KEY")!;
                });
            });
        }).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/chat/complete", new
        {
            model = "gpt-3.5-turbo",
            message = "Hello, World!"
        });

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hello", content);
    }
}
```

### Test Service Provider

Create a test service provider for unit tests:

```csharp
public static class TestServiceProvider
{
    public static ServiceProvider CreateWithMockOpenRouter()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add mock OpenRouter services
        var mockClient = new Mock<IOpenRouterClient>();
        services.AddSingleton(mockClient.Object);
        
        return services.BuildServiceProvider();
    }

    public static ServiceProvider CreateWithRealOpenRouter(string apiKey)
    {
        var services = new ServiceCollection();
        
        services.AddLogging(builder => builder.AddConsole());
        services.AddOpenRouter(apiKey);
        
        return services.BuildServiceProvider();
    }
}

// Usage in tests
var provider = TestServiceProvider.CreateWithMockOpenRouter();
var service = provider.GetRequiredService<IOpenRouterClient>();
```

<!-- C# Code Example: Advanced DI configuration with custom authentication and caching -->

---

**Next**: [API Reference →](api-reference/index.md)