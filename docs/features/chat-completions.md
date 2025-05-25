# Chat Completions

## Introduction

The .NET OpenRouter library provides comprehensive chat completion capabilities, enabling developers to interact with various language models through OpenRouter's unified API. The library offers both direct API usage and a fluent builder pattern for constructing complex chat requests with ease.

## Basic Usage

### Creating ChatCompletionRequest Manually

You can create chat completion requests directly using the [`ChatCompletionRequest`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:1) model:

```csharp
var request = new ChatCompletionRequest
{
    Model = "anthropic/claude-3-haiku",
    Messages = new[]
    {
        new Message { Role = "user", Content = "Hello, world!" }
    }
};
```

### Using IChatService Directly

Access the chat service through the [`IChatService`](../../OpenRouter/Core/IOpenRouterClient.cs:11) interface:

```csharp
var response = await client.Chat.CreateChatCompletionAsync(request);
Console.WriteLine(response.Choices[0].Message.Content);
```

### Handling ChatCompletionResponse

Process responses using the [`ChatCompletionResponse`](../../OpenRouter/Models/Responses/ChatCompletionResponse.cs:1) model:

<!-- C# Code Example: Processing chat completion response with choices, usage, and metadata -->

## Fluent Builder Pattern

### IChatRequestBuilder Interface Overview

The [`IChatRequestBuilder`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:7) interface provides a fluent, chainable API for constructing chat requests:

```csharp
var response = await client.Chat.CreateRequest()
    .WithModel("anthropic/claude-3-haiku")
    .WithUserMessage("Hello, world!")
    .ExecuteAsync();
```

### Core Builder Methods

#### WithModel()

The [`WithModel()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:9) method specifies which language model to use:

<!-- C# Code Example: Model selection with different providers -->

#### WithMessages()

The [`WithMessages()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:10) method accepts an array of messages for complex conversations:

<!-- C# Code Example: Multi-turn conversation setup -->

### Message Helper Methods

#### WithSystemMessage()

The [`WithSystemMessage()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:11) method sets the system prompt:

```csharp
builder.WithSystemMessage("You are a helpful assistant specialized in programming.");
```

#### WithUserMessage()

The [`WithUserMessage()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:12) method adds user messages:

```csharp
builder.WithUserMessage("Explain dependency injection in .NET");
```

#### WithAssistantMessage()

The [`WithAssistantMessage()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:14) method adds assistant responses for conversation context:

<!-- C# Code Example: Building conversation history with assistant messages -->

### Chain Building and Execution

Build and execute requests in a single fluent chain using [`ExecuteAsync()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:42):

```csharp
var response = await client.Chat.CreateRequest()
    .WithModel("anthropic/claude-3-haiku")
    .WithSystemMessage("You are a coding assistant")
    .WithUserMessage("Write a hello world in C#")
    .WithTemperature(0.7)
    .WithMaxTokens(150)
    .ExecuteAsync();
```

## Message Types and Content

### Message Model Structure

The [`Message`](../../OpenRouter/Models/Common/Message.cs:1) model supports various content types and roles:

<!-- C# Code Example: Message structure with role, content, and metadata -->

### Text and Multimodal Content Support

Support for text, images, and other content types:

<!-- C# Code Example: Multimodal message with text and image content -->

### Message Roles and Formatting

Proper message role usage for optimal model performance:

<!-- C# Code Example: Conversation with system, user, and assistant roles -->

## Model Parameters

### Temperature

Control response randomness with temperature settings:

```csharp
builder.WithTemperature(0.7); // Balanced creativity
builder.WithTemperature(0.0); // Deterministic
builder.WithTemperature(1.0); // Highly creative
```

### Max Tokens

Limit response length:

```csharp
builder.WithMaxTokens(150);
```

### Top-p

Nucleus sampling parameter:

```csharp
builder.WithTopP(0.9);
```

### Top-k

Top-k sampling parameter:

```csharp
builder.WithTopK(40);
```

### Penalties

Control repetition and creativity:

```csharp
builder.WithFrequencyPenalty(0.5);
builder.WithPresencePenalty(0.6);
builder.WithRepetitionPenalty(1.1);
```

## Code Examples

### Complete Request/Response Scenarios

<!-- C# Code Example: Complete chat completion workflow -->

<!-- C# Code Example: Error handling and retry logic -->

<!-- C# Code Example: Conversation management with message history -->

<!-- C# Code Example: Advanced parameter configuration -->