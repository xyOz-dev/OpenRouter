# Tool Calling and Function Integration

The OpenRouter .NET library provides comprehensive support for tool calling and function integration, enabling AI models to execute functions and integrate with external systems. This document covers tool definition, configuration, execution patterns, and advanced integration scenarios.

## Tool Calling Overview

### Function Calling Capabilities

Tool calling enables AI models to:
- Execute predefined functions with validated parameters
- Integrate with external APIs and services
- Perform calculations and data processing
- Access real-time information and databases
- Trigger business logic and workflows
- Provide structured responses with computed data

The tool calling system provides:
- Type-safe function parameter validation
- Automatic JSON schema generation
- Async function execution support
- Error handling and retry mechanisms
- Multi-turn conversation support with tool results

## Defining Tools

### [`Tool`](OpenRouter/Models/Common/Tool.cs:1) Model Structure

The [`Tool`](OpenRouter/Models/Common/Tool.cs:1) model defines the structure for function definitions:

```csharp
public class Tool
{
    public string Type { get; set; } = "function";
    public Function Function { get; set; }
}

public class Function
{
    public string Name { get; set; }
    public string Description { get; set; }
    public object Parameters { get; set; }
}
```

### Function Schema Definition

**Basic Function Definition**
```csharp
var weatherTool = new Tool
{
    Function = new Function
    {
        Name = "get_weather",
        Description = "Get current weather information for a location",
        Parameters = new
        {
            type = "object",
            properties = new
            {
                location = new
                {
                    type = "string",
                    description = "The city and state, e.g. San Francisco, CA"
                },
                unit = new
                {
                    type = "string",
                    @enum = new[] { "celsius", "fahrenheit" },
                    description = "Temperature unit"
                }
            },
            required = new[] { "location" }
        }
    }
};
```

### Parameter Specification

**Complex Parameter Schema**
```csharp
var databaseQueryTool = new Tool
{
    Function = new Function
    {
        Name = "query_database",
        Description = "Execute a database query with filters and sorting",
        Parameters = new
        {
            type = "object",
            properties = new
            {
                table = new
                {
                    type = "string",
                    description = "Database table name"
                },
                filters = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            field = new { type = "string" },
                            @operator = new { type = "string", @enum = new[] { "eq", "gt", "lt", "contains" } },
                            value = new { type = "string" }
                        },
                        required = new[] { "field", "operator", "value" }
                    }
                },
                limit = new
                {
                    type = "integer",
                    minimum = 1,
                    maximum = 1000,
                    @default = 10
                }
            },
            required = new[] { "table" }
        }
    }
};
```

<!-- C# Code Example: Automatic schema generation from C# classes using reflection -->

## Tool Configuration

### [`WithTools()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:15) Method Usage

**Basic Tool Configuration**
```csharp
var tools = new[]
{
    weatherTool,
    calculatorTool,
    databaseTool
};

var response = await client.Chat
    .WithModel("gpt-4")
    .WithMessages("What's the weather like in Paris and calculate 15% tip on $120?")
    .WithTools(tools)
    .ExecuteAsync();
```

**Tool Configuration with Fluent API**
```csharp
var response = await client.Chat
    .WithModel("claude-3-sonnet")
    .WithMessages(userMessage)
    .WithTools(builder =>
    {
        builder.AddWeatherTool();
        builder.AddCalculatorTool();
        builder.AddDatabaseTool();
    })
    .ExecuteAsync();
```

### [`WithToolChoice()`](OpenRouter/Services/Chat/IChatRequestBuilder.cs:16) Control

**Tool Choice Configuration**
```csharp
// Auto mode - let the model decide
var response1 = await client.Chat
    .WithMessages(message)
    .WithTools(tools)
    .WithToolChoice("auto")
    .ExecuteAsync();

// Force specific tool usage
var response2 = await client.Chat
    .WithMessages(message)
    .WithTools(tools)
    .WithToolChoice(new { type = "function", function = new { name = "get_weather" } })
    .ExecuteAsync();

// Disable tool calling
var response3 = await client.Chat
    .WithMessages(message)
    .WithTools(tools)
    .WithToolChoice("none")
    .ExecuteAsync();
```

### [`ToolChoiceExtensions`](OpenRouter/Models/Common/Tool.cs:67) Utilities

**Helper Methods for Tool Choice**
```csharp
public static class ToolChoiceExtensions
{
    public static object Auto() => "auto";
    
    public static object None() => "none";
    
    public static object Required() => "required";
    
    public static object Function(string functionName)
    {
        return new
        {
            type = "function",
            function = new { name = functionName }
        };
    }
}

// Usage
var response = await client.Chat
    .WithMessages(message)
    .WithTools(tools)
    .WithToolChoice(ToolChoiceExtensions.Function("get_weather"))
    .ExecuteAsync();
```

<!-- C# Code Example: Dynamic tool selection based on conversation context -->

## Function Execution

### Tool Call Response Handling

**Processing Tool Calls**
```csharp
public async Task<string> HandleConversationWithTools(string userMessage)
{
    var messages = new List<ChatMessage> { new("user", userMessage) };
    
    while (true)
    {
        var response = await client.Chat
            .WithModel("gpt-4")
            .WithMessages(messages)
            .WithTools(availableTools)
            .ExecuteAsync();
            
        messages.Add(response.Choices[0].Message);
        
        if (response.Choices[0].Message.ToolCalls?.Any() == true)
        {
            foreach (var toolCall in response.Choices[0].Message.ToolCalls)
            {
                var result = await ExecuteToolCall(toolCall);
                messages.Add(new ChatMessage("tool", result)
                {
                    ToolCallId = toolCall.Id
                });
            }
        }
        else
        {
            return response.Choices[0].Message.Content;
        }
    }
}
```

### Function Result Processing

**Tool Execution Framework**
```csharp
public class ToolExecutor
{
    private readonly Dictionary<string, Func<string, Task<string>>> toolRegistry;
    
    public ToolExecutor()
    {
        toolRegistry = new Dictionary<string, Func<string, Task<string>>>
        {
            ["get_weather"] = ExecuteWeatherTool,
            ["calculate"] = ExecuteCalculatorTool,
            ["query_database"] = ExecuteDatabaseTool
        };
    }
    
    public async Task<string> ExecuteToolCall(ToolCall toolCall)
    {
        try
        {
            if (toolRegistry.TryGetValue(toolCall.Function.Name, out var executor))
            {
                return await executor(toolCall.Function.Arguments);
            }
            throw new InvalidOperationException($"Tool '{toolCall.Function.Name}' not found");
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
    
    private async Task<string> ExecuteWeatherTool(string argumentsJson)
    {
        var args = JsonSerializer.Deserialize<WeatherArgs>(argumentsJson);
        var weather = await weatherService.GetWeatherAsync(args.Location, args.Unit);
        return JsonSerializer.Serialize(weather);
    }
}
```

### Multi-turn Conversations with Tools

**Conversation State Management**
```csharp
public class ConversationManager
{
    private readonly List<ChatMessage> conversationHistory = new();
    private readonly ToolExecutor toolExecutor;
    
    public async Task<string> ProcessUserMessage(string message)
    {
        conversationHistory.Add(new ChatMessage("user", message));
        
        var maxTurns = 10; // Prevent infinite loops
        var turnCount = 0;
        
        while (turnCount < maxTurns)
        {
            var response = await GetAIResponse();
            conversationHistory.Add(response.Choices[0].Message);
            
            if (response.Choices[0].Message.ToolCalls?.Any() == true)
            {
                await ProcessToolCalls(response.Choices[0].Message.ToolCalls);
                turnCount++;
            }
            else
            {
                return response.Choices[0].Message.Content;
            }
        }
        
        return "Conversation exceeded maximum turns.";
    }
    
    private async Task ProcessToolCalls(IEnumerable<ToolCall> toolCalls)
    {
        foreach (var toolCall in toolCalls)
        {
            var result = await toolExecutor.ExecuteToolCall(toolCall);
            conversationHistory.Add(new ChatMessage("tool", result)
            {
                ToolCallId = toolCall.Id
            });
        }
    }
}
```

<!-- C# Code Example: Parallel tool execution for improved performance -->

## Custom Function Integration

### .NET Method Binding to Tools

**Automatic Tool Generation from Methods**
```csharp
public class WeatherService
{
    [ToolFunction("get_weather", "Get current weather for a location")]
    public async Task<WeatherResult> GetWeatherAsync(
        [ToolParameter("location", "City and state")] string location,
        [ToolParameter("unit", "Temperature unit")] string unit = "fahrenheit")
    {
        // Implementation
        return new WeatherResult();
    }
}

public class ToolGenerator
{
    public static Tool[] GenerateTools<T>()
    {
        var type = typeof(T);
        var methods = type.GetMethods()
            .Where(m => m.GetCustomAttribute<ToolFunctionAttribute>() != null);
            
        return methods.Select(GenerateToolFromMethod).ToArray();
    }
    
    private static Tool GenerateToolFromMethod(MethodInfo method)
    {
        var attr = method.GetCustomAttribute<ToolFunctionAttribute>();
        var parameters = GenerateParameterSchema(method);
        
        return new Tool
        {
            Function = new Function
            {
                Name = attr.Name,
                Description = attr.Description,
                Parameters = parameters
            }
        };
    }
}
```

### Async Function Support

**Async Tool Execution with Cancellation**
```csharp
public class AsyncToolExecutor
{
    public async Task<string> ExecuteAsync(
        ToolCall toolCall, 
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);
            
        try
        {
            return await ExecuteToolInternal(toolCall, combinedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            return JsonSerializer.Serialize(new { error = "Tool execution timeout" });
        }
    }
    
    private async Task<string> ExecuteToolInternal(ToolCall toolCall, CancellationToken ct)
    {
        // Long-running tool execution with cancellation support
        await Task.Delay(100, ct); // Simulated work
        return "Tool result";
    }
}
```

### Error Handling in Tool Execution

**Robust Error Handling Pattern**
```csharp
public class SafeToolExecutor
{
    private readonly ILogger<SafeToolExecutor> logger;
    
    public async Task<string> ExecuteWithErrorHandling(ToolCall toolCall)
    {
        try
        {
            logger.LogInformation("Executing tool: {ToolName}", toolCall.Function.Name);
            
            var result = await ExecuteTool(toolCall);
            
            logger.LogInformation("Tool executed successfully: {ToolName}", toolCall.Function.Name);
            return result;
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid arguments for tool: {ToolName}", toolCall.Function.Name);
            return JsonSerializer.Serialize(new 
            { 
                error = "Invalid arguments", 
                details = ex.Message,
                tool = toolCall.Function.Name
            });
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "Tool execution timeout: {ToolName}", toolCall.Function.Name);
            return JsonSerializer.Serialize(new 
            { 
                error = "Execution timeout", 
                tool = toolCall.Function.Name 
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Tool execution failed: {ToolName}", toolCall.Function.Name);
            return JsonSerializer.Serialize(new 
            { 
                error = "Execution failed", 
                message = ex.Message,
                tool = toolCall.Function.Name
            });
        }
    }
}
```

<!-- C# Code Example: Dependency injection integration for tool services -->

## Advanced Tool Patterns

### Parallel Tool Execution

**Concurrent Tool Processing**
```csharp
public async Task<List<ChatMessage>> ExecuteToolsInParallel(IEnumerable<ToolCall> toolCalls)
{
    var tasks = toolCalls.Select(async toolCall =>
    {
        var result = await toolExecutor.ExecuteAsync(toolCall);
        return new ChatMessage("tool", result) { ToolCallId = toolCall.Id };
    });
    
    var results = await Task.WhenAll(tasks);
    return results.ToList();
}
```

### Tool Result Validation

**Schema Validation for Tool Results**
```csharp
public class ToolResultValidator
{
    public bool ValidateResult(string toolName, string result)
    {
        try
        {
            var schema = GetResultSchema(toolName);
            var jsonDocument = JsonDocument.Parse(result);
            return ValidateAgainstSchema(jsonDocument, schema);
        }
        catch (JsonException)
        {
            return false;
        }
    }
    
    private JsonSchema GetResultSchema(string toolName)
    {
        // Return expected schema for tool results
        return toolSchemas[toolName];
    }
}
```

### Tool Composition Strategies

**Composite Tool Workflows**
```csharp
public class CompositeToolWorkflow
{
    public async Task<string> ExecuteWorkflow(string workflow, object parameters)
    {
        var steps = workflowDefinitions[workflow];
        var context = new WorkflowContext(parameters);
        
        foreach (var step in steps)
        {
            var toolCall = new ToolCall
            {
                Function = new Function
                {
                    Name = step.ToolName,
                    Arguments = JsonSerializer.Serialize(
                        step.BuildArguments(context))
                }
            };
            
            var result = await toolExecutor.ExecuteAsync(toolCall);
            context.AddResult(step.Name, result);
        }
        
        return context.GetFinalResult();
    }
}
```

<!-- C# Code Example: Tool result caching and performance optimization -->

## Code Examples

### Complete Tool Integration Workflows

<!-- C# Code Example: End-to-end weather assistant with multiple tool integrations -->
<!-- C# Code Example: Database query tool with dynamic schema introspection -->
<!-- C# Code Example: File processing tools with streaming support -->
<!-- C# Code Example: API integration tools with OAuth authentication -->
<!-- C# Code Example: Business logic tools with transaction support -->

The comprehensive tool calling system enables sophisticated AI-driven applications that can interact with external systems, process data, and execute complex workflows while maintaining type safety and error resilience.