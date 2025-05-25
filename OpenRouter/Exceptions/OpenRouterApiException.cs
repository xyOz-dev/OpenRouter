namespace OpenRouter.Exceptions;

public class OpenRouterApiException : OpenRouterException
{
    public object? ErrorDetails { get; }
    
    public OpenRouterApiException(string message, int statusCode, string? errorCode = null, string? requestId = null, object? errorDetails = null)
        : base(message, errorCode, statusCode, requestId)
    {
        ErrorDetails = errorDetails;
    }
    
    public OpenRouterApiException(string message, Exception innerException, int statusCode, string? errorCode = null, string? requestId = null, object? errorDetails = null)
        : base(message, innerException, errorCode, statusCode, requestId)
    {
        ErrorDetails = errorDetails;
    }
}

public class OpenRouterAuthenticationException : OpenRouterApiException
{
    public OpenRouterAuthenticationException(string message, string? requestId = null)
        : base(message, 401, "authentication_error", requestId)
    {
    }
}

public class OpenRouterAuthorizationException : OpenRouterApiException
{
    public OpenRouterAuthorizationException(string message, string? requestId = null)
        : base(message, 403, "authorization_error", requestId)
    {
    }
}

public class OpenRouterRateLimitException : OpenRouterApiException
{
    public TimeSpan? RetryAfter { get; }
    
    public OpenRouterRateLimitException(string message, TimeSpan? retryAfter = null, string? requestId = null)
        : base(message, 429, "rate_limit_exceeded", requestId)
    {
        RetryAfter = retryAfter;
    }
}

public class OpenRouterValidationException : OpenRouterApiException
{
    public Dictionary<string, string[]>? ValidationErrors { get; }
    
    public OpenRouterValidationException(string message, Dictionary<string, string[]>? validationErrors = null, string? requestId = null)
        : base(message, 400, "validation_error", requestId, validationErrors)
    {
        ValidationErrors = validationErrors;
    }
}

public class OpenRouterModerationException : OpenRouterApiException
{
    public OpenRouterModerationException(string message, string? requestId = null)
        : base(message, 400, "moderation_error", requestId)
    {
    }
}

public class OpenRouterProviderException : OpenRouterApiException
{
    public string? ProviderName { get; }
    
    public OpenRouterProviderException(string message, string? providerName = null, int statusCode = 502, string? requestId = null)
        : base(message, statusCode, "provider_error", requestId)
    {
        ProviderName = providerName;
    }
}

public class OpenRouterTimeoutException : OpenRouterException
{
    public TimeSpan Timeout { get; }
    
    public OpenRouterTimeoutException(string message, TimeSpan timeout)
        : base(message)
    {
        Timeout = timeout;
    }
    
    public OpenRouterTimeoutException(string message, TimeSpan timeout, Exception innerException)
        : base(message, innerException)
    {
        Timeout = timeout;
    }
}

public class OpenRouterNetworkException : OpenRouterException
{
    public OpenRouterNetworkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class OpenRouterConfigurationException : OpenRouterException
{
    public string? ConfigurationKey { get; }
    
    public OpenRouterConfigurationException(string message, string? configurationKey = null)
        : base(message)
    {
        ConfigurationKey = configurationKey;
    }
}

public class OpenRouterStreamingException : OpenRouterException
{
    public OpenRouterStreamingException(string message)
        : base(message)
    {
    }
    
    public OpenRouterStreamingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}