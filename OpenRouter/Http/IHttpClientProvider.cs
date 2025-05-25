using OpenRouter.Models.Responses;

namespace OpenRouter.Http;

public interface IHttpClientProvider : IDisposable
{
    Task<T> SendAsync<T>(string endpoint, object? request = null, CancellationToken cancellationToken = default);
    
    Task<HttpResponseMessage> SendRawAsync(string endpoint, object? request = null, HttpMethod? method = null, CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<T> StreamAsync<T>(string endpoint, object? request = null, CancellationToken cancellationToken = default);
    
    Task<string> SendStringAsync(string endpoint, object? request = null, CancellationToken cancellationToken = default);
}