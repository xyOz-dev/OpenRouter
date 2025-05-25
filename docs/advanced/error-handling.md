# Error Handling and Exceptions

The OpenRouter .NET library provides a comprehensive exception hierarchy designed to help developers handle various error scenarios gracefully. This document covers the exception types, handling patterns, and best practices for robust error management.

## Exception Hierarchy

### [`OpenRouterException`](OpenRouter/Exceptions/OpenRouterException.cs:3) Base Class

All OpenRouter-specific exceptions inherit from [`OpenRouterException`](OpenRouter/Exceptions/OpenRouterException.cs:3), providing a consistent base for exception handling:

- Inherits from `Exception`
- Contains common properties for error context
- Enables catch-all exception handling for OpenRouter-related errors
- Preserves inner exceptions for debugging

### [`OpenRouterApiException`](OpenRouter/Exceptions/OpenRouterApiException.cs:3) for API Errors

The [`OpenRouterApiException`](OpenRouter/Exceptions/OpenRouterApiException.cs:3) serves as the base class for all API-related errors:

- Contains HTTP status code information
- Includes API error messages and codes
- Provides request/response correlation data
- Supports error details from OpenRouter API responses

## API-Specific Exceptions

### Authentication and Authorization Errors

**[`OpenRouterAuthenticationException`](OpenRouter/Exceptions/OpenRouterApiException.cs:20)**
- Thrown when API key is missing, invalid, or malformed
- HTTP Status: 401 Unauthorized
- Common causes: Missing API key, expired key, incorrect key format

**[`OpenRouterAuthorizationException`](OpenRouter/Exceptions/OpenRouterApiException.cs:28)**
- Thrown when authentication is valid but access is denied
- HTTP Status: 403 Forbidden
- Common causes: Insufficient permissions, account restrictions, feature access limitations

### Rate Limiting and Quota Errors

**[`OpenRouterRateLimitException`](OpenRouter/Exceptions/OpenRouterApiException.cs:36)**
- Thrown when API rate limits are exceeded
- HTTP Status: 429 Too Many Requests
- Contains retry-after information when available
- Includes current usage and limit information

### Request Validation Errors

**[`OpenRouterValidationException`](OpenRouter/Exceptions/OpenRouterApiException.cs:47)**
- Thrown when request parameters are invalid
- HTTP Status: 400 Bad Request
- Contains detailed validation error messages
- Includes field-specific error information

### Content and Safety Errors

**[`OpenRouterModerationException`](OpenRouter/Exceptions/OpenRouterApiException.cs:58)**
- Thrown when content violates safety policies
- Contains moderation category information
- Includes content policy details
- Provides guidance for content modification

### Provider and Service Errors

**[`OpenRouterProviderException`](OpenRouter/Exceptions/OpenRouterApiException.cs:66)**
- Thrown when upstream model provider encounters errors
- Contains provider-specific error information
- Includes provider availability status
- May suggest alternative models

## Client-Side Exceptions

### Network and Connectivity Errors

**[`OpenRouterTimeoutException`](OpenRouter/Exceptions/OpenRouterApiException.cs:77)**
- Thrown when requests exceed configured timeout periods
- Contains timeout configuration information
- Distinguishes between connection and response timeouts
- Provides retry recommendations

**[`OpenRouterNetworkException`](OpenRouter/Exceptions/OpenRouterApiException.cs:94)**
- Thrown for general network connectivity issues
- Contains underlying network error information
- Includes connection diagnostics data
- Provides troubleshooting guidance

### Configuration and Setup Errors

**[`OpenRouterConfigurationException`](OpenRouter/Exceptions/OpenRouterApiException.cs:102)**
- Thrown when client configuration is invalid
- Contains configuration validation details
- Includes setup guidance and examples
- Prevents runtime configuration errors

### Streaming and Response Errors

**[`OpenRouterStreamingException`](OpenRouter/Exceptions/OpenRouterApiException.cs:113)**
- Thrown during streaming response processing
- Contains stream state information
- Includes partial response data when available
- Provides stream recovery options

**[`OpenRouterSerializationException`](OpenRouter/Exceptions/OpenRouterSerializationException.cs:3)**
- Thrown when JSON serialization/deserialization fails
- Contains serialization context information
- Includes problematic data details
- Provides format guidance

## Error Handling Patterns

### Try-Catch Best Practices

**Specific Exception Handling**
```csharp
try
{
    var response = await client.Chat
        .WithModel("gpt-4")
        .WithMessages("Hello, world!")
        .ExecuteAsync();
}
catch (OpenRouterAuthenticationException ex)
{
    // Handle authentication errors - redirect to API key setup
    logger.LogError(ex, "Authentication failed");
    return RedirectToApiKeySetup();
}
catch (OpenRouterRateLimitException ex)
{
    // Handle rate limiting - implement exponential backoff
    logger.LogWarning(ex, "Rate limit exceeded");
    await Task.Delay(ex.RetryAfter ?? TimeSpan.FromMinutes(1));
    return await RetryRequest();
}
catch (OpenRouterValidationException ex)
{
    // Handle validation errors - return user-friendly messages
    logger.LogWarning(ex, "Request validation failed");
    return BadRequest(ex.ValidationErrors);
}
```

<!-- C# Code Example: Comprehensive exception handling strategy -->

### Retry Logic Implementation

**Exponential Backoff Pattern**
```csharp
public async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    var retryCount = 0;
    var delay = TimeSpan.FromSeconds(1);

    while (retryCount < maxRetries)
    {
        try
        {
            return await operation();
        }
        catch (OpenRouterRateLimitException ex)
        {
            retryCount++;
            var retryDelay = ex.RetryAfter ?? delay;
            
            if (retryCount >= maxRetries)
                throw;
                
            await Task.Delay(retryDelay);
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
        }
        catch (OpenRouterNetworkException ex) when (retryCount < maxRetries)
        {
            retryCount++;
            await Task.Delay(delay);
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
        }
    }

    throw new InvalidOperationException("Maximum retry attempts exceeded");
}
```

<!-- C# Code Example: Polly integration for advanced retry policies -->

### Graceful Degradation Strategies

**Fallback Model Selection**
```csharp
public async Task<ChatCompletionResponse> GetChatCompletionWithFallback(string message)
{
    var models = new[] { "gpt-4", "gpt-3.5-turbo", "claude-3-sonnet" };
    
    foreach (var model in models)
    {
        try
        {
            return await client.Chat
                .WithModel(model)
                .WithMessages(message)
                .ExecuteAsync();
        }
        catch (OpenRouterProviderException ex) when (ex.IsProviderUnavailable)
        {
            logger.LogWarning(ex, "Model {Model} unavailable, trying fallback", model);
            continue;
        }
        catch (OpenRouterRateLimitException ex) when (ex.IsModelSpecific)
        {
            logger.LogWarning(ex, "Model {Model} rate limited, trying fallback", model);
            continue;
        }
    }
    
    throw new OpenRouterException("All fallback models failed");
}
```

<!-- C# Code Example: Circuit breaker pattern for provider failures -->

## Logging and Diagnostics

### Error Logging Recommendations

**Structured Logging with Serilog**
```csharp
public class OpenRouterErrorHandler
{
    private readonly ILogger<OpenRouterErrorHandler> logger;

    public async Task<T> HandleRequest<T>(Func<Task<T>> request, string operationName)
    {
        try
        {
            logger.LogInformation("Starting {Operation}", operationName);
            var result = await request();
            logger.LogInformation("Completed {Operation} successfully", operationName);
            return result;
        }
        catch (OpenRouterException ex)
        {
            logger.LogError(ex, 
                "OpenRouter operation {Operation} failed with {ExceptionType}: {Message}",
                operationName, ex.GetType().Name, ex.Message);
            
            // Log additional context based on exception type
            switch (ex)
            {
                case OpenRouterApiException apiEx:
                    logger.LogError("API Error - Status: {StatusCode}, Code: {ErrorCode}",
                        apiEx.StatusCode, apiEx.ErrorCode);
                    break;
                case OpenRouterRateLimitException rateLimitEx:
                    logger.LogError("Rate Limited - Retry After: {RetryAfter}, Usage: {Usage}",
                        rateLimitEx.RetryAfter, rateLimitEx.CurrentUsage);
                    break;
            }
            
            throw;
        }
    }
}
```

### Diagnostic Information Collection

**Exception Context Enrichment**
<!-- C# Code Example: Adding correlation IDs and request context to exceptions -->
<!-- C# Code Example: Performance metrics collection during error scenarios -->
<!-- C# Code Example: Integration with Application Insights or other monitoring tools -->

## Code Examples

### Comprehensive Error Handling Scenarios

<!-- C# Code Example: Complete error handling in ASP.NET Core web application -->
<!-- C# Code Example: Error handling in background services and hosted applications -->
<!-- C# Code Example: Unit testing exception scenarios with mock clients -->
<!-- C# Code Example: Custom exception mapping for domain-specific error handling -->
<!-- C# Code Example: Integration with health checks and monitoring systems -->

The comprehensive exception hierarchy and handling patterns enable developers to build resilient applications that gracefully handle various error conditions while providing meaningful feedback to users and detailed diagnostics for troubleshooting.