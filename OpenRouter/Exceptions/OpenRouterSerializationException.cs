namespace OpenRouter.Exceptions;

public class OpenRouterSerializationException : OpenRouterException
{
    public string? ResponseContent { get; }
    public Type? TargetType { get; }
    
    public OpenRouterSerializationException(string message, string? responseContent = null, Type? targetType = null)
        : base(message)
    {
        ResponseContent = responseContent;
        TargetType = targetType;
    }
    
    public OpenRouterSerializationException(string message, Exception innerException, string? responseContent = null, Type? targetType = null)
        : base(message, innerException)
    {
        ResponseContent = responseContent;
        TargetType = targetType;
    }
}