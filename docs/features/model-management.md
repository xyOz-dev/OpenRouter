# Feature: Managing Models

The OpenRouter.NET SDK allows you to interact with the OpenRouter API to retrieve information about available AI models. This is essential for selecting appropriate models for your tasks and understanding their capabilities.

The primary service for this functionality is [`IModelsService`](../../OpenRouter/Services/Models/IModelsService.cs:1), accessed via `openRouterClient.Models`.

## Listing Available Models

You can fetch a list of all models accessible through your OpenRouter account. Each model entry provides details like its ID, name, pricing, context length, and more.

**Steps:**
1.  Obtain an instance of `IModelsService` (or `IOpenRouterClient`).
2.  Call `await _modelsService.GetModelsAsync()`.
3.  Process the [`ModelResponse`](../../OpenRouter/Models/Responses/ModelResponse.cs:1) (which typically contains a `Data` property which is a list of model details). Note: The actual response object might be a wrapper like `ModelsListResponse` if the API returns a structured list. Based on `OpenRouterClient.cs`, `GetModelsAsync` returns `Task<ModelsResponse?>`, and `ModelsResponse` has a `Data` property which is `List<Model>`. Let's assume `Model` is the detailed model object.

**Example:**
(Adapted from [`ModelsExample.cs`](../../OpenRouter.Examples/BasicUsage/ModelsExample.cs:1))

```csharp
using OpenRouter.Services.Models;
using OpenRouter.Models.Responses; // For ModelResponse and nested Model class
using Microsoft.Extensions.Logging;
using System.Linq;

public class ModelManagementExample
{
    private readonly IModelsService _modelsService;
    private readonly ILogger<ModelManagementExample> _logger;

    public ModelManagementExample(IModelsService modelsService, ILogger<ModelManagementExample> logger)
    {
        _modelsService = modelsService;
        _logger = logger;
    }

    public async Task RunListModelsAsync()
    {
        _logger.LogInformation("--- List Models Example ---");
        try
        {
            var modelsResponse = await _modelsService.GetModelsAsync(); // This returns ModelsResponse
            
            if (modelsResponse?.Data != null && modelsResponse.Data.Any())
            {
                _logger.LogInformation("Found {Count} models:", modelsResponse.Data.Count);
                foreach (var model in modelsResponse.Data.Take(10)) // Displaying first 10 for brevity
                {
                    _logger.LogInformation("ID: {Id}, Name: {Name}, Context: {ContextLength} tokens", 
                        model.Id, model.Name, model.ContextLength);
                }
                if (modelsResponse.Data.Count > 10)
                {
                    _logger.LogInformation("...and {MoreCount} more models.", modelsResponse.Data.Count - 10);
                }
            }
            else
            {
                _logger.LogWarning("No models found or error fetching models.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing models");
        }
    }
}
```
*Review `OpenRouter.Models.Responses.ModelsResponse` and the nested `Model` class (if it exists, or if `ModelResponse` itself is the item in the list) for accurate property names like `ContextLength`, `Name`, `Id`.* The `ModelResponse` class itself contains the `Data` list of `Model` objects. The `Model` class (likely named `ModelDetails` or similar within the SDK, or just `Model` if it's `OpenRouter.Models.Responses.Model`) would have these properties. Let's assume it is `OpenRouter.Models.Responses.Model` based on the project structure.

## Retrieving Model Details

The general `GetModelsAsync()` call returns a list of models, each with comprehensive details. If you need details for a specific model and already have its ID, you would typically find it within the list returned by `GetModelsAsync()`.

The OpenRouter API might offer an endpoint to query a single model by ID. If so, the SDK would expose a method like `_modelsService.GetModelDetailsAsync(string modelId)`.
Currently, the `OpenRouter.Examples/BasicUsage/ModelsExample.cs` shows filtering the already fetched list or using the `ModelId` property of the models from the list.

**Example (Filtering from existing list):**
(Adapted from [`ModelsExample.cs`](../../OpenRouter.Examples/BasicUsage/ModelsExample.cs:1))

```csharp
public async Task RunModelDetailsAsync(string modelIdToFind = "openai/gpt-3.5-turbo")
{
    _logger.LogInformation("--- Model Details Example for {ModelId} ---", modelIdToFind);
    try
    {
        var modelsResponse = await _modelsService.GetModelsAsync();
        var model = modelsResponse?.Data?.FirstOrDefault(m => m.Id == modelIdToFind);

        if (model != null)
        {
            _logger.LogInformation("Details for Model ID: {Id}", model.Id);
            _logger.LogInformation("  Name: {Name}", model.Name);
            _logger.LogInformation("  Description: {Description}", model.Description);
            _logger.LogInformation("  Context Length: {ContextLength} tokens", model.ContextLength);
            _logger.LogInformation("  Pricing (Prompt): ${PromptPrice}/1M tokens", model.Pricing?.Prompt);
            _logger.LogInformation("  Pricing (Completion): ${CompletionPrice}/1M tokens", model.Pricing?.Completion);
            // Add more properties as needed, e.g., model.Architecture?.Tokenizer
        }
        else
        {
            _logger.LogWarning("Model with ID '{ModelId}' not found.", modelIdToFind);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting model details for {ModelId}", modelIdToFind);
    }
}
```
*Note: The `Pricing` object structure (e.g., `model.Pricing.Prompt`) needs to match the actual class definition in [`ModelResponse`](../../OpenRouter/Models/Responses/ModelResponse.cs:1) (or the nested `Model` class).*

## Filtering Models (Client-Side)

The OpenRouter API might not offer server-side filtering for the models list beyond basic parameters (like "updated_after"). Therefore, filtering is often done client-side after fetching the full list.

**Example (Client-side filtering for models supporting a certain context length):**
(Adapted from [`ModelsExample.cs`](../../OpenRouter.Examples/BasicUsage/ModelsExample.cs:1))
```csharp
public async Task RunFilterModelsAsync(int minContextLength = 16000)
{
    _logger.LogInformation("--- Filter Models Example (Min Context: {Length}) ---", minContextLength);
    try
    {
        var modelsResponse = await _modelsService.GetModelsAsync();
        var filteredModels = modelsResponse?.Data?
            .Where(m => m.ContextLength >= minContextLength)
            .OrderBy(m => m.Id)
            .ToList();

        if (filteredModels != null && filteredModels.Any())
        {
            _logger.LogInformation("Found {Count} models with at least {Length} tokens context window:",
                filteredModels.Count, minContextLength);
            foreach (var model in filteredModels)
            {
                _logger.LogInformation("  ID: {Id}, Name: {Name}, Context: {ContextLength}",
                    model.Id, model.Name, model.ContextLength);
            }
        }
        else
        {
            _logger.LogWarning("No models found matching the criteria.");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error filtering models");
    }
}
```

## Understanding Model Properties

The `Model` object (within `ModelsResponse.Data`) contains various properties. Key ones include:
*   `Id` (string): The unique identifier for the model, used in API requests (e.g., "openai/gpt-4o").
*   `Name` (string): A human-readable name (e.g., "GPT-4o").
*   `Description` (string): A brief description of the model.
*   `Pricing`: An object detailing the cost per token (or per million tokens) for prompts and completions.
    *   Example: `Pricing.Prompt`, `Pricing.Completion`.
*   `ContextLength` (int?): The maximum number of tokens the model can process in a single request (prompt + completion).
*   `Architecture`: Information about the model's architecture, possibly including tokenizer details.
    *   Example: `Architecture.Tokenizer`, `Architecture.InstructType`.
*   `TopProvider`: Information about the original provider of the model.
*   `PerRequestLimits`: Rate limits or other request-specific limitations.

Consult the [`ModelResponse`](../../OpenRouter/Models/Responses/ModelResponse.cs:1) class (and its nested `Model` class if applicable) and the official OpenRouter API documentation for a definitive list of all properties and their meanings.

## Next Steps
*   [Chat Completions](chat-completions.md) (to use the models you've selected)
*   [Authentication](authentication.md)
*   [Error Handling](error-handling.md)