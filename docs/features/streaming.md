# Streaming Responses

## Streaming Overview

Real-time response processing provides significant benefits for user experience, allowing applications to display partial responses as they're generated rather than waiting for complete responses. This is particularly valuable for longer responses where immediate feedback improves perceived performance.

## Enable Streaming

### WithStreaming() Method

Enable streaming using the [`WithStreaming()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:18) method:

```csharp
var streamBuilder = client.Chat.CreateRequest()
    .WithModel("anthropic/claude-3-haiku")
    .WithUserMessage("Write a long story about space exploration")
    .WithStreaming(true);
```

### ExecuteStreamAsync() Usage

Execute streaming requests with [`ExecuteStreamAsync()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:43):

```csharp
await foreach (var chunk in streamBuilder.ExecuteStreamAsync())
{
    if (chunk.Choices?.FirstOrDefault()?.Delta?.Content != null)
    {
        Console.Write(chunk.Choices[0].Delta.Content);
    }
}
```

## Handling Stream Chunks

### ChatCompletionChunk Processing

Process individual chunks from the streaming response:

<!-- C# Code Example: Detailed chunk processing with delta content -->

### Async Enumerable Patterns with await foreach

Leverage C# async enumerable patterns for efficient streaming:

```csharp
await foreach (var chunk in response.WithCancellation(cancellationToken))
{
    // Process each chunk as it arrives
    ProcessChunk(chunk);
}
```

### Reconstructing Complete Responses from Chunks

Build complete responses from streaming chunks:

<!-- C# Code Example: Accumulating chunks into complete response -->

## Stream Error Handling

### OpenRouterStreamingException Handling

Handle streaming-specific exceptions using [`OpenRouterStreamingException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:113):

```csharp
try
{
    await foreach (var chunk in streamResponse)
    {
        ProcessChunk(chunk);
    }
}
catch (OpenRouterStreamingException ex)
{
    Console.WriteLine($"Streaming error: {ex.Message}");
    // Handle streaming-specific errors
}
```

### Connection Interruption Recovery

Implement robust error recovery for connection issues:

<!-- C# Code Example: Connection retry logic and graceful degradation -->

## Real-time UI Updates

### Blazor Component Integration

Integrate streaming responses with Blazor components:

<!-- C# Code Example: Blazor component with streaming chat interface -->

### Console Application Streaming Examples

Simple console streaming implementation:

```csharp
public async Task StreamChatToConsole()
{
    var stream = client.Chat.CreateRequest()
        .WithModel("anthropic/claude-3-haiku")
        .WithUserMessage("Explain quantum computing")
        .WithStreaming()
        .ExecuteStreamAsync();

    await foreach (var chunk in stream)
    {
        var content = chunk.Choices?.FirstOrDefault()?.Delta?.Content;
        if (!string.IsNullOrEmpty(content))
        {
            Console.Write(content);
        }
    }
    Console.WriteLine();
}
```

### Web API Streaming Responses

Stream responses through web APIs:

<!-- C# Code Example: ASP.NET Core streaming endpoint -->

## Performance Considerations

### Memory Management for Long Streams

Efficiently manage memory during long streaming sessions:

<!-- C# Code Example: Memory-efficient streaming with disposal patterns -->

### Cancellation Token Usage

Proper cancellation token implementation for streaming:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

try
{
    await foreach (var chunk in response.WithCancellation(cts.Token))
    {
        ProcessChunk(chunk);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Streaming cancelled due to timeout");
}
```

## Advanced Streaming Patterns

### Buffered Streaming

Implement buffered streaming for better user experience:

<!-- C# Code Example: Buffered streaming with word boundary detection -->

### Multi-Consumer Streaming

Share streaming responses across multiple consumers:

<!-- C# Code Example: Observable pattern for multiple stream consumers -->

### Stream Transformation

Transform streaming content in real-time:

<!-- C# Code Example: Real-time content filtering and transformation -->