# Feature: Chat Completions

The OpenRouter.NET SDK provides robust support for chat completions, enabling rich conversational AI experiences. This includes standard (blocking) chat calls, streaming responses for real-time interaction, and tools for managing conversation history.

The primary service for this functionality is [`IChatService`](../../OpenRouter/Services/Chat/IChatService.cs:1), accessed via `openRouterClient.Chat`.

## Basic Chat Completion

A basic chat completion involves sending a request with a series of messages and receiving the model's full response once it's generated.

**Steps:**
1.  Create a [`ChatCompletionRequest`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:1).
2.  Specify the `Model` ID.
3.  Provide a list of [`Message`](../../OpenRouter/Models/Common/Message.cs:1) objects representing the conversation.
4.  Call `await _chatService.CreateChatCompletionAsync(request)`.
5.  Process the [`ChatCompletionResponse`](../../OpenRouter/Models/Responses/ChatCompletionResponse.cs:1).

**Example:**
(Adapted from [`BasicChatExample.cs`](../../OpenRouter.Examples/BasicUsage/BasicChatExample.cs:1))

```csharp
using OpenRouter.Services.Chat;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Common;
using Microsoft.Extensions.Logging; // For ILogger

public class ChatExample
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatExample> _logger;

    public ChatExample(IChatService chatService, ILogger<ChatExample> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task RunBasicChatAsync()
    {
        _logger.LogInformation("--- Basic Chat Example ---");
        var request = new ChatCompletionRequest
        {
            Model = "openai/gpt-3.5-turbo", // Or any preferred model
            Messages = new List<Message>
            {
                new Message { Role = "system", Content = "You are a helpful assistant." },
                new Message { Role = "user", Content = "What is the weather like in London today?" }
            },
            MaxTokens = 150
        };

        try
        {
            var response = await _chatService.CreateChatCompletionAsync(request);
            var firstChoice = response.Choices?.FirstOrDefault();
            if (firstChoice != null)
            {
                _logger.LogInformation("Model Response: {Content}", firstChoice.Message?.Content);
                _logger.LogInformation("Finish Reason: {Reason}", firstChoice.FinishReason);
            }
            else
            {
                _logger.LogWarning("No response choices received.");
            }
            LogUsage(response.Usage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during basic chat completion");
        }
    }

    private void LogUsage(Usage? usage)
    {
        if (usage != null)
        {
            _logger.LogInformation("Usage: Prompt Tokens={Prompt}, Completion Tokens={Completion}, Total Tokens={Total}",
                usage.PromptTokens, usage.CompletionTokens, usage.TotalTokens);
        }
    }
}
```

## Streaming Chat Completions

Streaming allows you to receive the model's response in chunks as it's being generated. This is ideal for user interfaces where you want to display text progressively.

**Steps:**
1.  Create a `ChatCompletionRequest`, ensuring `Stream = true` (though the SDK's `StreamChatCompletionAsync` method typically handles setting this).
2.  Define an `Action<ChatCompletionChunk>` to handle each incoming [`ChatCompletionChunk`](../../OpenRouter/Models/Responses/ChatCompletionChunk.cs:1).
3.  Call `await _chatService.StreamChatCompletionAsync(request, chunkHandler)`.

**Example:**
(Adapted from [`BasicChatExample.cs`](../../OpenRouter.Examples/BasicUsage/BasicChatExample.cs:1))

```csharp
public async Task RunStreamingChatAsync()
{
    _logger.LogInformation("--- Streaming Chat Example ---");
    var request = new ChatCompletionRequest
    {
        Model = "mistralai/mistral-7b-instruct",
        Messages = new List<Message>
        {
            new Message { Role = "user", Content = "Write a short poem about AI." }
        },
        MaxTokens = 100,
        // Stream property defaults to true when using StreamChatCompletionAsync internally
    };

    var fullResponse = new System.Text.StringBuilder();
    _logger.LogInformation("Streaming Response:");

    try
    {
        await _chatService.StreamChatCompletionAsync(request, chunk =>
        {
            var content = chunk.Choices?.FirstOrDefault()?.Delta?.Content;
            if (!string.IsNullOrEmpty(content))
            {
                Console.Write(content); // Write directly to console for immediate display
                fullResponse.Append(content);
            }

            if (chunk.Choices?.FirstOrDefault()?.FinishReason != null)
            {
                _logger.LogInformation("\nStream finished. Finish Reason: {Reason}", chunk.Choices.First().FinishReason);
                LogUsage(chunk.Usage); // Usage info usually comes with the last chunk
            }
        });
        _logger.LogInformation("\n--- End of Stream ---");
        _logger.LogInformation("Full assembled response: {FullResponse}", fullResponse.ToString());
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during streaming chat completion");
    }
}
```
Note: The `Console.Write` in the chunk handler is for demonstration. In a UI application, you'd append to a display buffer.

## Conversation Management

Maintaining conversation history is crucial for context. The `Messages` list in `ChatCompletionRequest` serves this purpose. You typically append the user's new message and the assistant's last response to this list before making the next request.

**Example:**
(Adapted from [`BasicChatExample.cs`](../../OpenRouter.Examples/BasicUsage/BasicChatExample.cs:1))
```csharp
public async Task RunConversationAsync()
{
    _logger.LogInformation("--- Conversation Example ---");
    var conversationHistory = new List<Message>
    {
        new Message { Role = "system", Content = "You are a witty and slightly sarcastic assistant." }
    };

    await ProcessUserMessage("Hello there!", conversationHistory);
    await ProcessUserMessage("What's your name?", conversationHistory);
    await ProcessUserMessage("Can you tell me a fun fact?", conversationHistory);
}

private async Task ProcessUserMessage(string userInput, List<Message> history)
{
    _logger.LogInformation("User: {Input}", userInput);
    history.Add(new Message { Role = "user", Content = userInput });

    var request = new ChatCompletionRequest
    {
        Model = "nousresearch/nous-hermes-2-mixtral-8x7b-dpo",
        Messages = history, // Pass the updated history
        MaxTokens = 150
    };

    try
    {
        var response = await _chatService.CreateChatCompletionAsync(request);
        var assistantResponse = response.Choices?.FirstOrDefault()?.Message;

        if (assistantResponse != null)
        {
            _logger.LogInformation("Assistant: {Content}", assistantResponse.Content);
            history.Add(assistantResponse); // Add assistant's response to history
            LogUsage(response.Usage);
        }
        else
        {
            _logger.LogWarning("No response from assistant.");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during conversation turn");
    }
}
```

## Using `ChatRequestBuilder`
The SDK includes a fluent [`IChatRequestBuilder`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:1) to help construct `ChatCompletionRequest` objects in a more readable way.

```csharp
var request = _chatService.CreateChatCompletionRequestBuilder()
    .WithModel("openai/gpt-4o")
    .AddSystemMessage("You are a travel planner.")
    .AddUserMessage("Suggest a 3-day itinerary for Paris.")
    .WithMaxTokens(500)
    .WithTemperature(0.7)
    // .WithTools(myToolsList) // If using tools
    .Build();

// var response = await _chatService.CreateChatCompletionAsync(request);
```

## Key Parameters in `ChatCompletionRequest`

*   `Model` (string, required): The ID of the model to use.
*   `Messages` (List<`Message`>, required): The conversation history.
*   `MaxTokens` (int?): The maximum number of tokens to generate in the chat completion.
*   `Temperature` (double?): Controls randomness. Lower values (~0.2) make output more deterministic, higher values (~1.0) make it more random.
*   `TopP` (double?): Nucleus sampling. Consider alternatives like temperature.
*   `Stream` (bool?): Set to `true` for streaming (handled by `StreamChatCompletionAsync`).
*   `Stop` (string or List<string>?): Sequences where the API will stop generating further tokens.
*   `PresencePenalty`, `FrequencyPenalty` (double?): Penalize new tokens based on their existing presence or frequency.
*   `Tools` (List<[`Tool`](../../OpenRouter/Models/Common/Tool.cs:1)>?): A list of tools the model may call. See [Tool Use / Function Calling](#) (future section or OpenRouter docs).
*   `ToolChoice` (object?): Controls which tool the model is forced to call, or if any.
*   `RouteConfig` (object?): For advanced routing or model-specific parameters if supported by OpenRouter's passthrough capabilities.
*   `Transforms` (List<string>): Apply content transformations (e.g., "middle-out").

Refer to the [`ChatCompletionRequest`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:1) class and the official OpenRouter API documentation for a complete list and detailed explanations of all parameters. The [`OpenRouter.Examples`](../../OpenRouter.Examples/) project demonstrates usage of some of these, like custom parameters.

## Next Steps
*   [Managing Models](model-management.md)
*   [Authentication](authentication.md)
*   [Error Handling](error-handling.md)