# OpenRouter .NET Library Documentation

Welcome to the OpenRouter .NET library documentation! This comprehensive library provides a robust, feature-rich interface to the OpenRouter API, designed specifically for .NET developers who need powerful AI chat completion capabilities.

## Overview

OpenRouter .NET is a modern, async-first library that simplifies integration with OpenRouter's AI model platform. Built with .NET best practices, it offers a fluent API design, comprehensive error handling, and seamless dependency injection support.

## Key Features

- **🚀 Chat Completions with Fluent API**: Create chat completion requests with an intuitive, builder-pattern API using [`IChatService`](OpenRouter/Core/IOpenRouterClient.cs:11)
- **📊 Model Management**: Discover and query available AI models through [`IModelsService`](OpenRouter/Core/IOpenRouterClient.cs:12)
- **💰 Credit Monitoring**: Track your API usage and credits with [`ICreditsService`](OpenRouter/Core/IOpenRouterClient.cs:13)
- **🔑 API Key Management**: Manage your API keys using [`IKeysService`](OpenRouter/Core/IOpenRouterClient.cs:14)
- **🔐 OAuth Authentication**: Full OAuth PKCE flow support via [`IAuthService`](OpenRouter/Core/IOpenRouterClient.cs:15)
- **🛡️ Comprehensive Error Handling**: Robust error handling with detailed exception information
- **🔧 Dependency Injection Integration**: First-class support for Microsoft.Extensions.DependencyInjection

## Quick Navigation

- **[Getting Started Guide](getting-started.md)** - Installation and basic setup
- **[Authentication](authentication.md)** - API keys, tokens, and OAuth configuration
- **[Dependency Injection](dependency-injection.md)** - DI container registration and configuration
- **[Examples](examples/)** - Practical usage examples
- **[API Reference](api-reference/)** - Complete API documentation
- **[Advanced Features](advanced/)** - Streaming, retry policies, and advanced configurations

## Installation

Install the OpenRouter .NET library from NuGet:

```bash
dotnet add package OpenRouter
```

Or via Package Manager Console:

```powershell
Install-Package OpenRouter
```

## Quick Start

Get up and running with just a few lines of code:

```csharp
// Simple client initialization
var client = new OpenRouterClient("YOUR_API_KEY");

// Your first chat completion
var response = await client.Chat
    .CreateChatCompletion("gpt-3.5-turbo")
    .AddUserMessage("Hello, world!")
    .SendAsync();

Console.WriteLine(response.FirstChoiceContent);
```

<!-- C# Code Example: Complete chat completion with response handling -->

## Next Steps

1. **[Install and configure](getting-started.md#installation)** the library in your project
2. **[Set up authentication](authentication.md)** with your OpenRouter API key
3. **[Configure dependency injection](dependency-injection.md)** for ASP.NET Core applications
4. **[Explore examples](examples/)** for common use cases
5. **[Review the API reference](api-reference/)** for detailed method documentation

## Support and Community

- **GitHub Repository**: [OpenRouter.NET](https://github.com/your-repo/openrouter-dotnet)
- **Issues and Bug Reports**: [GitHub Issues](https://github.com/your-repo/openrouter-dotnet/issues)
- **Documentation**: [https://docs.openrouter.ai](https://docs.openrouter.ai)

---

*Built with ❤️ for the .NET community*