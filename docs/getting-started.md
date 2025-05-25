# Getting Started with OpenRouter .NET

This guide will walk you through installing, configuring, and making your first requests with the OpenRouter .NET library.

## Prerequisites

- **.NET 8.0 or later** - The library targets .NET 8.0+ for optimal performance and modern language features
- **OpenRouter API Key** - [Sign up at OpenRouter](https://openrouter.ai) to get your API key

## Installation

### Clone the Repository

```bash
git clone https://github.com/xyOz-dev/OpenRouter.git
```

### Add Project Reference

Add the project reference to your .csproj file:

```xml
<ProjectReference Include="path/to/OpenRouter/OpenRouter.csproj" />
```

### Using Visual Studio

1. Right-click on your solution in Solution Explorer
2. Select "Add" > "Existing Project..."
3. Navigate to and select the OpenRouter.csproj file
4. Right-click on your project and select "Add" > "Project Reference..."
5. Check the OpenRouter project and click "OK"

## Basic Setup

### Creating an OpenRouterClient Instance

The simplest way to get started is by creating an [`OpenRouterClient`](OpenRouter/Core/OpenRouterClient.cs:1) instance directly:

```csharp
using OpenRouter.Core;

// Basic client with API key
var client = new OpenRouterClient("YOUR_API_KEY");

// Client with custom configuration
var client = new OpenRouterClient("YOUR_API_KEY", options =>
{
    options.BaseUrl = "https://openrouter.ai/api/v1";
    options.Timeout = TimeSpan.FromSeconds(30);
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
});
```

### API Key Configuration

#### Environment Variable (Recommended)

Store your API key in an environment variable for security:

```bash
# Windows (Command Prompt)
set OPENROUTER_API_KEY=your_api_key_here

# Windows (PowerShell)
$env:OPENROUTER_API_KEY="your_api_key_here"

# macOS/Linux
export OPENROUTER_API_KEY="your_api_key_here"
```

```csharp
var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
    ?? throw new InvalidOperationException("OPENROUTER_API_KEY not found");

var client = new OpenRouterClient(apiKey);
```

#### Configuration File

Use `appsettings.json` for ASP.NET Core applications:

```json
{
  "OpenRouter": {
    "ApiKey": "your_api_key_here",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "Timeout": "00:00:30",
    "EnableRetry": true,
    "MaxRetryAttempts": 3
  }
}
```

### Basic OpenRouterOptions Setup

Configure [`OpenRouterOptions`](OpenRouter/Core/OpenRouterOptions.cs:5) for advanced scenarios:

```csharp
var options = new OpenRouterOptions
{
    ApiKey = "YOUR_API_KEY",
    BaseUrl = "https://openrouter.ai/api/v1",
    Timeout = TimeSpan.FromSeconds(30),
    RequestTimeout = TimeSpan.FromSeconds(120),
    MaxRetryAttempts = 3,
    RetryDelay = TimeSpan.FromSeconds(1),
    EnableRetry = true,
    EnableLogging = true,
    LogLevel = LogLevel.Information,
    ValidateApiKey = true,
    ThrowOnApiErrors = true
};

var client = new OpenRouterClient(
    new OpenRouterHttpClient(new HttpClient(), 
        new BearerTokenProvider(options.ApiKey, options.ValidateApiKey), 
        options, null), 
    options);
```

## Your First Request

### Simple Chat Completion

Create a basic chat completion using the [`IChatService`](OpenRouter/Core/IOpenRouterClient.cs:11):

```csharp
using OpenRouter.Core;

var client = new OpenRouterClient("YOUR_API_KEY");

try
{
    var response = await client.Chat
        .CreateChatCompletion("gpt-3.5-turbo")
        .AddUserMessage("What is the capital of France?")
        .SendAsync();

    Console.WriteLine($"Response: {response.FirstChoiceContent}");
    Console.WriteLine($"Model: {response.Model}");
    Console.WriteLine($"Usage: {response.Usage?.TotalTokens} tokens");
}
catch (OpenRouterException ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
}
```

### Response Handling with ChatCompletionResponse

Work with the [`ChatCompletionResponse`](OpenRouter/Models/Responses/ChatCompletionResponse.cs:6) object:

```csharp
var response = await client.Chat
    .CreateChatCompletion("gpt-3.5-turbo")
    .AddUserMessage("Tell me a joke")
    .SendAsync();

// Access response properties
Console.WriteLine($"Response ID: {response.Id}");
Console.WriteLine($"Created: {response.CreatedAt}");
Console.WriteLine($"Model: {response.Model}");

// Access choices
foreach (var choice in response.Choices)
{
    Console.WriteLine($"Choice {choice.Index}: {choice.Message?.Content}");
    Console.WriteLine($"Finish Reason: {choice.FinishReason}");
    
    // Check completion status
    if (choice.IsCompleted)
    {
        Console.WriteLine("Response completed successfully");
    }
}

// Usage information
if (response.Usage != null)
{
    Console.WriteLine($"Prompt tokens: {response.Usage.PromptTokens}");
    Console.WriteLine($"Completion tokens: {response.Usage.CompletionTokens}");
    Console.WriteLine($"Total tokens: {response.Usage.TotalTokens}");
}
```

## Dependency Injection Setup

For ASP.NET Core applications, use the built-in DI container integration.

### Basic Registration

Use [`ServiceCollectionExtensions.AddOpenRouter()`](OpenRouter/Extensions/ServiceCollectionExtensions.cs:18) in `Program.cs`:

```csharp
using OpenRouter.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Basic registration with API key
builder.Services.AddOpenRouter("YOUR_API_KEY");

// Or with configuration
builder.Services.AddOpenRouter(options =>
{
    options.ApiKey = builder.Configuration["OpenRouter:ApiKey"]!;
    options.BaseUrl = "https://openrouter.ai/api/v1";
    options.EnableRetry = true;
    options.MaxRetryAttempts = 3;
});

var app = builder.Build();
```

### Configuration from appsettings.json

```csharp
// Program.cs
builder.Services.AddOpenRouter(options =>
    builder.Configuration.GetSection("OpenRouter").Bind(options));
```

```json
// appsettings.json
{
  "OpenRouter": {
    "ApiKey": "your_api_key_here",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "Timeout": "00:00:30",
    "EnableRetry": true,
    "MaxRetryAttempts": 3
  }
}
```

### Controller/Service Injection Examples

Inject the [`IOpenRouterClient`](OpenRouter/Core/IOpenRouterClient.cs:9) into your controllers or services:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IOpenRouterClient _openRouterClient;

    public ChatController(IOpenRouterClient openRouterClient)
    {
        _openRouterClient = openRouterClient;
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] string prompt)
    {
        try
        {
            var response = await _openRouterClient.Chat
                .CreateChatCompletion("gpt-3.5-turbo")
                .AddUserMessage(prompt)
                .SendAsync();

            return Ok(new { content = response.FirstChoiceContent });
        }
        catch (OpenRouterException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

## Next Steps

Now that you have the basics working, explore these topics:

- **[Authentication Methods](authentication.md)** - Learn about different authentication options
- **[Dependency Injection](dependency-injection.md)** - Advanced DI scenarios and testing
- **[Chat Features](features/chat.md)** - Explore the full chat completion API
- **[Model Management](features/models.md)** - Discover and work with different AI models
- **[Error Handling](features/error-handling.md)** - Robust error handling patterns
- **[Examples](examples/)** - Real-world usage examples

<!-- C# Code Example: Advanced chat completion with system message and function calling -->

---

**Next**: [Authentication Methods →](authentication.md)