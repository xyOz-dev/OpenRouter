namespace OpenRouter.Exceptions;

public abstract class OpenRouterException : Exception
{
    public string? ErrorCode { get; }
    public int? StatusCode { get; }
    public string? RequestId { get; }
    
    protected OpenRouterException(string message) : base(message)
    {
    }
    
    protected OpenRouterException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    protected OpenRouterException(string message, string? errorCode = null, int? statusCode = null, string? requestId = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        RequestId = requestId;
    }
    
    protected OpenRouterException(string message, Exception innerException, string? errorCode = null, int? statusCode = null, string? requestId = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        RequestId = requestId;
    }
}