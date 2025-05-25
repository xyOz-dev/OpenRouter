using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using OpenRouter.Authentication;
using OpenRouter.Core;
using OpenRouter.Exceptions;
using OpenRouter.Models.Responses;

namespace OpenRouter.Http;

public class OpenRouterHttpClient : IHttpClientProvider
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationProvider _authProvider;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterHttpClient>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private bool _disposed;

    public OpenRouterHttpClient(
        HttpClient httpClient,
        IAuthenticationProvider authProvider,
        OpenRouterOptions options,
        ILogger<OpenRouterHttpClient>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        _retryPolicy = CreateRetryPolicy();
        ConfigureHttpClient();
    }

    public async Task<T> SendAsync<T>(string endpoint, object? request = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRawAsync(endpoint, request, HttpMethod.Post, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonOptions) ?? 
                   throw new OpenRouterSerializationException($"Failed to deserialize response of type {typeof(T).Name}", content, typeof(T));
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Failed to deserialize response: {Content}", content);
            throw new OpenRouterSerializationException("Failed to deserialize API response", ex, content, typeof(T));
        }
    }

    public async Task<HttpResponseMessage> SendRawAsync(string endpoint, object? request = null, HttpMethod? method = null, CancellationToken cancellationToken = default)
    {
        method ??= HttpMethod.Post;
        
        var requestMessage = await CreateRequestMessage(endpoint, request, method, cancellationToken);
        
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var clonedRequest = await CloneRequestMessage(requestMessage, cancellationToken);
                _logger?.LogDebug("Sending {Method} request to {Endpoint}", method, endpoint);
                return await _httpClient.SendAsync(clonedRequest, cancellationToken);
            });

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response, cancellationToken);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error occurred while making request to {Endpoint}", endpoint);
            throw new OpenRouterNetworkException("Network error occurred", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger?.LogError(ex, "Request to {Endpoint} timed out", endpoint);
            throw new OpenRouterTimeoutException("Request timed out", _options.RequestTimeout, ex);
        }
    }

    public async IAsyncEnumerable<T> StreamAsync<T>(string endpoint, object? request = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestMessage = await CreateRequestMessage(endpoint, request, HttpMethod.Post, cancellationToken);
        requestMessage.Headers.Accept.Clear();
        requestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(Constants.ContentTypes.TextEventStream));

        HttpResponseMessage response;
        
        try
        {
            response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error occurred while streaming from {Endpoint}", endpoint);
            throw new OpenRouterNetworkException("Network error occurred during streaming", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger?.LogError(ex, "Stream to {Endpoint} was cancelled", endpoint);
            throw new OpenRouterTimeoutException("Stream was cancelled", _options.RequestTimeout, ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                await HandleErrorResponse(response, cancellationToken);
            }
            finally
            {
                response.Dispose();
            }
            yield break;
        }

        _logger?.LogDebug("Starting SSE stream for endpoint {Endpoint}", endpoint);

        await foreach (var item in ReadServerSentEventsInternal<T>(response, cancellationToken))
        {
            yield return item;
        }
    }

    public async Task<string> SendStringAsync(string endpoint, object? request = null, CancellationToken cancellationToken = default)
    {
        var response = await SendRawAsync(endpoint, request, HttpMethod.Post, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<HttpRequestMessage> CreateRequestMessage(string endpoint, object? request, HttpMethod method, CancellationToken cancellationToken)
    {
        var requestMessage = new HttpRequestMessage(method, endpoint);
        
        var authHeader = await _authProvider.GetAuthHeaderAsync(cancellationToken);
        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_authProvider.AuthenticationScheme, authHeader);

        if (request != null)
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            requestMessage.Content = new StringContent(json, Encoding.UTF8, Constants.ContentTypes.ApplicationJson);
            _logger?.LogDebug("Request payload: {Json}", json);
        }

        return requestMessage;
    }

    private async Task<HttpRequestMessage> CloneRequestMessage(HttpRequestMessage original, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (original.Content != null)
        {
            var content = await original.Content.ReadAsStringAsync(cancellationToken);
            clone.Content = new StringContent(content, Encoding.UTF8, Constants.ContentTypes.ApplicationJson);
        }

        return clone;
    }

    private async IAsyncEnumerable<T> ReadServerSentEventsInternal<T>(HttpResponseMessage response, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using (response)
        using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
        using (var reader = new StreamReader(stream))
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith(Constants.Events.Data))
                {
                    var data = line[Constants.Events.Data.Length..].Trim();
                    
                    if (data == Constants.Events.Done)
                    {
                        _logger?.LogDebug("Stream completed with [DONE] marker");
                        yield break;
                    }

                    T? item = default;
                    try
                    {
                        item = JsonSerializer.Deserialize<T>(data, _jsonOptions);
                    }
                    catch (JsonException ex)
                    {
                        _logger?.LogWarning(ex, "Failed to deserialize streaming data: {Data}", data);
                        continue;
                    }

                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var requestId = response.Headers.TryGetValues("x-request-id", out var values) ? values.FirstOrDefault() : null;
        
        _logger?.LogError("API error response: {StatusCode} - {Content}", response.StatusCode, content);

        ErrorResponse? errorResponse = null;
        try
        {
            errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
        }
        catch (JsonException)
        {
            // If we can't parse the error response, create a generic one
        }

        var errorMessage = errorResponse?.Error?.Message ?? $"API request failed with status {response.StatusCode}";
        var errorCode = errorResponse?.Error?.Code;

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                throw new OpenRouterAuthenticationException(errorMessage, requestId);
            
            case HttpStatusCode.Forbidden:
                throw new OpenRouterAuthorizationException(errorMessage, requestId);
            
            case HttpStatusCode.BadRequest when errorCode == "validation_error":
                throw new OpenRouterValidationException(errorMessage, errorResponse?.Error?.ValidationErrors, requestId);
            
            case HttpStatusCode.BadRequest when errorCode == "moderation_error":
                throw new OpenRouterModerationException(errorMessage, requestId);
            
            case HttpStatusCode.TooManyRequests:
                var retryAfter = errorResponse?.Error?.RetryAfter != null 
                    ? TimeSpan.FromSeconds(errorResponse.Error.RetryAfter.Value) 
                    : (TimeSpan?)null;
                throw new OpenRouterRateLimitException(errorMessage, retryAfter, requestId);
            
            case HttpStatusCode.BadGateway:
            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.GatewayTimeout:
                throw new OpenRouterProviderException(errorMessage, errorResponse?.Error?.ProviderName, (int)response.StatusCode, requestId);
            
            default:
                throw new OpenRouterApiException(errorMessage, (int)response.StatusCode, errorCode, requestId, errorResponse?.Error);
        }
    }

    private void ConfigureHttpClient()
    {
        // Ensure BaseUrl ends with a slash for proper URL combination
        var baseUrl = _options.BaseUrl.EndsWith("/") ? _options.BaseUrl : _options.BaseUrl + "/";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = _options.Timeout;
        
        _httpClient.DefaultRequestHeaders.Clear();
        // User-Agent will be handled by the _options.DefaultHeaders loop
        
        if (!string.IsNullOrEmpty(_options.HttpReferer))
        {
            _httpClient.DefaultRequestHeaders.Add(Constants.Headers.HttpReferer, _options.HttpReferer);
        }
        
        if (!string.IsNullOrEmpty(_options.XTitle))
        {
            _httpClient.DefaultRequestHeaders.Add(Constants.Headers.XTitle, _options.XTitle);
        }
        
        foreach (var header in _options.DefaultHeaders)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        if (!_options.EnableRetry)
        {
            return Policy.NoOpAsync<HttpResponseMessage>();
        }

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<OpenRouterRateLimitException>()
            .WaitAndRetryAsync(
                retryCount: _options.MaxRetryAttempts,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(_options.RetryDelay.TotalMilliseconds * Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger?.LogWarning("Retrying request in {Delay}ms (attempt {RetryCount}/{MaxRetries})", 
                        timespan.TotalMilliseconds, retryCount, _options.MaxRetryAttempts);
                });
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}