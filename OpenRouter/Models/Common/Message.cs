using System.Text.Json.Serialization;

namespace OpenRouter.Models.Common;

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public object? Content { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("tool_calls")]
    public ToolCall[]? ToolCalls { get; set; }
    
    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
    
    [JsonIgnore]
    public bool IsSystemMessage => Role.Equals("system", StringComparison.OrdinalIgnoreCase);
    
    [JsonIgnore]
    public bool IsUserMessage => Role.Equals("user", StringComparison.OrdinalIgnoreCase);
    
    [JsonIgnore]
    public bool IsAssistantMessage => Role.Equals("assistant", StringComparison.OrdinalIgnoreCase);
    
    [JsonIgnore]
    public bool IsToolMessage => Role.Equals("tool", StringComparison.OrdinalIgnoreCase);
    
    [JsonIgnore]
    public string? TextContent => Content as string;
    
    public static Message System(string content) => new() { Role = "system", Content = content };
    
    public static Message User(string content) => new() { Role = "user", Content = content };
    
    public static Message User(MessageContent[] content) => new() { Role = "user", Content = content };
    
    public static Message Assistant(string content) => new() { Role = "assistant", Content = content };
    
    public static Message Assistant(string? content, ToolCall[] toolCalls) => new() 
    { 
        Role = "assistant", 
        Content = content, 
        ToolCalls = toolCalls 
    };
    
    public static Message Tool(string content, string toolCallId) => new() 
    { 
        Role = "tool", 
        Content = content, 
        ToolCallId = toolCallId 
    };
}

public class MessageContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("image_url")]
    public ImageUrl? ImageUrl { get; set; }
    
    [JsonPropertyName("document_url")]
    public DocumentUrl? DocumentUrl { get; set; }
    
    public static MessageContent CreateText(string text) => new() { Type = "text", Text = text };
    
    public static MessageContent Image(string url, string? detail = null) => new()
    {
        Type = "image_url",
        ImageUrl = new ImageUrl { Url = url, Detail = detail }
    };
    
    public static MessageContent Document(string url, string? type = "pdf") => new()
    {
        Type = "document_url",
        DocumentUrl = new DocumentUrl { Url = url, Type = type }
    };
}

public class DocumentUrl
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class ImageUrl
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}

public class ToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";
    
    [JsonPropertyName("function")]
    public FunctionCall Function { get; set; } = new();
}

public class FunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}