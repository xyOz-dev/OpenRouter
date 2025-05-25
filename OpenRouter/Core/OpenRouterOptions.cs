using Microsoft.Extensions.Logging;

namespace OpenRouter.Core;

public class OpenRouterOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = Constants.DefaultBaseUrl;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(Constants.DefaultTimeoutSeconds);
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(Constants.DefaultRequestTimeoutSeconds);
    public int MaxRetryAttempts { get; set; } = Constants.DefaultMaxRetries;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(1000);
    public bool EnableRetry { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    public string? HttpReferer { get; set; }
    public string? XTitle { get; set; }
    public bool ValidateApiKey { get; set; } = true;
    public bool ThrowOnApiErrors { get; set; } = true;
    public bool EnableStreamingOptimizations { get; set; } = true;
    public int StreamingBufferSize { get; set; } = 8192;
    
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new ArgumentException("API key is required", nameof(ApiKey));
            
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("Base URL is required", nameof(BaseUrl));
            
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Base URL must be a valid absolute URI", nameof(BaseUrl));
            
        if (Timeout <= TimeSpan.Zero)
            throw new ArgumentException("Timeout must be positive", nameof(Timeout));
            
        if (RequestTimeout <= TimeSpan.Zero)
            throw new ArgumentException("Request timeout must be positive", nameof(RequestTimeout));
            
        if (MaxRetryAttempts < 0)
            throw new ArgumentException("Max retry attempts cannot be negative", nameof(MaxRetryAttempts));
            
        if (RetryDelay < TimeSpan.Zero)
            throw new ArgumentException("Retry delay cannot be negative", nameof(RetryDelay));
            
        if (StreamingBufferSize <= 0)
            throw new ArgumentException("Streaming buffer size must be positive", nameof(StreamingBufferSize));
    }
    
    public OpenRouterOptions Clone()
    {
        return new OpenRouterOptions
        {
            ApiKey = ApiKey,
            BaseUrl = BaseUrl,
            Timeout = Timeout,
            RequestTimeout = RequestTimeout,
            MaxRetryAttempts = MaxRetryAttempts,
            RetryDelay = RetryDelay,
            EnableRetry = EnableRetry,
            EnableLogging = EnableLogging,
            LogLevel = LogLevel,
            DefaultHeaders = new Dictionary<string, string>(DefaultHeaders),
            HttpReferer = HttpReferer,
            XTitle = XTitle,
            ValidateApiKey = ValidateApiKey,
            ThrowOnApiErrors = ThrowOnApiErrors,
            EnableStreamingOptimizations = EnableStreamingOptimizations,
            StreamingBufferSize = StreamingBufferSize
        };
    }
}