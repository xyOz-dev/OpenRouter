# Streaming Implementation Examples

This guide demonstrates real-time streaming implementations using the OpenRouter .NET library. Each example shows how to handle streaming responses in different application contexts with proper error handling and performance optimization.

## Console Streaming

### Real-time Console Output

Display streaming responses directly to the console as they arrive:

```csharp
using OpenRouter.Core;
using OpenRouter.Models.Streaming;

public class ConsoleStreamingExample
{
    private readonly IOpenRouterClient _client;

    public ConsoleStreamingExample(IOpenRouterClient client)
    {
        _client = client;
    }

    public async Task StreamToConsoleAsync(string prompt)
    {
        Console.WriteLine("AI Response:");
        Console.WriteLine(new string('-', 50));

        try
        {
            var streamingRequest = _client.Chat
                .CreateStreamingChatCompletion("gpt-3.5-turbo")
                .AddUserMessage(prompt)
                .WithTemperature(0.7);

            var completeResponse = new StringBuilder();

            await foreach (var chunk in streamingRequest.StreamAsync())
            {
                if (chunk.Choices?.Any() == true)
                {
                    var deltaContent = chunk.Choices[0].Delta?.Content;
                    if (!string.IsNullOrEmpty(deltaContent))
                    {
                        Console.Write(deltaContent);
                        completeResponse.Append(deltaContent);
                    }
                }

                // Handle completion
                if (chunk.Choices?[0].FinishReason != null)
                {
                    Console.WriteLine("\n" + new string('-', 50));
                    Console.WriteLine($"Stream completed. Reason: {chunk.Choices[0].FinishReason}");
                    break;
                }
            }

            Console.WriteLine($"\nComplete response length: {completeResponse.Length} characters");
        }
        catch (OpenRouterException ex)
        {
            Console.WriteLine($"\nStreaming error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nUnexpected error: {ex.Message}");
        }
    }
}

// Usage
var streamingExample = new ConsoleStreamingExample(client);
await streamingExample.StreamToConsoleAsync("Write a short story about a robot learning to feel emotions.");
```

#### Key Concepts
- [`IAsyncEnumerable<StreamingChatResponse>`](../OpenRouter/Models/Streaming/StreamingChatResponse.cs:1) provides real-time chunks
- Delta content represents incremental text additions
- FinishReason indicates when streaming is complete
- StringBuilder accumulates the complete response

#### Performance Considerations
- Console output is synchronous and may introduce slight delays
- Consider buffering for very high-frequency updates
- Monitor memory usage for long responses

### Advanced Console Streaming with Progress

Enhanced console streaming with progress indicators and metadata display:

<!-- C# Code Example: Advanced console streaming with progress bar, token counting, and real-time statistics -->

```csharp
using OpenRouter.Core;
using System.Diagnostics;

public class AdvancedConsoleStreaming
{
    private readonly IOpenRouterClient _client;

    public AdvancedConsoleStreaming(IOpenRouterClient client)
    {
        _client = client;
    }

    public async Task StreamWithProgressAsync(string prompt, string model = "gpt-3.5-turbo")
    {
        var stopwatch = Stopwatch.StartNew();
        var tokenCount = 0;
        var chunkCount = 0;
        var completeResponse = new StringBuilder();

        Console.Clear();
        Console.WriteLine($"Model: {model}");
        Console.WriteLine($"Prompt: {prompt}");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine();

        // Reserve space for progress info
        var responseStartLine = Console.CursorTop;
        Console.WriteLine();
        Console.WriteLine();
        var statsLine = Console.CursorTop;

        try
        {
            await foreach (var chunk in _client.Chat
                .CreateStreamingChatCompletion(model)
                .AddUserMessage(prompt)
                .WithTemperature(0.7)
                .StreamAsync())
            {
                chunkCount++;

                if (chunk.Choices?.Any() == true)
                {
                    var deltaContent = chunk.Choices[0].Delta?.Content;
                    if (!string.IsNullOrEmpty(deltaContent))
                    {
                        // Update response area
                        var currentLine = Console.CursorTop;
                        Console.SetCursorPosition(0, responseStartLine);
                        
                        completeResponse.Append(deltaContent);
                        var responseText = completeResponse.ToString();
                        
                        // Word wrap for console
                        var wrappedText = WrapText(responseText, Console.WindowWidth - 2);
                        Console.Write(wrappedText);
                        
                        // Update statistics
                        Console.SetCursorPosition(0, statsLine);
                        Console.WriteLine($"Chunks: {chunkCount} | Characters: {responseText.Length} | Time: {stopwatch.Elapsed.TotalSeconds:F1}s");
                        Console.WriteLine($"Speed: {responseText.Length / Math.Max(stopwatch.Elapsed.TotalSeconds, 0.1):F1} chars/sec");
                    }

                    // Check for completion
                    if (chunk.Choices[0].FinishReason != null)
                    {
                        Console.SetCursorPosition(0, statsLine + 3);
                        Console.WriteLine($"✅ Completed: {chunk.Choices[0].FinishReason}");
                        Console.WriteLine($"📊 Final Stats - Chunks: {chunkCount}, Time: {stopwatch.Elapsed.TotalSeconds:F2}s");
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.SetCursorPosition(0, statsLine + 3);
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    private static string WrapText(string text, int maxWidth)
    {
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > maxWidth)
            {
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                }
            }
            
            if (currentLine.Length > 0)
                currentLine.Append(" ");
            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString());

        return string.Join(Environment.NewLine, lines);
    }
}
```

#### Use Case Explanation
- Provides real-time feedback on streaming progress
- Useful for debugging and performance monitoring
- Enhances user experience with visual indicators

## Web API Streaming

### Server-Sent Events Implementation

Implement streaming responses in ASP.NET Core using Server-Sent Events:

```csharp
using Microsoft.AspNetCore.Mvc;
using OpenRouter.Core;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class StreamingChatController : ControllerBase
{
    private readonly IOpenRouterClient _openRouterClient;
    private readonly ILogger<StreamingChatController> _logger;

    public StreamingChatController(
        IOpenRouterClient openRouterClient, 
        ILogger<StreamingChatController> logger)
    {
        _openRouterClient = openRouterClient;
        _logger = logger;
    }

    [HttpPost("stream")]
    public async Task StreamChatCompletion([FromBody] StreamChatRequest request)
    {
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        Response.Headers.Add("Access-Control-Allow-Origin", "*");

        try
        {
            _logger.LogInformation("Starting streaming chat completion");

            var streamingRequest = _openRouterClient.Chat
                .CreateStreamingChatCompletion(request.Model ?? "gpt-3.5-turbo")
                .AddUserMessage(request.Message)
                .WithTemperature(request.Temperature ?? 0.7)
                .WithMaxTokens(request.MaxTokens ?? 500);

            if (!string.IsNullOrEmpty(request.SystemMessage))
            {
                streamingRequest = streamingRequest.AddSystemMessage(request.SystemMessage);
            }

            await foreach (var chunk in streamingRequest.StreamAsync())
            {
                if (HttpContext.RequestAborted.IsCancellationRequested)
                {
                    _logger.LogInformation("Client disconnected, stopping stream");
                    break;
                }

                if (chunk.Choices?.Any() == true)
                {
                    var choice = chunk.Choices[0];
                    var deltaContent = choice.Delta?.Content;

                    if (!string.IsNullOrEmpty(deltaContent))
                    {
                        var sseData = new
                        {
                            type = "content",
                            content = deltaContent,
                            timestamp = DateTimeOffset.UtcNow
                        };

                        await WriteSSEAsync("data", JsonSerializer.Serialize(sseData));
                    }

                    // Send completion event
                    if (choice.FinishReason != null)
                    {
                        var completionData = new
                        {
                            type = "completion",
                            finishReason = choice.FinishReason,
                            timestamp = DateTimeOffset.UtcNow
                        };

                        await WriteSSEAsync("data", JsonSerializer.Serialize(completionData));
                        break;
                    }
                }
            }

            await WriteSSEAsync("event", "stream-end");
            _logger.LogInformation("Streaming completed successfully");
        }
        catch (OpenRouterException ex)
        {
            _logger.LogError(ex, "OpenRouter API error during streaming");
            await WriteSSEAsync("error", JsonSerializer.Serialize(new { error = ex.Message, type = ex.ErrorType }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during streaming");
            await WriteSSEAsync("error", JsonSerializer.Serialize(new { error = "Internal server error" }));
        }
    }

    private async Task WriteSSEAsync(string eventType, string data)
    {
        await Response.WriteAsync($"{eventType}: {data}\n\n");
        await Response.Body.FlushAsync();
    }
}

public class StreamChatRequest
{
    public required string Message { get; set; }
    public string? Model { get; set; }
    public string? SystemMessage { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
}
```

#### Client-Side JavaScript Implementation

```javascript
// Frontend JavaScript for consuming the streaming API
class StreamingChatClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    async streamChat(message, options = {}) {
        const requestBody = {
            message: message,
            model: options.model || 'gpt-3.5-turbo',
            systemMessage: options.systemMessage,
            temperature: options.temperature || 0.7,
            maxTokens: options.maxTokens || 500
        };

        const response = await fetch(`${this.baseUrl}/api/streamingchat/stream`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(requestBody)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();

        return {
            async *[Symbol.asyncIterator]() {
                try {
                    while (true) {
                        const { done, value } = await reader.read();
                        
                        if (done) break;
                        
                        const chunk = decoder.decode(value, { stream: true });
                        const lines = chunk.split('\n');
                        
                        for (const line of lines) {
                            if (line.startsWith('data: ')) {
                                try {
                                    const data = JSON.parse(line.slice(6));
                                    yield data;
                                    
                                    if (data.type === 'completion') {
                                        return;
                                    }
                                } catch (e) {
                                    console.warn('Failed to parse SSE data:', line);
                                }
                            } else if (line.startsWith('error: ')) {
                                throw new Error(line.slice(7));
                            } else if (line === 'event: stream-end') {
                                return;
                            }
                        }
                    }
                } finally {
                    reader.releaseLock();
                }
            }
        };
    }
}

// Usage example
const client = new StreamingChatClient('https://localhost:5001');

async function displayStreamingResponse(message) {
    const responseDiv = document.getElementById('response');
    responseDiv.innerHTML = '';
    
    try {
        const stream = await client.streamChat(message);
        
        for await (const chunk of stream) {
            if (chunk.type === 'content') {
                responseDiv.innerHTML += chunk.content;
            } else if (chunk.type === 'completion') {
                console.log('Stream completed:', chunk.finishReason);
            }
        }
    } catch (error) {
        responseDiv.innerHTML = `Error: ${error.message}`;
    }
}
```

#### Use Case Explanation
- Provides real-time updates in web applications
- Essential for responsive chat interfaces
- Enables progressive content display

#### Performance Considerations
- SSE connections consume server resources per client
- Implement connection limits and timeouts
- Consider using SignalR for bi-directional communication

## Blazor Integration

### Real-time UI Updates

Implement streaming in Blazor Server applications with real-time UI updates:

<!-- C# Code Example: Blazor Server streaming implementation with StateHasChanged and real-time UI updates -->

```csharp
@page "/streaming-chat"
@using OpenRouter.Core
@inject IOpenRouterClient OpenRouterClient
@inject IJSRuntime JSRuntime
@implements IDisposable

<PageTitle>Streaming Chat</PageTitle>

<div class="container mt-4">
    <div class="row">
        <div class="col-12">
            <h3>AI Streaming Chat</h3>
            
            <div class="card mb-3">
                <div class="card-body">
                    <div class="form-group mb-3">
                        <label for="messageInput">Your Message:</label>
                        <textarea @bind="currentMessage" 
                                 @onkeypress="HandleKeyPress"
                                 class="form-control" 
                                 id="messageInput" 
                                 rows="3" 
                                 disabled="@isStreaming"
                                 placeholder="Type your message here..."></textarea>
                    </div>
                    
                    <div class="form-group mb-3">
                        <label for="modelSelect">Model:</label>
                        <select @bind="selectedModel" class="form-control" id="modelSelect" disabled="@isStreaming">
                            <option value="gpt-3.5-turbo">GPT-3.5 Turbo</option>
                            <option value="anthropic/claude-3-haiku">Claude 3 Haiku</option>
                            <option value="meta-llama/llama-2-70b-chat">Llama 2 70B</option>
                        </select>
                    </div>
                    
                    <button @onclick="SendMessageAsync" 
                           class="btn btn-primary" 
                           disabled="@(isStreaming || string.IsNullOrWhiteSpace(currentMessage))">
                        @if (isStreaming)
                        {
                            <span class="spinner-border spinner-border-sm me-2"></span>
                            Thinking...
                        }
                        else
                        {
                            <i class="fas fa-paper-plane me-2"></i>
                            Send Message
                        }
                    </button>
                    
                    @if (isStreaming)
                    {
                        <button @onclick="CancelStream" class="btn btn-secondary ms-2">
                            <i class="fas fa-stop me-2"></i>
                            Cancel
                        </button>
                    }
                </div>
            </div>

            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">AI Response</h5>
                    @if (!string.IsNullOrEmpty(streamingResponse))
                    {
                        <small class="text-muted">
                            @($"{streamingResponse.Length} characters • {streamingStats.ChunksReceived} chunks • {streamingStats.ElapsedTime.TotalSeconds:F1}s")
                        </small>
                    }
                </div>
                <div class="card-body">
                    @if (string.IsNullOrEmpty(streamingResponse) && !isStreaming)
                    {
                        <p class="text-muted">AI response will appear here...</p>
                    }
                    else
                    {
                        <div class="streaming-response">
                            @((MarkupString)FormatResponse(streamingResponse))
                            @if (isStreaming)
                            {
                                <span class="streaming-cursor">|</span>
                            }
                        </div>
                    }
                </div>
            </div>

            @if (conversationHistory.Any())
            {
                <div class="card mt-3">
                    <div class="card-header">
                        <h5 class="mb-0">Conversation History</h5>
                        <small class="text-muted">@conversationHistory.Count exchanges</small>
                    </div>
                    <div class="card-body">
                        @foreach (var exchange in conversationHistory.TakeLast(3))
                        {
                            <div class="mb-3">
                                <strong>You:</strong> @exchange.UserMessage<br />
                                <strong>AI:</strong> @exchange.AiResponse.Substring(0, Math.Min(exchange.AiResponse.Length, 100))@(exchange.AiResponse.Length > 100 ? "..." : "")
                            </div>
                        }
                    </div>
                </div>
            }
        </div>
    </div>
</div>

@code {
    private string currentMessage = string.Empty;
    private string selectedModel = "gpt-3.5-turbo";
    private string streamingResponse = string.Empty;
    private bool isStreaming = false;
    private List<ConversationExchange> conversationHistory = new();
    private CancellationTokenSource? cancellationTokenSource;
    private StreamingStats streamingStats = new();

    private class ConversationExchange
    {
        public required string UserMessage { get; set; }
        public required string AiResponse { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    private class StreamingStats
    {
        public int ChunksReceived { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan ElapsedTime => DateTime.Now - StartTime;
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(currentMessage) || isStreaming)
            return;

        var userMessage = currentMessage.Trim();
        currentMessage = string.Empty;
        streamingResponse = string.Empty;
        isStreaming = true;
        streamingStats = new StreamingStats { StartTime = DateTime.Now };
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var streamingRequest = OpenRouterClient.Chat
                .CreateStreamingChatCompletion(selectedModel)
                .AddUserMessage(userMessage)
                .WithTemperature(0.7);

            await foreach (var chunk in streamingRequest.StreamAsync(cancellationTokenSource.Token))
            {
                if (chunk.Choices?.Any() == true)
                {
                    var deltaContent = chunk.Choices[0].Delta?.Content;
                    if (!string.IsNullOrEmpty(deltaContent))
                    {
                        streamingResponse += deltaContent;
                        streamingStats.ChunksReceived++;
                        
                        // Update UI every few characters for smooth animation
                        if (streamingStats.ChunksReceived % 3 == 0)
                        {
                            await InvokeAsync(StateHasChanged);
                            await Task.Delay(10); // Small delay for visual effect
                        }
                    }

                    if (chunk.Choices[0].FinishReason != null)
                    {
                        break;
                    }
                }
            }

            // Add to conversation history
            conversationHistory.Add(new ConversationExchange
            {
                UserMessage = userMessage,
                AiResponse = streamingResponse
            });
        }
        catch (OperationCanceledException)
        {
            streamingResponse += "\n\n[Response cancelled by user]";
        }
        catch (Exception ex)
        {
            streamingResponse = $"Error: {ex.Message}";
        }
        finally
        {
            isStreaming = false;
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task CancelStream()
    {
        cancellationTokenSource?.Cancel();
        await Task.Delay(100); // Give time for cancellation to propagate
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && e.CtrlKey && !isStreaming)
        {
            await SendMessageAsync();
        }
    }

    private string FormatResponse(string response)
    {
        // Simple markdown-like formatting
        return response
            .Replace("\n", "<br/>")
            .Replace("**", "<strong>", StringComparison.OrdinalIgnoreCase)
            .Replace("**", "</strong>", StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }
}

<style>
    .streaming-response {
        min-height: 100px;
        white-space: pre-wrap;
        word-wrap: break-word;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        line-height: 1.5;
    }

    .streaming-cursor {
        animation: blink 1s infinite;
        font-weight: bold;
    }

    @@keyframes blink {
        0%, 50% { opacity: 1; }
        51%, 100% { opacity: 0; }
    }

    .card {
        border: 1px solid #dee2e6;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .btn:disabled {
        opacity: 0.6;
    }
</style>
```

#### Use Case Explanation
- Provides real-time streaming in Blazor Server applications
- Includes conversation history and cancellation support
- Offers visual feedback with streaming cursor animation

#### Performance Considerations
- Frequent StateHasChanged calls can impact performance
- Consider batching updates for very fast streams
- Monitor SignalR connection stability for Blazor Server

## Background Processing

### Long-running Stream Handling

Handle streaming responses in background services for processing and storage:

<!-- C# Code Example: Background service for processing streaming responses with queuing and persistence -->

## Error Recovery

### Stream Interruption Handling

Robust error handling and recovery for interrupted streams:

<!-- C# Code Example: Stream interruption detection, automatic retry, and graceful degradation -->

## Performance Optimization

### Memory-efficient Streaming Patterns

Optimize memory usage and performance for high-volume streaming scenarios:

<!-- C# Code Example: Memory-efficient streaming with buffering, backpressure handling, and resource cleanup -->

---

**Next Steps:**
- [Integration Examples →](integration-examples.md) - Platform-specific implementations
- [Troubleshooting →](../troubleshooting.md) - Common streaming issues and solutions
- [API Reference →](../api-reference/) - Detailed API documentation