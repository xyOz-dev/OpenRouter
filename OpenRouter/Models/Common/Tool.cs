using System.Text.Json.Serialization;

namespace OpenRouter.Models.Common;

public class Tool
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";
    
    [JsonPropertyName("function")]
    public FunctionDefinition Function { get; set; } = new();
    
    public static Tool CreateFunction(string name, string description, object? parameters = null)
    {
        return new Tool
        {
            Type = "function",
            Function = new FunctionDefinition
            {
                Name = name,
                Description = description,
                Parameters = parameters
            }
        };
    }
}

public class FunctionDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }
}

public class ToolChoice
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("function")]
    public ToolChoiceFunction? Function { get; set; }
    
    public static ToolChoice Auto() => new() { Type = "auto" };
    
    public static ToolChoice None() => new() { Type = "none" };
    
    public static ToolChoice Required() => new() { Type = "required" };
    
    public static ToolChoice CreateFunction(string name) => new()
    {
        Type = "function",
        Function = new ToolChoiceFunction { Name = name }
    };
}

public class ToolChoiceFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public static class ToolChoiceExtensions
{
    public static object ToRequestObject(this ToolChoice toolChoice)
    {
        return toolChoice.Type switch
        {
            "auto" => "auto",
            "none" => "none",
            "required" => "required",
            "function" => toolChoice,
            _ => "auto"
        };
    }
}