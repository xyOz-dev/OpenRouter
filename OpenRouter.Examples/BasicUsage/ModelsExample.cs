using Microsoft.Extensions.Logging;
using OpenRouter.Core;
using OpenRouter.Models.Requests;

namespace OpenRouter.Examples.BasicUsage;

public class ModelsExample
{
    private readonly IOpenRouterClient _client;
    private readonly ILogger<ModelsExample> _logger;

    public ModelsExample(IOpenRouterClient client, ILogger<ModelsExample> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task RunListModelsAsync()
    {
        _logger.LogInformation("Starting list models example...");

        try
        {
            _logger.LogInformation("Fetching available models...");
            var response = await _client.Models.GetModelsAsync();

            _logger.LogInformation("Found {ModelCount} available models", response.Data.Length);

            var freeModels = response.Data.Where(m => 
                m.Pricing?.Prompt == "0" && 
                m.Pricing?.Completion == "0").ToList();

            _logger.LogInformation("Free models ({Count}):", freeModels.Count);
            foreach (var model in freeModels.Take(5))
            {
                _logger.LogInformation("  {Id} - {Name}", model.Id, model.Name);
                _logger.LogInformation("    Context Length: {ContextLength:N0}", model.ContextLength);
                _logger.LogInformation("    Modality: {Modality}", model.Architecture?.Modality);
            }

            var paidModels = response.Data.Where(m => 
                m.Pricing?.Prompt != "0" || 
                m.Pricing?.Completion != "0").ToList();

            _logger.LogInformation("Premium models ({Count}) - showing top 5:", paidModels.Count);
            foreach (var model in paidModels.Take(5))
            {
                _logger.LogInformation("  {Id} - {Name}", model.Id, model.Name);
                _logger.LogInformation("    Pricing: ${Prompt}/prompt, ${Completion}/completion", 
                    model.Pricing?.Prompt, model.Pricing?.Completion);
                _logger.LogInformation("    Context Length: {ContextLength:N0}", model.ContextLength);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in list models example");
            throw;
        }
    }

    public async Task RunFilterModelsAsync()
    {
        _logger.LogInformation("Starting filter models example...");

        try
        {
            var request = new ModelsRequest
            {
                SupportedParameters = new[] { "tool_use" }
            };

            _logger.LogInformation("Fetching models that support tool use...");
            var response = await _client.Models.GetModelsAsync(request);

            _logger.LogInformation("Found {ModelCount} models with tool support", response.Data.Length);

            foreach (var model in response.Data.Take(10))
            {
                _logger.LogInformation("  {Id} - {Name}", model.Id, model.Name);
                _logger.LogInformation("    Context: {ContextLength:N0}, Modality: {Modality}", 
                    model.ContextLength, model.Architecture?.Modality);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in filter models example");
            throw;
        }
    }

    public async Task RunModelDetailsAsync()
    {
        _logger.LogInformation("Starting model details example...");

        try
        {
            var modelId = "meta-llama/llama-3.1-8b-instruct:free";
            _logger.LogInformation("Fetching details for model: {ModelId}", modelId);

            var model = await _client.Models.GetModelAsync(modelId);

            _logger.LogInformation("Model Details:");
            _logger.LogInformation("  ID: {Id}", model.Id);
            _logger.LogInformation("  Name: {Name}", model.Name);
            _logger.LogInformation("  Description: {Description}", model.Description);
            _logger.LogInformation("  Context Length: {ContextLength:N0}", model.ContextLength);
            _logger.LogInformation("  Architecture:");
            _logger.LogInformation("    Modality: {Modality}", model.Architecture?.Modality);
            _logger.LogInformation("    Tokenizer: {Tokenizer}", model.Architecture?.Tokenizer);
            _logger.LogInformation("    Instruct Type: {InstructType}", model.Architecture?.InstructType);
            _logger.LogInformation("  Pricing:");
            _logger.LogInformation("    Prompt: ${Prompt}", model.Pricing?.Prompt);
            _logger.LogInformation("    Completion: ${Completion}", model.Pricing?.Completion);
            
            if (model.TopProvider != null)
            {
                _logger.LogInformation("  Top Provider:");
                _logger.LogInformation("    Max Completion Tokens: {MaxTokens}", model.TopProvider.MaxCompletionTokens);
                _logger.LogInformation("    Is Moderated: {IsModerated}", model.TopProvider.IsModerated);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in model details example");
            throw;
        }
    }

    public async Task RunMultimodalModelsAsync()
    {
        _logger.LogInformation("Starting multimodal models example...");

        try
        {
            _logger.LogInformation("Fetching all models...");
            var response = await _client.Models.GetModelsAsync();

            var multimodalModels = response.Data
                .Where(m => m.Architecture?.Modality?.Contains("image") == true)
                .ToList();

            _logger.LogInformation("Found {Count} multimodal models:", multimodalModels.Count);

            foreach (var model in multimodalModels.Take(10))
            {
                _logger.LogInformation("  {Id} - {Name}", model.Id, model.Name);
                _logger.LogInformation("    Modality: {Modality}", model.Architecture?.Modality);
                _logger.LogInformation("    Context Length: {ContextLength:N0}", model.ContextLength);
                _logger.LogInformation("    Pricing: ${Prompt}/prompt, ${Completion}/completion", 
                    model.Pricing?.Prompt, model.Pricing?.Completion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in multimodal models example");
            throw;
        }
    }

    public async Task RunCompareModelsAsync()
    {
        _logger.LogInformation("Starting model comparison example...");

        try
        {
            var modelIds = new[] 
            { 
                "meta-llama/llama-3.1-8b-instruct:free",
                "openai/gpt-4o-mini",
                "anthropic/claude-3-haiku"
            };

            _logger.LogInformation("Comparing models:");

            foreach (var modelId in modelIds)
            {
                try
                {
                    var model = await _client.Models.GetModelAsync(modelId);
                    
                    _logger.LogInformation("  {Id}:", model.Id);
                    _logger.LogInformation("    Name: {Name}", model.Name);
                    _logger.LogInformation("    Context: {Context:N0} tokens", model.ContextLength);
                    _logger.LogInformation("    Modality: {Modality}", model.Architecture?.Modality);
                    _logger.LogInformation("    Cost: ${Prompt}/${Completion} per token", 
                        model.Pricing?.Prompt, model.Pricing?.Completion);
                    _logger.LogInformation("    Max Output: {MaxTokens} tokens", 
                        model.TopProvider?.MaxCompletionTokens);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not fetch details for {ModelId}: {Error}", modelId, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in model comparison example");
            throw;
        }
    }
}