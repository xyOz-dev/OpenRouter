using Microsoft.Extensions.Logging;
using OpenRouter.Core;
using OpenRouter.Http;
using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;

namespace OpenRouter.Services.Chat;

public class ChatService : IChatService
{
    private readonly IHttpClientProvider _httpClient;
    private readonly ILogger<ChatService>? _logger;

    public ChatService(IHttpClientProvider httpClient, ILogger<ChatService>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger;
    }

    public async Task<ChatCompletionResponse> CreateAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        
        ValidateRequest(request);
        
        _logger?.LogDebug("Creating chat completion for model {Model}", request.Model);
        
        var response = await _httpClient.SendAsync<ChatCompletionResponse>(
            Constants.Endpoints.ChatCompletions, 
            request, 
            cancellationToken);
            
        _logger?.LogDebug("Chat completion created successfully with ID {Id}", response.Id);
        
        return response;
    }

    public async IAsyncEnumerable<ChatCompletionChunk> CreateStreamAsync(ChatCompletionRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        
        ValidateRequest(request);
        
        // Ensure streaming is enabled
        request.Stream = true;
        
        _logger?.LogDebug("Creating streaming chat completion for model {Model}", request.Model);
        
        await foreach (var chunk in _httpClient.StreamAsync<ChatCompletionChunk>(
            Constants.Endpoints.ChatCompletions, 
            request, 
            cancellationToken))
        {
            yield return chunk;
        }
        
        _logger?.LogDebug("Streaming chat completion completed");
    }

    public async Task<CompletionResponse> CreateCompletionAsync(CompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Basic validation for CompletionRequest
        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ArgumentException("Model is required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt is required", nameof(request));
        
        _logger?.LogDebug("Creating text completion for model {Model}", request.Model);

        var response = await _httpClient.SendAsync<CompletionResponse>(
            Constants.Endpoints.Completions,
            request,
            cancellationToken);
            
        _logger?.LogDebug("Text completion created successfully with ID {Id}", response.Id);
        
        return response;
    }

    public async IAsyncEnumerable<CompletionChunk> CreateCompletionStreamAsync(CompletionRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Basic validation for CompletionRequest
        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ArgumentException("Model is required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt is required", nameof(request));

        // Ensure streaming is enabled for this call
        // Note: CompletionRequest model does not have a Stream property,
        // but the HTTP client handles the streaming behavior based on the return type and endpoint.
        
        _logger?.LogDebug("Creating streaming text completion for model {Model}", request.Model);

        await foreach (var chunk in _httpClient.StreamAsync<CompletionChunk>(
            Constants.Endpoints.Completions,
            request,
            cancellationToken))
        {
            yield return chunk;
        }
        
        _logger?.LogDebug("Streaming text completion completed");
    }

    public IChatRequestBuilder CreateRequest()
    {
        return new ChatRequestBuilder(this);
    }

    private static void ValidateRequest(ChatCompletionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ArgumentException("Model is required", nameof(request));
            
        if (request.Messages == null || request.Messages.Length == 0)
            throw new ArgumentException("At least one message is required", nameof(request));
            
        foreach (var message in request.Messages)
        {
            if (string.IsNullOrWhiteSpace(message.Role))
                throw new ArgumentException("Message role is required", nameof(request));
                
            if (message.Content == null && message.ToolCalls == null)
                throw new ArgumentException("Message content or tool calls are required", nameof(request));
        }
        
        if (request.Temperature < 0 || request.Temperature > 2)
            throw new ArgumentException("Temperature must be between 0 and 2", nameof(request));
            
        if (request.TopP < 0 || request.TopP > 1)
            throw new ArgumentException("TopP must be between 0 and 1", nameof(request));
            
        if (request.MaxTokens < 1)
            throw new ArgumentException("MaxTokens must be positive", nameof(request));
    }
}