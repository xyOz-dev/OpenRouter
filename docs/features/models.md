# Model Management

## Models Service Overview

The [`IModelsService`](../../OpenRouter/Core/IOpenRouterClient.cs:12) interface provides comprehensive model discovery and management capabilities, allowing applications to dynamically discover available models, their capabilities, and pricing information.

### Available Model Operations

Access model operations through the OpenRouter client:

```csharp
var modelsService = client.Models;
var models = await modelsService.GetModelsAsync();
```

## Listing Available Models

### Retrieving All Models

Get a complete list of available models:

```csharp
var allModels = await client.Models.GetModelsAsync();
foreach (var model in allModels.Data)
{
    Console.WriteLine($"{model.Id} - {model.Name}");
}
```

### Filtering and Searching Models

Filter models based on specific criteria:

<!-- C# Code Example: Filtering models by provider, capabilities, or pricing -->

### Model Capabilities and Pricing Information

Access detailed model information including capabilities and costs:

<!-- C# Code Example: Examining model capabilities, context windows, and pricing -->

## Model Details

### ModelDetailsRequest Usage

Retrieve specific model details using [`ModelDetailsRequest`](../../OpenRouter/Models/Requests/ModelDetailsRequest.cs:1):

```csharp
var modelDetails = await client.Models.GetModelDetailsAsync("anthropic/claude-3-haiku");
```

### Model Specifications

Access comprehensive model specifications:

<!-- C# Code Example: Accessing model specifications, parameters, and limits -->

### Context Windows

Understanding model context window limitations:

<!-- C# Code Example: Context window management and token counting -->

### Pricing

Model pricing information and cost calculation:

<!-- C# Code Example: Cost calculation based on input/output tokens -->

## Model Selection

### Choosing Appropriate Models for Use Cases

Guidelines for selecting models based on requirements:

```csharp
// For code generation
var codeModel = "anthropic/claude-3-haiku";

// For creative writing
var creativeModel = "anthropic/claude-3-opus";

// For analysis tasks
var analyticsModel = "openai/gpt-4-turbo";
```

### Performance vs Cost Considerations

Balance performance requirements with cost constraints:

<!-- C# Code Example: Cost-performance analysis and model selection logic -->

## Provider Routing

### WithProviderRouting() Configuration

Configure provider preferences using [`WithProviderRouting()`](../../OpenRouter/Services/Chat/IChatRequestBuilder.cs:17):

```csharp
var response = await client.Chat.CreateRequest()
    .WithModel("anthropic/claude-3-haiku")
    .WithProviderRouting(routing =>
    {
        routing.PreferredProviders = new[] { "anthropic", "openai" };
        routing.AllowFallback = true;
    })
    .WithUserMessage("Hello, world!")
    .ExecuteAsync();
```

### Provider Preferences and Fallback Strategies

Implement robust provider routing with fallback options:

<!-- C# Code Example: Complex provider routing with multiple fallback strategies -->

## Advanced Model Operations

### Model Availability Checking

Check model availability before making requests:

<!-- C# Code Example: Model availability verification -->

### Model Performance Monitoring

Monitor model performance and response times:

<!-- C# Code Example: Performance tracking and model benchmarking -->

### Dynamic Model Selection

Select models dynamically based on request characteristics:

<!-- C# Code Example: Dynamic model selection based on content type and complexity -->

## Code Examples

### Model Discovery and Selection Patterns

Complete workflow for discovering and selecting appropriate models:

<!-- C# Code Example: Complete model discovery workflow -->

### Cost-Optimized Model Selection

Implement cost-aware model selection:

<!-- C# Code Example: Cost optimization strategies for model selection -->

### Multi-Model Strategies

Use multiple models for different parts of a workflow:

<!-- C# Code Example: Multi-model workflow with different models for different tasks -->

## Best Practices

### Model Caching

Cache model information for better performance:

<!-- C# Code Example: Model information caching strategies -->

### Error Handling

Handle model-related errors gracefully:

<!-- C# Code Example: Model availability error handling and fallback -->

### Rate Limiting

Respect model-specific rate limits:

<!-- C# Code Example: Rate limiting implementation for different models -->