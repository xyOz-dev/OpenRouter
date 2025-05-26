# OpenRouter .NET Library

[![License: Custom](https://img.shields.io/badge/License-Custom-blue.svg)](LICENSE)

A comprehensive .NET client library for the OpenRouter API - the unified gateway to access multiple AI models including OpenAI, Anthropic, Meta, Google, and more.

## 📚 Documentation

**Complete documentation is now available in the [`docs/`](docs/) directory:**

- **[📋 Getting Started Guide](docs/getting-started.md)** - Installation, setup, and first steps
- **[🔐 Authentication](docs/authentication.md)** - API keys, tokens, and OAuth setup
- **[🏗️ Dependency Injection](docs/dependency-injection.md)** - ASP.NET Core integration
- **[💬 Chat Completions](docs/features/chat-completions.md)** - Core chat functionality
- **[📊 Model Management](docs/features/models.md)** - Discovering and querying models
- **[🔄 Streaming Support](docs/features/streaming.md)** - Real-time streaming responses
- **[🛠️ Advanced Features](docs/advanced/)** - Tools, structured outputs, and more
- **[📖 API Reference](docs/api-reference/)** - Complete method documentation
- **[💡 Examples](docs/examples/)** - Practical usage examples

🚀 **Start here:** [`docs/index.md`](docs/index.md)

## ✨ Key Features

- **🚀 Easy Integration** - Simple, intuitive API with fluent builder pattern ([`IChatRequestBuilder`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:1))
- **🔄 Streaming Support** - Real-time response streaming with async enumerable
- **🔐 Multiple Authentication** - API Key, Bearer Token, and OAuth PKCE flows
- **🛠️ Advanced Features** - Tools, structured outputs, web search capabilities
- **📊 Model Management** - List, filter, and get detailed model information ([`IModelsService`](OpenRouter/Core/IOpenRouterClient.cs:12))
- **💰 Cost Management** - Credit tracking and usage monitoring ([`ICreditsService`](OpenRouter/Core/IOpenRouterClient.cs:13))
- **🔧 Provider Routing** - Automatic fallbacks and intelligent load balancing
- **🖼️ Multimodal Support** - Text, images, and other media types
- **🏗️ Dependency Injection** - Built-in DI container support ([`ServiceCollectionExtensions`](OpenRouter/Extensions/ServiceCollectionExtensions.cs:16))
- **⚡ Async/Await** - Full async support with cancellation tokens
- **🛡️ Error Handling** - Comprehensive exception handling ([`OpenRouterException`](OpenRouter/Exceptions/OpenRouterException.cs:3), [`OpenRouterApiException`](OpenRouter/Exceptions/OpenRouterApiException.cs:3))

## ⚡ Quick Start

### Installation

Clone the repository and reference the project in your solution:

```bash
git clone https://github.com/xyOz-dev/LogiQ.OpenRouter.git
```

Or add the project reference to your .csproj file:

```xml
<ProjectReference Include="path/to/OpenRouter/OpenRouter.csproj" />
```

### Basic Usage

```csharp
using OpenRouter.Core;

// Initialize client
var client = new OpenRouterClient("your-api-key-here");

// Simple chat completion
var response = await client.Chat
    .CreateRequest()
    .WithModel("meta-llama/llama-3.1-8b-instruct:free")
    .AddUserMessage("Hello, how are you?")
    .SendAsync();

Console.WriteLine(response.GetFirstChoiceMessageContent());
```

### Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.Extensions;

services.AddOpenRouter(options =>
{
    options.ApiKey = "your-api-key-here";
});

var client = serviceProvider.GetRequiredService<IOpenRouterClient>();
```

## 🎯 Core Services

The library is built around a central [`IOpenRouterClient`](OpenRouter/Core/IOpenRouterClient.cs:9) interface with specialized services:

- **[`IChatService`](OpenRouter/Core/IOpenRouterClient.cs:11)** - Chat completions with fluent API
- **[`IModelsService`](OpenRouter/Core/IOpenRouterClient.cs:12)** - Model discovery and querying
- **[`ICreditsService`](OpenRouter/Core/IOpenRouterClient.cs:13)** - Credit monitoring and usage tracking
- **[`IKeysService`](OpenRouter/Core/IOpenRouterClient.cs:14)** - API key management
- **[`IAuthService`](OpenRouter/Core/IOpenRouterClient.cs:15)** - OAuth authentication flows

## 🔧 Configuration

```csharp
services.AddOpenRouter(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://openrouter.ai";
    options.UserAgent = "MyApp/1.0.0";
    options.Timeout = TimeSpan.FromSeconds(30);
    options.MaxRetries = 3;
});
```

For comprehensive configuration options, see [`OpenRouterOptions`](OpenRouter/Core/IOpenRouterClient.cs:21).

## 🚀 Examples

### Streaming Chat
```csharp
await foreach (var chunk in client.Chat
    .CreateRequest()
    .WithModel("openai/gpt-4o")
    .AddUserMessage("Tell me a story")
    .WithStreaming(true)
    .SendStreamAsync())
{
    Console.Write(chunk.GetDeltaContent());
}
```

### Tool Calling
```csharp
var response = await client.Chat
    .CreateRequest()
    .WithModel("openai/gpt-4o")
    .AddUserMessage("What's the weather like?")
    .AddTool("get_weather", "Get current weather", new
    {
        type = "object",
        properties = new
        {
            location = new { type = "string", description = "City name" }
        }
    })
    .SendAsync();
```

### Model Discovery
```csharp
var models = await client.Models.GetModelsAsync();
var gptModels = models.Data.Where(m => m.Id.Contains("gpt"));
```

For more examples, visit the [`docs/examples/`](docs/examples/) directory.

## 🧪 Testing

```bash
# Run unit tests
dotnet test OpenRouter.Tests --filter Category=Unit

# Run integration tests (requires API key)
export OPENROUTER_API_KEY="your-api-key-here"
dotnet test OpenRouter.Tests --filter Category=Integration
```

## 🛠️ Development

```bash
git clone https://github.com/xyOz-dev/LogiQ.OpenRouter.git
cd OpenRouter
dotnet restore
dotnet build
dotnet test
```

## 📄 License

This project is licensed under a custom license that allows free use but prohibits commercial sale - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

## 📞 Support

- 📚 **[Complete Documentation](docs/index.md)** - Start here for comprehensive guides
- 🌐 **[OpenRouter API Docs](https://openrouter.ai/docs)** - Official API documentation
- 🐛 **[Bug Reports](https://github.com/xyOz-dev/LogiQ.OpenRouter/issues)** - GitHub Issues

---

*Built with ❤️ for the .NET community*