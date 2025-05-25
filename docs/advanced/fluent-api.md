# Fluent API Deep Dive

The OpenRouter .NET library provides a comprehensive fluent API that enables intuitive and readable chat completion requests through method chaining. This document explores the design principles, implementation patterns, and advanced usage of the fluent API.

## Fluent Pattern Overview

### Benefits and Design Principles

The fluent API pattern offers several key advantages:

- **Readability**: Method chaining creates self-documenting code that reads like natural language
- **Discoverability**: IntelliSense and auto-completion guide developers through available options
- **Type Safety**: Generic constraints and method signatures prevent runtime errors
- **Immutability**: Each method call returns a new builder instance, ensuring thread safety
- **Progressive Disclosure**: Simple methods for basic use cases, advanced methods for complex scenarios

The design follows these core principles:
- Methods return the builder interface to enable chaining
- Parameter validation occurs at build/execute time
- Complex configurations are exposed through overloads and action delegates
- The API surface remains minimal while providing comprehensive functionality

## Builder Interface Design

### [`IChatRequestBuilder`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:7) Method Chaining

The [`IChatRequestBuilder`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:7) interface serves as the foundation for all fluent operations. Each configuration method returns the same interface, enabling seamless chaining:

```csharp
var response = await client.Chat
    .WithModel("anthropic/claude-3-sonnet")
    .WithMessages(messages)
    .WithMaxTokens(1000)
    .WithTemperature(0.7)
    .ExecuteAsync();
```

### Method Return Patterns and Fluency

The fluent pattern maintains consistency across all builder methods:
- Configuration methods return [`IChatRequestBuilder`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:7)
- Terminal methods ([`ExecuteAsync()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:42), [`ExecuteStreamAsync()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:43), [`Build()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:45)) return task or request objects
- Method overloads provide flexibility without breaking the chain

<!-- C# Code Example: Fluent chain demonstrating return patterns -->

## Configuration Methods

### Parameter Setting Methods

The builder provides intuitive methods for setting common parameters:

- **Model Selection**: [`WithModel(string model)`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:8)
- **Message Management**: [`WithMessages()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:9) variants for different input types
- **Output Control**: [`WithMaxTokens()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:17), [`WithTemperature()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:22), [`WithTopP()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:23)
- **Behavioral Settings**: [`WithStream()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:18), [`WithStop()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:19)

### Complex Configuration

Advanced scenarios are supported through action delegates and specialized methods:

```csharp
var response = await client.Chat
    .WithModel("gpt-4")
    .WithMessages(messages)
    .WithWebSearch(options => {
        options.MaxResults = 10;
        options.TimeRange = TimeRange.LastMonth;
    })
    .ExecuteAsync();
```

<!-- C# Code Example: Complex configuration with nested options -->

## Execution Methods

### [`ExecuteAsync()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:42) vs [`ExecuteStreamAsync()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:43)

The builder provides two primary execution patterns:

**Standard Execution**: [`ExecuteAsync()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:42) returns the complete response
- Suitable for most use cases
- Returns `Task<ChatCompletionResponse>`
- Handles buffering internally

**Streaming Execution**: [`ExecuteStreamAsync()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:43) provides real-time response chunks
- Optimal for user-facing applications
- Returns `IAsyncEnumerable<StreamingChatCompletionResponse>`
- Enables progressive UI updates

### [`Build()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:45) for Request Inspection

The [`Build()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:45) method allows request inspection without execution:

```csharp
var request = client.Chat
    .WithModel("gpt-4")
    .WithMessages(messages)
    .Build();

// Inspect or modify the request before sending
var jsonPayload = JsonSerializer.Serialize(request);
```

<!-- C# Code Example: Request inspection and modification -->

## Advanced Configurations

### [`WithStructuredOutput<T>()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:20) Generic Constraint

The structured output method leverages generic constraints for type safety:

```csharp
public class PersonInfo
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string[] Hobbies { get; set; }
}

var response = await client.Chat
    .WithModel("gpt-4")
    .WithMessages("Extract person info from: John is 30 and likes hiking, reading.")
    .WithStructuredOutput<PersonInfo>()
    .ExecuteAsync();
```

The generic constraint ensures the type can be serialized to JSON schema automatically.

### [`WithTools()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:15) and [`WithToolChoice()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:16)

Tool integration provides function calling capabilities:

<!-- C# Code Example: Tool definition and usage -->

### [`WithLogitBias()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:34) and [`WithLogprobs()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:35)

Advanced token-level control for specialized use cases:

```csharp
var response = await client.Chat
    .WithModel("gpt-4")
    .WithMessages(messages)
    .WithLogitBias(new Dictionary<string, float> { ["yes"] = 10.0f, ["no"] = -10.0f })
    .WithLogprobs(5)
    .ExecuteAsync();
```

<!-- C# Code Example: Logit bias and log probability configuration -->

## Custom Builder Extensions

### Extending the Fluent API

The fluent API can be extended through extension methods:

```csharp
public static class ChatBuilderExtensions
{
    public static IChatRequestBuilder WithConversationalTone(this IChatRequestBuilder builder)
    {
        return builder
            .WithTemperature(0.8f)
            .WithTopP(0.9f)
            .WithSystemMessage("Respond in a friendly, conversational tone.");
    }
    
    public static IChatRequestBuilder WithCodeGeneration(this IChatRequestBuilder builder)
    {
        return builder
            .WithTemperature(0.2f)
            .WithMaxTokens(2000)
            .WithSystemMessage("Generate clean, well-documented code with explanations.");
    }
}
```

<!-- C# Code Example: Custom extension methods for domain-specific configurations -->

## Code Examples

### Complex Fluent Chains and Patterns

<!-- C# Code Example: Multi-turn conversation with tools and structured output -->
<!-- C# Code Example: Conditional configuration based on runtime parameters -->
<!-- C# Code Example: Parallel request building with shared base configuration -->
<!-- C# Code Example: Error handling within fluent chains -->
<!-- C# Code Example: Integration with dependency injection and configuration services -->

The fluent API design enables both simple and complex use cases while maintaining readability and type safety throughout the development experience.