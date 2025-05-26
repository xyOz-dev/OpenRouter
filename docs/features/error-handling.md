# Feature: Error Handling

Effective error handling is crucial for building resilient applications. The OpenRouter.NET SDK provides mechanisms to manage and interpret errors that may occur during API interactions.

## Primary Exception: `OpenRouterApiException`

Most errors originating from the OpenRouter API (e.g., invalid API key, malformed request, rate limits, server-side issues) will be wrapped and thrown as an [`OpenRouterApiException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:1).

**Key Properties of `OpenRouterApiException`:**

*   `StatusCode` (System.Net.HttpStatusCode): The HTTP status code returned by the API (e.g., 400, 401, 429, 500).
*   `ErrorResponse` ([`ErrorResponse`](../../OpenRouter/Models/Responses/ErrorResponse.cs:1)?): A deserialized representation of the JSON error payload from OpenRouter, if available. This object typically contains:
    *   `Error.Message` (string): A human-readable error message from the API.
    *   `Error.Type` (string): The type of error (e.g., "invalid_request_error", "authentication_error").
    *   `Error.Code` (string?): A more specific error code, if provided by the API.
    *   `Error.Param` (string?): If the error relates to a specific request parameter, this indicates which one.
*   `Content` (string?): The raw string content of the HTTP error response body. Useful if deserialization into `ErrorResponse` fails or for debugging.
*   `Headers` (System.Net.Http.Headers.HttpResponseHeaders?): The HTTP response headers. Can contain useful information like rate limit details (`RateLimit-...` headers).

**Handling `OpenRouterApiException`:**

```csharp
using OpenRouter.Services.Chat;
using OpenRouter.Models.Requests;
using OpenRouter.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net;

public class ErrorHandlingExample
{
    private readonly IChatService _chatService;
    private readonly ILogger<ErrorHandlingExample> _logger;

    public ErrorHandlingExample(IChatService chatService, ILogger<ErrorHandlingExample> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task AttemptInvalidRequestAsync()
    {
        _logger.LogInformation("--- Error Handling Example ---");
        var request = new ChatCompletionRequest
        {
            Model = "non_existent_model/fake-model-id", // Intentionally invalid model
            Messages = new List<OpenRouter.Models.Common.Message>
                { new OpenRouter.Models.Common.Message { Role = "user", Content = "Hello" } }
        };

        try
        {
            var response = await _chatService.CreateChatCompletionAsync(request);
            _logger.LogInformation("Request succeeded (unexpectedly).");
        }
        catch (OpenRouterApiException apiEx)
        {
            _logger.LogError(apiEx, "OpenRouter API Error occurred.");
            _logger.LogError("Status Code: {StatusCode}", apiEx.StatusCode);
            _logger.LogError("Raw Content: {Content}", apiEx.Content);

            if (apiEx.ErrorResponse?.Error != null)
            {
                _logger.LogError("API Error Type: {Type}", apiEx.ErrorResponse.Error.Type);
                _logger.LogError("API Error Message: {Message}", apiEx.ErrorResponse.Error.Message);
                if (!string.IsNullOrEmpty(apiEx.ErrorResponse.Error.Code))
                {
                    _logger.LogError("API Error Code: {Code}", apiEx.ErrorResponse.Error.Code);
                }
            }

            // Specific handling based on status code
            switch (apiEx.StatusCode)
            {
                case HttpStatusCode.Unauthorized: // 401
                    _logger.LogError("Authentication failed. Check your API key.");
                    break;
                case HttpStatusCode.TooManyRequests: // 429
                    _logger.LogError("Rate limit exceeded. Check response headers for retry information.");
                    // Example: apiEx.Headers?.RetryAfter?.Delta
                    break;
                case HttpStatusCode.BadRequest: // 400
                    _logger.LogError("Bad request. Likely an issue with request parameters. Param: {Param}", apiEx.ErrorResponse?.Error?.Param);
                    break;
                // Add more cases as needed
                default:
                    _logger.LogError("Unhandled API error.");
                    break;
            }
        }
        catch (HttpRequestException httpEx)
        {
            // Network issues, DNS problems, etc.
            _logger.LogError(httpEx, "HTTP Request Error (network level)");
        }
        catch (OpenRouterSerializationException serializationEx)
        {
            // Errors during JSON serialization/deserialization
            _logger.LogError(serializationEx, "Serialization/Deserialization Error");
             _logger.LogError("Raw Content that failed to deserialize: {Content}", serializationEx.Content);
        }
        catch (Exception ex)
        {
            // Other unexpected errors
            _logger.LogError(ex, "An unexpected error occurred.");
        }
    }
}
```

## Other SDK-Specific Exceptions

*   **[`OpenRouterSerializationException`](../../OpenRouter/Exceptions/OpenRouterSerializationException.cs:1)**:
    *   Thrown if there's an issue serializing a request object to JSON or deserializing an API response from JSON.
    *   Contains a `Content` property with the raw string data that caused the serialization issue, which is helpful for debugging.
    *   Inherits from `OpenRouterException`.

*   **[`OpenRouterException`](../../OpenRouter/Exceptions/OpenRouterException.cs:1)**:
    *   A base exception type for general SDK-related errors that are not specific API errors or serialization errors.
    *   You might catch this as a more general fallback for SDK issues.

## Standard .NET Exceptions

*   **`HttpRequestException`**:
    *   Can be thrown by the underlying `HttpClient` for network-level issues (e.g., DNS resolution failure, connection refused, SSL/TLS errors) before an HTTP response is even received from OpenRouter.
*   **`TaskCanceledException` / `OperationCanceledException`**:
    *   Can occur if an HTTP request times out or if a `CancellationToken` passed to an SDK method is signaled.
*   **`ArgumentNullException` / `ArgumentException`**:
    *   May be thrown by SDK methods if required parameters are null or invalid before an API call is made.

## Retry Policy (Polly)

The SDK's `OpenRouterHttpClient` integrates with Polly to provide automatic retries for transient HTTP errors (like 5xx server errors, request timeouts, network glitches).

*   Configuration for retries is available via `OpenRouterOptions.RetryPolicyOptions`.
    *   `MaxRetries` (default: 3)
    *   `InitialDelay` (default: 2 seconds)
    *   `MaxDelay` (default: 30 seconds)
*   This means some `HttpRequestException`s or certain `OpenRouterApiException`s (those with 5xx status codes) might be automatically retried by the SDK before an exception ultimately surfaces in your code.
*   If all retries fail, the final exception will be thrown.

## Best Practices

1.  **Specific Catch Blocks**: Catch `OpenRouterApiException` specifically to handle API errors where you can inspect `StatusCode` and `ErrorResponse`.
2.  **General Catch Blocks**: Include catch blocks for `HttpRequestException`, `OpenRouterSerializationException`, and the general `Exception` for comprehensive error coverage.
3.  **Logging**: Log detailed error information, especially the properties of `OpenRouterApiException` and the content of `OpenRouterSerializationException`.
4.  **User Feedback**: Provide clear, user-friendly error messages based on the type of error encountered. Avoid exposing raw API error messages directly to end-users unless appropriate.
5.  **Rate Limits**: Pay special attention to `429 Too Many Requests` errors. Implement client-side backoff strategies if you hit these frequently, potentially using information from `Retry-After` headers if provided by OpenRouter.
6.  **Idempotency**: For operations that modify state, be mindful of retries. Ensure your requests are idempotent or your system can handle repeated operations if retries occur after a successful but unconfirmed request. (Chat completions are generally safe, but if future API endpoints involve state changes, this is a consideration).

By understanding these exception types and patterns, you can build robust integrations with the OpenRouter API.
Refer to the [`OpenRouter.Examples`](../../OpenRouter.Examples/) project for sample error handling within the example console application.

## Next Steps
*   [Dependency Injection Setup](../advanced/dependency-injection.md)
*   [HTTP Client Customization](../advanced/http-client.md)