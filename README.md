# OpenRouter.NET SDK

Welcome to my personal .NET SDK for OpenRouter! This SDK provides a convenient way to integrate your .NET applications with the OpenRouter API, allowing you to leverage a diverse range of AI models for various tasks.

## Overview

OpenRouter is a platform that provides access to a multitude of Large Language Models (LLMs) and other AI models through a unified API. This SDK simplifies the process of making requests to the OpenRouter API, handling authentication, and processing responses.

## Features

*   **Easy-to-use Client**: Intuitive `OpenRouterClient` for interacting with the API.
*   **Comprehensive Model Support**: Access any model available on OpenRouter.
*   **Chat Completions**: Both blocking and streaming responses for chat interactions.
*   **Model Management**: List available models, get model details, and more.
*   **Built-in Retries**: Resilient HTTP client with Polly for transient error handling.
*   **Dependency Injection**: Extension methods for easy integration with `Microsoft.Extensions.DependencyInjection`.
*   **Strongly Typed**: Clear request and response models.

## Getting Started

To start using the SDK, check out the [Getting Started Guide](getting-started.md). This guide covers installation and basic configuration.

## Core Concepts

Understand the fundamental components of the SDK:
*   [OpenRouterClient and Options](core-concepts/client-and-options.md)
*   [Available Services](core-concepts/services.md)
*   [Request and Response Models](core-concepts/data-models.md)

## Key Functionalities

Dive deeper into specific features:
*   [Chat Completions](features/chat-completions.md) (including streaming and conversation management)
*   [Managing Models](features/model-management.md) (listing, details, etc.)
*   [Authentication](features/authentication.md)
*   [Error Handling](features/error-handling.md)

## Examples

For practical examples of how to use the SDK, please refer to the [`OpenRouter.Examples`](../OpenRouter.Examples/) project in this repository. It demonstrates various use cases, including:
*   Basic Chat
*   Streaming Chat
*   Conversation Management
*   Listing and Filtering Models
*   Retrieving Model Details
*   Using Custom Parameters

## Advanced Topics
*   [Dependency Injection Setup](advanced/dependency-injection.md)
*   [HTTP Client Customization](advanced/http-client.md)

## Contributing

Contributions are welcome! Please refer to the main project `README.md` for guidelines.