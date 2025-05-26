# Core Concepts: Available Services

The `IOpenRouterClient` provides access to various services, each catering to a specific set of OpenRouter API functionalities. These services are accessible as properties on the `IOpenRouterClient` instance.

This design organizes API interactions into logical groups, making the SDK easier to understand and use.

## Main Services

Here are the primary services offered by the SDK:

### 1. Chat Service (`IChatService`)

*   **Interface**: [`OpenRouter.Services.Chat.IChatService`](../../OpenRouter/Services/Chat/IChatService.cs:1)
*   **Implementation**: [`OpenRouter.Services.Chat.ChatService`](../../OpenRouter/Services/Chat/ChatService.cs:1)
*   **Accessed via**: `client.Chat`

The `ChatService` is responsible for all interactions related to chat completions. This is likely the most frequently used service.

**Key Functionalities:**
*   **Create Chat Completion**: Sends a request to get a model's response to a series of messages. Supports both standard and streaming responses.
    *   `CreateChatCompletionAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)`
    *   `StreamChatCompletionAsync(ChatCompletionRequest request, Action<ChatCompletionChunk> chunkHandler, CancellationToken cancellationToken = default)`
*   **Chat Request Builder**: Provides a fluent API to construct `ChatCompletionRequest` objects.
    *   `CreateChatCompletionRequestBuilder()` returns an [`IChatRequestBuilder`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:1).

See [Chat Completions Feature Guide](features/chat-completions.md) for detailed usage.

### 2. Models Service (`IModelsService`)

*   **Interface**: [`OpenRouter.Services.Models.IModelsService`](../../OpenRouter/Services/Models/IModelsService.cs:1)
*   **Implementation**: [`OpenRouter.Services.Models.ModelsService`](../../OpenRouter/Services/Models/ModelsService.cs:1)
*   **Accessed via**: `client.Models`

The `ModelsService` allows you to retrieve information about the AI models available through OpenRouter.

**Key Functionalities:**
*   **Get Models**: Fetches a list of all available models, or models updated after a certain generation number.
    *   `GetModelsAsync(CancellationToken cancellationToken = default)`
    *   Can also be called directly from `client.GetModelsAsync()`.
*   **Get Model Details**: Retrieves detailed information about a specific model by its ID. (This functionality might be implicitly part of `GetModelsAsync` or might require a dedicated method if the API supports fetching single model details not covered by the general list - review API docs for specific method if needed). The current SDK structure suggests model details are part of the `ModelResponse` objects returned by `GetModelsAsync`.

See [Model Management Feature Guide](features/model-management.md) for detailed usage.

### 3. Authentication Service (`IAuthService`)
*   **Interface**: [`OpenRouter.Services.Auth.IAuthService`](../../OpenRouter/Services/Auth/IAuthService.cs:1)
*   **Implementation**: [`OpenRouter.Services.Auth.AuthService`](../../OpenRouter/Services/Auth/AuthService.cs:1)
*   **Accessed via**: `client.Auth`

The `AuthService` deals with authentication-related operations. While API key authentication is handled transparently by the client for most requests, this service might be used for specific authentication schemes or operations if supported by the OpenRouter API (e.g., token validation, key generation/management if the API exposed such endpoints).

**Note**: The current primary authentication method (API Key in header) is handled by [`ApiKeyProvider`](../../OpenRouter/Authentication/ApiKeyProvider.cs:1) and configured via `OpenRouterOptions`. The `AuthService` might be for more advanced or alternative OpenRouter auth features.

### 4. Credits Service (`ICreditsService`)

*   **Interface**: [`OpenRouter.Services.Credits.ICreditsService`](../../OpenRouter/Services/Credits/ICreditsService.cs:1)
*   **Implementation**: [`OpenRouter.Services.Credits.CreditsService`](../../OpenRouter/Services/Credits/CreditsService.cs:1)
*   **Accessed via**: `client.Credits`

The `CreditsService` is used to check the remaining credits or usage status for your OpenRouter account.

**Key Functionalities:**
*   **Get Credits**: Fetches the current credit status.
    *   `GetCreditsAsync(CancellationToken cancellationToken = default)`
    *   Can also be called directly from `client.GetCreditsAsync()`.

### 5. Keys Service (`IKeysService`)

*   **Interface**: [`OpenRouter.Services.Keys.IKeysService`](../../OpenRouter/Services/Keys/IKeysService.cs:1)
*   **Implementation**: [`OpenRouter.Services.Keys.KeysService`](../../OpenRouter/Services/Keys/KeysService.cs:1)
*   **Accessed via**: `client.Keys`

The `KeysService` would be used for operations related to managing API keys Programmatically, if the OpenRouter API provides such endpoints (e.g., creating, listing, deleting API keys).

**Note**: This is distinct from *using* an API key for authentication. It's about *managing* the keys themselves.

### 6. Generation Service (`IGenerationService`)
*   **Interface**: [`OpenRouter.Services.Generation.IGenerationService`](../../OpenRouter/Services/Generation/IGenerationService.cs:1)
*   **Implementation**: [`OpenRouter.Services.Generation.GenerationService`](../../OpenRouter/Services/Generation/GenerationService.cs:1)
*   **Accessed via**: `client.Generation` (or specific methods on client)

The `GenerationService` appears to be related to retrieving details about specific generations or inference runs. The main use case seems to be fetching generation details, often referenced after a chat completion.

**Key Functionalities:**
*   **Get Generation Details**: Fetches details for a specific generation ID.
    *   `GetGenerationDetailsAsync(string generationId, CancellationToken cancellationToken = default)`
    *   This is often used to get more information about a response, especially if a `generation_id` is returned in a chat completion response.

## Using Services

To use a service, you first need an instance of `IOpenRouterClient` (usually obtained through dependency injection). Then, you can access the desired service as a property:

```csharp
public class MyLogic
{
    private readonly IOpenRouterClient _openRouterClient;

    public MyLogic(IOpenRouterClient openRouterClient)
    {
        _openRouterClient = openRouterClient;
    }

    public async Task ListModelsAndChat()
    {
        // Using the Models service
        var modelsResponse = await _openRouterClient.Models.GetModelsAsync();
        if (modelsResponse?.Data != null)
        {
            foreach (var model in modelsResponse.Data)
            {
                Console.WriteLine($"Model ID: {model.Id}, Name: {model.Name}");
            }
        }

        // Using the Chat service
        var chatRequest = new OpenRouter.Models.Requests.ChatCompletionRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new List<OpenRouter.Models.Common.Message>
            {
                new OpenRouter.Models.Common.Message { Role = "user", Content = "Tell me a joke." }
            }
        };
        var chatResponse = await _openRouterClient.Chat.CreateChatCompletionAsync(chatRequest);
        Console.WriteLine($"Joke: {chatResponse.Choices?.FirstOrDefault()?.Message?.Content}");
    }
}
```

Each service encapsulates the logic for its domain, including constructing requests, sending them via the shared `HttpClient`, and deserializing responses.

## Next Steps
*   [Request and Response Models](data-models.md)
*   Explore feature-specific guides like [Chat Completions](features/chat-completions.md) and [Model Management](features/model-management.md).