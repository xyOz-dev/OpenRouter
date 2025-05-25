# Exception Reference

This document provides comprehensive reference for all exception types in the OpenRouter .NET library, including the exception hierarchy, properties, and usage patterns.

## Exception Hierarchy Diagram

```
Exception
└── OpenRouterException (abstract)
    ├── OpenRouterApiException
    │   ├── OpenRouterAuthenticationException
    │   ├── OpenRouterAuthorizationException
    │   ├── OpenRouterRateLimitException
    │   ├── OpenRouterValidationException
    │   ├── OpenRouterModerationException
    │   └── OpenRouterProviderException
    ├── OpenRouterTimeoutException
    ├── OpenRouterNetworkException
    ├── OpenRouterConfigurationException
    ├── OpenRouterStreamingException
    └── OpenRouterSerializationException
```

## Base Exceptions

### [`OpenRouterException`](../../OpenRouter/Exceptions/OpenRouterException.cs:3) (Abstract)

The base class for all OpenRouter-related exceptions, providing common properties and functionality.

#### Properties

```csharp
string? ErrorCode { get; }
int? StatusCode { get; }
string? RequestId { get; }
```

- **`ErrorCode`** - Specific error code from the API or library
- **`StatusCode`** - HTTP status code when applicable
- **`RequestId`** - Unique identifier for the request (useful for support)

#### Constructors

```csharp
protected OpenRouterException(string message);
protected OpenRouterException(string message, Exception innerException);
protected OpenRouterException(string message, string? errorCode = null, int? statusCode = null, string? requestId = null);
protected OpenRouterException(string message, Exception innerException, string? errorCode = null, int? statusCode = null, string? requestId = null);
```

### [`OpenRouterApiException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:3)

Base class for exceptions that originate from API responses, extending [`OpenRouterException`](../../OpenRouter/Exceptions/OpenRouterException.cs:3) with API-specific information.

#### Additional Properties

```csharp
object? ErrorDetails { get; }
```

- **`ErrorDetails`** - Detailed error information from the API response

#### Constructors

```csharp
public OpenRouterApiException(string message, int statusCode, string? errorCode = null, string? requestId = null, object? errorDetails = null);
public OpenRouterApiException(string message, Exception innerException, int statusCode, string? errorCode = null, string? requestId = null, object? errorDetails = null);
```

## Specific Exception Types

### Authentication and Authorization

#### [`OpenRouterAuthenticationException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:20)

Thrown when API key authentication fails.

**Common scenarios:**
- Invalid API key
- Expired API key
- Missing API key

**Properties:**
- `StatusCode`: Always 401
- `ErrorCode`: "authentication_error"

**Constructor:**
```csharp
public OpenRouterAuthenticationException(string message, string? requestId = null);
```

<!-- C# Code Example: Handling authentication exceptions -->
```csharp
try
{
    var response = await client.Chat.CreateRequest()
        .WithModel("gpt-3.5-turbo")
        .WithUserMessage("Hello!")
        .ExecuteAsync();
}
catch (OpenRouterAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
    Console.WriteLine($"Request ID: {ex.RequestId}");
    // Handle re-authentication or configuration update
}
```

#### [`OpenRouterAuthorizationException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:28)

Thrown when the authenticated user lacks permissions for the requested operation.

**Common scenarios:**
- Insufficient permissions for the endpoint
- Account limitations
- Feature not available for current plan

**Properties:**
- `StatusCode`: Always 403
- `ErrorCode`: "authorization_error"

**Constructor:**
```csharp
public OpenRouterAuthorizationException(string message, string? requestId = null);
```

### Rate Limiting

#### [`OpenRouterRateLimitException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:36)

Thrown when API rate limits are exceeded.

**Properties:**
- `StatusCode`: Always 429
- `ErrorCode`: "rate_limit_exceeded"
- `RetryAfter`: Time to wait before retrying

**Additional Properties:**
```csharp
TimeSpan? RetryAfter { get; }
```

**Constructor:**
```csharp
public OpenRouterRateLimitException(string message, TimeSpan? retryAfter = null, string? requestId = null);
```

**Recommended handling:**
```csharp
try
{
    var response = await client.Chat.CreateAsync(request);
}
catch (OpenRouterRateLimitException ex)
{
    if (ex.RetryAfter.HasValue)
    {
        Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter.Value}");
        await Task.Delay(ex.RetryAfter.Value);
        // Retry the request
    }
}
```

### Validation Errors

#### [`OpenRouterValidationException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:47)

Thrown when request validation fails.

**Common scenarios:**
- Invalid parameter values
- Missing required parameters
- Parameter format errors

**Properties:**
- `StatusCode`: Always 400
- `ErrorCode`: "validation_error"
- `ValidationErrors`: Detailed validation error information

**Additional Properties:**
```csharp
Dictionary<string, string[]>? ValidationErrors { get; }
```

**Constructor:**
```csharp
public OpenRouterValidationException(string message, Dictionary<string, string[]>? validationErrors = null, string? requestId = null);
```

**Usage example:**
```csharp
try
{
    var response = await client.Chat.CreateAsync(invalidRequest);
}
catch (OpenRouterValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    
    if (ex.ValidationErrors != null)
    {
        foreach (var error in ex.ValidationErrors)
        {
            Console.WriteLine($"Field '{error.Key}': {string.Join(", ", error.Value)}");
        }
    }
}
```

### Content Moderation

#### [`OpenRouterModerationException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:58)

Thrown when content is rejected by moderation systems.

**Common scenarios:**
- Harmful content detected
- Policy violations
- Content filtering triggers

**Properties:**
- `StatusCode`: Always 400
- `ErrorCode`: "moderation_error"

**Constructor:**
```csharp
public OpenRouterModerationException(string message, string? requestId = null);
```

### Provider Issues

#### [`OpenRouterProviderException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:66)

Thrown when upstream provider issues occur.

**Common scenarios:**
- Provider API unavailable
- Provider rate limits
- Provider-specific errors

**Properties:**
- `StatusCode`: Typically 502, but can vary
- `ErrorCode`: "provider_error"
- `ProviderName`: Name of the affected provider

**Additional Properties:**
```csharp
string? ProviderName { get; }
```

**Constructor:**
```csharp
public OpenRouterProviderException(string message, string? providerName = null, int statusCode = 502, string? requestId = null);
```

## Non-API Exceptions

### [`OpenRouterTimeoutException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:77)

Thrown when requests exceed configured timeout values.

**Properties:**
- `Timeout`: The timeout duration that was exceeded

**Additional Properties:**
```csharp
TimeSpan Timeout { get; }
```

**Constructors:**
```csharp
public OpenRouterTimeoutException(string message, TimeSpan timeout);
public OpenRouterTimeoutException(string message, TimeSpan timeout, Exception innerException);
```

**Recommended handling:**
```csharp
try
{
    var response = await client.Chat.CreateAsync(request);
}
catch (OpenRouterTimeoutException ex)
{
    Console.WriteLine($"Request timed out after {ex.Timeout}");
    // Consider retrying with longer timeout or different configuration
}
```

### [`OpenRouterNetworkException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:94)

Thrown when network-level issues occur.

**Common scenarios:**
- DNS resolution failures
- Connection timeouts
- SSL/TLS errors

**Constructor:**
```csharp
public OpenRouterNetworkException(string message, Exception innerException);
```

### [`OpenRouterConfigurationException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:102)

Thrown when configuration issues are detected.

**Properties:**
- `ConfigurationKey`: The specific configuration key that caused the issue

**Additional Properties:**
```csharp
string? ConfigurationKey { get; }
```

**Constructor:**
```csharp
public OpenRouterConfigurationException(string message, string? configurationKey = null);
```

### [`OpenRouterStreamingException`](../../OpenRouter/Exceptions/OpenRouterApiException.cs:113)

Thrown when streaming operations encounter issues.

**Common scenarios:**
- Stream interruption
- Invalid streaming format
- Connection loss during streaming

**Constructors:**
```csharp
public OpenRouterStreamingException(string message);
public OpenRouterStreamingException(string message, Exception innerException);
```

### [`OpenRouterSerializationException`](../../OpenRouter/Exceptions/OpenRouterSerializationException.cs)

Thrown when JSON serialization/deserialization issues occur.

**Common scenarios:**
- Invalid JSON response
- Unexpected response format
- Type conversion errors

## Exception Usage Patterns

### When to Catch Specific vs General Exceptions

#### Catch Specific Exceptions When:
- You need to handle different error types differently
- You want to implement retry logic for specific scenarios
- You need access to exception-specific properties

```csharp
try
{
    var response = await client.Chat.CreateAsync(request);
}
catch (OpenRouterRateLimitException rateLimitEx)
{
    // Implement exponential backoff
    await WaitAndRetry(rateLimitEx.RetryAfter);
}
catch (OpenRouterAuthenticationException authEx)
{
    // Refresh authentication
    await RefreshApiKey();
}
catch (OpenRouterValidationException validationEx)
{
    // Fix request parameters
    FixValidationErrors(validationEx.ValidationErrors);
}
```

#### Catch General Exceptions When:
- You want to handle all OpenRouter exceptions uniformly
- You're implementing general error logging
- You don't need specific error handling logic

```csharp
try
{
    var response = await client.Chat.CreateAsync(request);
}
catch (OpenRouterApiException apiEx)
{
    // Handle all API-related exceptions
    Logger.LogError("API Error: {Message} (Status: {Status}, Code: {Code})", 
        apiEx.Message, apiEx.StatusCode, apiEx.ErrorCode);
}
catch (OpenRouterException ex)
{
    // Handle all OpenRouter exceptions
    Logger.LogError("OpenRouter Error: {Message}", ex.Message);
}
```

### Exception Property Inspection

All OpenRouter exceptions provide properties that can be used for logging, monitoring, and debugging:

```csharp
catch (OpenRouterException ex)
{
    var logContext = new
    {
        Message = ex.Message,
        ErrorCode = ex.ErrorCode,
        StatusCode = ex.StatusCode,
        RequestId = ex.RequestId,
        InnerException = ex.InnerException?.Message
    };
    
    Logger.LogError("OpenRouter exception: {@LogContext}", logContext);
    
    // Send to monitoring service
    if (ex.RequestId != null)
    {
        TelemetryClient.TrackException(ex, new Dictionary<string, string>
        {
            ["RequestId"] = ex.RequestId,
            ["ErrorCode"] = ex.ErrorCode ?? "unknown"
        });
    }
}
```

### Logging and Diagnostic Information Extraction

#### Comprehensive Exception Logging

```csharp
public static void LogOpenRouterException(ILogger logger, OpenRouterException exception)
{
    var logLevel = exception switch
    {
        OpenRouterAuthenticationException => LogLevel.Warning,
        OpenRouterRateLimitException => LogLevel.Information,
        OpenRouterValidationException => LogLevel.Warning,
        OpenRouterConfigurationException => LogLevel.Error,
        _ => LogLevel.Error
    };
    
    logger.Log(logLevel, exception, 
        "OpenRouter operation failed: {Message} | ErrorCode: {ErrorCode} | StatusCode: {StatusCode} | RequestId: {RequestId}",
        exception.Message, 
        exception.ErrorCode, 
        exception.StatusCode, 
        exception.RequestId);
}
```

#### Exception Metrics Collection

```csharp
public static void TrackExceptionMetrics(OpenRouterException exception)
{
    var tags = new Dictionary<string, string>
    {
        ["exception_type"] = exception.GetType().Name,
        ["error_code"] = exception.ErrorCode ?? "unknown",
        ["status_code"] = exception.StatusCode?.ToString() ?? "unknown"
    };
    
    // Increment counter
    Metrics.Counter("openrouter_exceptions_total", tags).Increment();
    
    // Track rate limits specifically
    if (exception is OpenRouterRateLimitException rateLimitEx && rateLimitEx.RetryAfter.HasValue)
    {
        Metrics.Histogram("openrouter_rate_limit_retry_after_seconds", tags)
            .Record(rateLimitEx.RetryAfter.Value.TotalSeconds);
    }
}