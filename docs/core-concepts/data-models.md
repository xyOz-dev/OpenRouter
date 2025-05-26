# Core Concepts: Request and Response Models

The OpenRouter.NET SDK utilizes a set of strongly-typed C# classes to represent data sent to (requests) and received from (responses) the OpenRouter API. These models ensure type safety and make it easier to work with API data.

All models are located under the [`OpenRouter.Models`](../../OpenRouter/Models/) namespace, categorized into `Requests` and `Responses` sub-namespaces, and a `Common` sub-namespace for shared structures.

## Request Models

Request models are used to structure the data you send to the API. Each service operation typically has a corresponding request model.

**Key Request Models:**

*   **[`ChatCompletionRequest`](../../OpenRouter/Models/Requests/ChatCompletionRequest.cs:1)**:
    *   Used with `IChatService.CreateChatCompletionAsync()` and `IChatService.StreamChatCompletionAsync()`.
    *   **Core Properties**:
        *   `Model` (string): ID of the model to use (e.g., "openai/gpt-3.5-turbo").
        *   `Messages` (List<[`Message`](../../OpenRouter/Models/Common/Message.cs:1)>): A list of message objects describing the conversation history.
        *   `Stream` (bool?): Whether to stream back partial progress.
        *   `MaxTokens` (int?): Maximum number of tokens to generate.
        *   `Temperature` (double?): Sampling temperature.
        *   `TopP` (double?): Nucleus sampling parameter.
        *   `Tools` (List<[`Tool`](../../OpenRouter/Models/Common/Tool.cs:1)>?): A list of tools the model may call.
        *   ...and other model-specific parameters (often via `Transforms` or `RouteConfig`).
    *   The [`ChatRequestBuilder`](../../OpenRouter/Services/Chat/ChatRequestBuilder.cs:1) provides a fluent way to construct this request.

*   **[`CompletionRequest`](../../OpenRouter/Models/Requests/CompletionRequest.cs:1)**:
    *   (Potentially for older/legacy completion endpoints, if OpenRouter still supports them separately from chat. The SDK structure points primarily to chat completions.)
    *   **Core Properties**:
        *   `Model` (string)
        *   `Prompt` (string)
        *   ...other completion parameters.

*   **[`ModelsRequest`](../../OpenRouter/Models/Requests/ModelsRequest.cs:1)**:
    *   Used implicitly by `IModelsService.GetModelsAsync()`. Often, this request model might be minimal or not directly exposed if parameters are passed directly to the service method (e.g., a generation ID for filtering).

*   **[`ModelDetailsRequest`](../../OpenRouter/Models/Requests/ModelDetailsRequest.cs:1)**: (Potentially for fetching a single model's details, if different from the list endpoint.)

*   **[`AuthRequest`](../../OpenRouter/Models/Requests/AuthRequest.cs:1)**, **[`CreditsRequest`](../../OpenRouter/Models/Requests/CreditsRequest.cs:1)**, **[`KeysRequest`](../../OpenRouter/Models/Requests/KeysRequest.cs:1)**:
    *   These are likely minimal or empty if the operations they correspond to (e.g., getting credits) don't require a request body beyond URL parameters or headers handled by the client.

**Building Requests:**
You typically instantiate these request objects and set their properties before passing them to a service method.

```csharp
var chatRequest = new ChatCompletionRequest
{
    Model = "mistralai/mistral-7b-instruct",
    Messages = new List<Message>
    {
        new Message { Role = "user", Content = "What is the capital of France?" }
    },
    MaxTokens = 50,
    Stream = false // For a non-streaming request
};

// Then pass to: await chatService.CreateChatCompletionAsync(chatRequest);
```

## Response Models

Response models structure the data returned by the API.

**Key Response Models:**

*   **[`ChatCompletionResponse`](../../OpenRouter/Models/Responses/ChatCompletionResponse.cs:1)**:
    *   Returned by `IChatService.CreateChatCompletionAsync()`.
    *   **Core Properties**:
        *   `Id` (string): A unique identifier for the chat completion.
        *   `Choices` (List<`ChatChoice`>): A list of chat completion choices (usually one).
            *   Each `ChatChoice` contains:
                *   `Message` ([`Message`](../../OpenRouter/Models/Common/Message.cs:1)): The model-generated message.
                *   `FinishReason` (string): e.g., "stop", "length", "tool_calls".
        *   `Usage` ([`Usage`](../../OpenRouter/Models/Common/Usage.cs:1)): Token usage statistics.
        *   `Model` (string): The model used for the completion.
        *   `Created` (long?): Timestamp of creation.
        *   `SystemFingerprint` (string?): System fingerprint.

*   **[`ChatCompletionChunk`](../../OpenRouter/Models/Responses/ChatCompletionChunk.cs:1)**:
    *   Received by the `chunkHandler` in `IChatService.StreamChatCompletionAsync()`. Represents a piece of a streaming response.
    *   **Core Properties**:
        *   `Id` (string)
        *   `Choices` (List<`StreamingChatChoice`>): Each choice contains a `Delta` ([`Message`](../../OpenRouter/Models/Common/Message.cs:1)) with the new token(s) and potentially `FinishReason`.
        *   `Usage` ([`Usage`](../../OpenRouter/Models/Common/Usage.cs:1)) (typically sent with the final chunk).

*   **[`ModelResponse`](../../OpenRouter/Models/Responses/ModelResponse.cs:1)**: (Or a wrapper like `ModelsListResponse` containing `List<ModelResponse>`)
    *   Returned by `IModelsService.GetModelsAsync()`. Represents information about a single AI model.
    *   **Core Properties**:
        *   `Id` (string): Model ID.
        *   `Name` (string): Human-readable name.
        *   `Description` (string): Model description.
        *   `Pricing` (object): Pricing details (e.g., prompt token cost, completion token cost).
        *   `ContextLength` (int?): Maximum context length.
        *   `Architecture` (object): Model architecture details.
        *   ...and other metadata.

*   **[`CompletionResponse`](../../OpenRouter/Models/Responses/CompletionResponse.cs:1)** and **[`CompletionChunk`](../../OpenRouter/Models/Responses/CompletionChunk.cs:1)**: For legacy completion endpoints.

*   **[`CreditsResponse`](../../OpenRouter/Models/Responses/CreditsResponse.cs:1)**:
    *   Returned by `ICreditsService.GetCreditsAsync()`.
    *   Properties detailing credit usage, remaining balance, etc.

*   **[`GenerationDetailsResponse`](../../OpenRouter/Models/Responses/GenerationDetailsResponse.cs:1)**:
    *   Returned by `IGenerationService.GetGenerationDetailsAsync()`.
    *   Properties detailing a specific inference generation.

*   **[`ErrorResponse`](../../OpenRouter/Models/Responses/ErrorResponse.cs:1)**:
    *   If an API call fails, the SDK may attempt to deserialize the error into this model. This is typically handled within an [`OpenRouterApiException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:1).
    *   **Core Properties**: `Error` (object) containing `Message`, `Type`, `Code`, etc.

## Common Models

These models are often nested within various request and response objects.

*   **[`Message`](../../OpenRouter/Models/Common/Message.cs:1)**:
    *   Represents a single message in a conversation.
    *   **Core Properties**:
        *   `Role` (string): "system", "user", "assistant", or "tool".
        *   `Content` (string | List<`ContentPart`>): Text content or multimodal content parts.
        *   `ToolCalls` (List<`ToolCall`>?): For assistant messages requesting tool usage.
        *   `ToolCallId` (string?): For tool messages, the ID of the tool call being responded to.

*   **[`Usage`](../../OpenRouter/Models/Common/Usage.cs:1)**:
    *   Provides token usage information for a request.
    *   **Core Properties**:
        *   `PromptTokens` (int?)
        *   `CompletionTokens` (int?)
        *   `TotalTokens` (int?)

*   **[`Tool`](../../OpenRouter/Models/Common/Tool.cs:1)** & `ToolCall`:
    *   Define functions/tools the model can interact with.
    *   `Tool`: Describes the tool's schema (name, description, parameters).
    *   `ToolCall`: Represents a model's request to invoke a specific tool with arguments.

## Exploring Models

The best way to understand the exact structure of these models is to navigate to their definitions in the source code (links provided above). The SDK is designed to map closely to the JSON structures defined by the OpenRouter API. Refer to the official OpenRouter API documentation for the canonical schema definitions.

These models provide the contract for how your .NET application interacts with the OpenRouter API, making it easier to build robust and maintainable integrations.

## Next Steps
*   Explore feature-specific guides which show these models in action:
    *   [Chat Completions](features/chat-completions.md)
    *   [Model Management](features/model-management.md)