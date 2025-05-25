# Quick Start Examples

This guide provides complete, runnable examples to get you started with the OpenRouter .NET library quickly. Each example includes full code implementations and explanations of key concepts.

## Basic Client Setup

### Minimal Working Example

The simplest way to create an [`OpenRouterClient`](../OpenRouter/Core/OpenRouterClient.cs:1) and start making requests:

```csharp
using OpenRouter.Core;

// Create client with API key
var client = new OpenRouterClient("your-api-key-here");

// Make a simple chat completion request
var response = await client.Chat
    .CreateChatCompletion("gpt-3.5-turbo")
    .AddUserMessage("Hello, world!")
    .SendAsync();

Console.WriteLine(response.FirstChoiceContent);
```

#### Key Concepts
- [`OpenRouterClient`](../OpenRouter/Core/OpenRouterClient.cs:1) is the main entry point for all operations
- The fluent API allows chaining method calls for clean, readable code
- [`SendAsync()`](../OpenRouter/Services/Chat/IChatRequestBuilder.cs:42) executes the request asynchronously

#### Common Variations

```csharp
// With timeout configuration
var client = new OpenRouterClient("your-api-key", options =>
{
    options.RequestTimeout = TimeSpan.FromSeconds(60);
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
});

// Using environment variable
var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
    ?? throw new InvalidOperationException("API key not found");
var client = new OpenRouterClient(apiKey);
```

## Simple Chat Completion

### Request and Response Handling

Complete example showing how to handle [`ChatCompletionResponse`](../OpenRouter/Models/Responses/ChatCompletionResponse.cs:6) objects:

<!-- C# Code Example: Complete chat completion with proper response handling and error checking -->

```csharp
using OpenRouter.Core;
using OpenRouter.Models.Responses;

var client = new OpenRouterClient("your-api-key");

try
{
    var response = await client.Chat
        .CreateChatCompletion("anthropic/claude-3-haiku")
        .AddSystemMessage("You are a helpful programming assistant.")
        .AddUserMessage("Explain what async/await means in C#")
        .WithTemperature(0.7)
        .WithMaxTokens(200)
        .SendAsync();

    // Access response details
    Console.WriteLine($"Response ID: {response.Id}");
    Console.WriteLine($"Model Used: {response.Model}");
    Console.WriteLine($"Created: {response.CreatedAt:yyyy-MM-dd HH:mm:ss}");
    
    // Get the response content
    var content = response.FirstChoiceContent;
    Console.WriteLine($"Response: {content}");
    
    // Check token usage
    if (response.Usage != null)
    {
        Console.WriteLine($"Tokens used: {response.Usage.TotalTokens}");
        Console.WriteLine($"  - Prompt: {response.Usage.PromptTokens}");
        Console.WriteLine($"  - Completion: {response.Usage.CompletionTokens}");
    }
}
catch (OpenRouterException ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
    Console.WriteLine($"Error Type: {ex.ErrorType}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
}
```

#### Key Concepts
- [`ChatCompletionResponse`](../OpenRouter/Models/Responses/ChatCompletionResponse.cs:6) contains all response metadata and choices
- `FirstChoiceContent` provides quick access to the main response text
- [`Usage`](../OpenRouter/Models/Responses/Usage.cs:1) property tracks token consumption for billing
- [`OpenRouterException`](../OpenRouter/Core/Exceptions/OpenRouterException.cs:1) handles API-specific errors

#### Response Processing Variations

```csharp
// Process multiple choices if available
foreach (var choice in response.Choices)
{
    Console.WriteLine($"Choice {choice.Index}: {choice.Message?.Content}");
    Console.WriteLine($"Finish Reason: {choice.FinishReason}");
}

// Check completion status
var firstChoice = response.Choices.FirstOrDefault();
if (firstChoice?.IsCompleted == true)
{
    Console.WriteLine("Response completed successfully");
}
```

## Dependency Injection Setup

### ASP.NET Core Integration

Complete setup for ASP.NET Core applications using [`ServiceCollectionExtensions`](../OpenRouter/Extensions/ServiceCollectionExtensions.cs:18):

```csharp
// Program.cs
using OpenRouter.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add OpenRouter services
builder.Services.AddOpenRouter(options =>
{
    options.ApiKey = builder.Configuration["OpenRouter:ApiKey"]!;
    options.BaseUrl = "https://openrouter.ai/api/v1";
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
    options.RetryDelay = TimeSpan.FromSeconds(1);
    options.RequestTimeout = TimeSpan.FromSeconds(120);
});

// Add other services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### Controller Implementation

```csharp
// Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using OpenRouter.Core;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IOpenRouterClient _openRouterClient;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IOpenRouterClient openRouterClient, ILogger<ChatController> logger)
    {
        _openRouterClient = openRouterClient;
        _logger = logger;
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] ChatRequest request)
    {
        try
        {
            _logger.LogInformation("Processing chat completion request");

            var response = await _openRouterClient.Chat
                .CreateChatCompletion(request.Model ?? "gpt-3.5-turbo")
                .AddUserMessage(request.Message)
                .WithTemperature(request.Temperature ?? 0.7)
                .WithMaxTokens(request.MaxTokens ?? 150)
                .SendAsync();

            return Ok(new ChatApiResponse 
            { 
                Content = response.FirstChoiceContent,
                Model = response.Model,
                TokensUsed = response.Usage?.TotalTokens ?? 0
            });
        }
        catch (OpenRouterException ex)
        {
            _logger.LogError(ex, "OpenRouter API error");
            return BadRequest(new { error = ex.Message, type = ex.ErrorType });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class ChatRequest
{
    public required string Message { get; set; }
    public string? Model { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
}

public class ChatApiResponse
{
    public required string Content { get; set; }
    public required string Model { get; set; }
    public int TokensUsed { get; set; }
}
```

#### Key Concepts
- [`AddOpenRouter()`](../OpenRouter/Extensions/ServiceCollectionExtensions.cs:18) registers all necessary services
- [`IOpenRouterClient`](../OpenRouter/Core/IOpenRouterClient.cs:9) is injected as a scoped service
- Configuration is bound from `appsettings.json`
- Proper logging and error handling are included

## Error Handling Example

### Basic Try-Catch Pattern

Comprehensive error handling for production applications:

```csharp
using OpenRouter.Core;
using OpenRouter.Core.Exceptions;

public class ChatService
{
    private readonly IOpenRouterClient _client;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IOpenRouterClient client, ILogger<ChatService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> GetCompletionAsync(string prompt, string model = "gpt-3.5-turbo")
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                _logger.LogInformation("Attempting chat completion (attempt {Attempt})", retryCount + 1);

                var response = await _client.Chat
                    .CreateChatCompletion(model)
                    .AddUserMessage(prompt)
                    .WithTimeout(TimeSpan.FromSeconds(30))
                    .SendAsync();

                _logger.LogInformation("Chat completion successful");
                return response.FirstChoiceContent;
            }
            catch (OpenRouterAuthenticationException ex)
            {
                _logger.LogError(ex, "Authentication failed - check API key");
                throw new InvalidOperationException("Invalid API credentials", ex);
            }
            catch (OpenRouterRateLimitException ex)
            {
                _logger.LogWarning("Rate limit exceeded, waiting {Delay}ms", ex.RetryAfterMs);
                
                if (ex.RetryAfterMs > 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(ex.RetryAfterMs));
                }
                
                retryCount++;
                continue;
            }
            catch (OpenRouterModelException ex)
            {
                _logger.LogError(ex, "Model error: {Message}", ex.Message);
                throw new ArgumentException($"Model '{model}' error: {ex.Message}", ex);
            }
            catch (OpenRouterNetworkException ex)
            {
                _logger.LogWarning(ex, "Network error on attempt {Attempt}", retryCount + 1);
                
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Max retries exceeded for network errors");
                    throw new ServiceUnavailableException("OpenRouter service is unavailable", ex);
                }
                
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // Exponential backoff
                continue;
            }
            catch (OpenRouterException ex)
            {
                _logger.LogError(ex, "OpenRouter API error: {ErrorType}", ex.ErrorType);
                throw new ApplicationException($"API error: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogWarning("Request timeout on attempt {Attempt}", retryCount + 1);
                
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    throw new TimeoutException("Request timed out after multiple attempts", ex);
                }
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during chat completion");
                throw new ApplicationException("An unexpected error occurred", ex);
            }
        }

        throw new ApplicationException("Failed to complete request after all retries");
    }
}

public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

#### Key Concepts
- Different exception types require different handling strategies
- [`OpenRouterRateLimitException`](../OpenRouter/Core/Exceptions/OpenRouterRateLimitException.cs:1) includes retry timing information
- Exponential backoff helps avoid overwhelming the service
- Comprehensive logging aids in debugging and monitoring

## Configuration Example

### appsettings.json Setup

Complete configuration setup for different environments:

```json
{
  "OpenRouter": {
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "RequestTimeout": "00:02:00",
    "EnableRetry": true,
    "MaxRetryAttempts": 3,
    "RetryDelay": "00:00:01",
    "EnableLogging": true,
    "LogLevel": "Information",
    "ValidateApiKey": true,
    "ThrowOnApiErrors": true,
    "DefaultModel": "gpt-3.5-turbo",
    "DefaultTemperature": 0.7,
    "DefaultMaxTokens": 150
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "OpenRouter": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### Environment-Specific Configuration

```json
// appsettings.Development.json
{
  "OpenRouter": {
    "EnableLogging": true,
    "LogLevel": "Debug",
    "RequestTimeout": "00:05:00"
  }
}

// appsettings.Production.json
{
  "OpenRouter": {
    "EnableLogging": false,
    "LogLevel": "Warning",
    "RequestTimeout": "00:01:30",
    "MaxRetryAttempts": 5
  }
}
```

#### Configuration Binding

```csharp
// Program.cs - Advanced configuration
var builder = WebApplication.CreateBuilder(args);

// Bind configuration to options
builder.Services.Configure<OpenRouterOptions>(
    builder.Configuration.GetSection("OpenRouter"));

// Register with configuration validation
builder.Services.AddOpenRouter(options =>
{
    builder.Configuration.GetSection("OpenRouter").Bind(options);
    
    // Override with environment variables if present
    options.ApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? options.ApiKey;
    
    // Validate required settings
    if (string.IsNullOrEmpty(options.ApiKey))
    {
        throw new InvalidOperationException("OpenRouter API key is required");
    }
});

// Add options validation
builder.Services.AddOptions<OpenRouterOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        config.GetSection("OpenRouter").Bind(options);
    })
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

#### Key Concepts
- Environment-specific configurations override base settings
- Configuration validation ensures required values are present
- Environment variables can override configuration file settings
- [`OpenRouterOptions`](../OpenRouter/Core/OpenRouterOptions.cs:5) supports data annotations for validation

#### Usage in Services

```csharp
public class ConfigurableService
{
    private readonly OpenRouterOptions _options;
    private readonly IOpenRouterClient _client;

    public ConfigurableService(IOptions<OpenRouterOptions> options, IOpenRouterClient client)
    {
        _options = options.Value;
        _client = client;
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        var response = await _client.Chat
            .CreateChatCompletion(_options.DefaultModel ?? "gpt-3.5-turbo")
            .AddUserMessage(prompt)
            .WithTemperature(_options.DefaultTemperature ?? 0.7)
            .WithMaxTokens(_options.DefaultMaxTokens ?? 150)
            .SendAsync();

        return response.FirstChoiceContent;
    }
}
```

---

**Next Steps:**
- [Chat Examples →](chat-examples.md) - Explore advanced chat completion features
- [Streaming Examples →](streaming-examples.md) - Learn real-time response handling
- [Integration Examples →](integration-examples.md) - See platform-specific implementations