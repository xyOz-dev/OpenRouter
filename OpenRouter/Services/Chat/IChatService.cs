using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;

namespace OpenRouter.Services.Chat;

public interface IChatService
{
    Task<ChatCompletionResponse> CreateAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<ChatCompletionChunk> CreateStreamAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);

    Task<CompletionResponse> CreateCompletionAsync(CompletionRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<CompletionChunk> CreateCompletionStreamAsync(CompletionRequest request, CancellationToken cancellationToken = default);

    IChatRequestBuilder CreateRequest();
}