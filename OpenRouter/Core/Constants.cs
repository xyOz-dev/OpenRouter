namespace OpenRouter.Core;

public static class Constants
{
    public const string DefaultBaseUrl = "https://openrouter.ai/api/v1";
    public const string UserAgent = "OpenRouter.NET/1.0.0";
    public const int DefaultTimeoutSeconds = 120;
    public const int DefaultMaxRetries = 3;
    public const int DefaultRequestTimeoutSeconds = 60;
    
    public static class Headers
    {
        public const string Authorization = "Authorization";
        public const string HttpReferer = "HTTP-Referer";
        public const string XTitle = "X-Title";
        public const string ContentType = "Content-Type";
        public const string Accept = "Accept";
        public const string UserAgent = "User-Agent";
    }
    
    public static class ContentTypes
    {
        public const string ApplicationJson = "application/json";
        public const string TextEventStream = "text/event-stream";
    }
    
    public static class Endpoints
    {
        public const string ChatCompletions = "chat/completions";
        public const string Completions = "completions";
        public const string Models = "models";
        public const string Generation = "generation";
        public const string Credits = "credits";
        public const string AuthKeys = "auth/keys";
        public const string Key = "key";
        public const string Keys = "keys";
        public const string CoinbaseCredits = "credits/coinbase";
    }
    
    public static class Events
    {
        public const string Done = "[DONE]";
        public const string Data = "data: ";
        public const string Event = "event: ";
        public const string Id = "id: ";
        public const string Retry = "retry: ";
    }
    
    public static class AuthSchemes
    {
        public const string Bearer = "Bearer";
        public const string ApiKey = "X-API-Key";
    }
}