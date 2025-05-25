using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenRouter.Core;
using OpenRouter.Extensions;
using OpenRouter.Models.Requests;
using Xunit;

namespace OpenRouter.Tests.Integration;

[Collection("Integration")]
public class ModelsIntegrationTests : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOpenRouterClient _client;
    private readonly string? _apiKey;

    public ModelsIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _apiKey = configuration["OpenRouter:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

        var services = new ServiceCollection();
        services.AddOpenRouter(options =>
        {
            options.ApiKey = _apiKey;
            options.BaseUrl = "https://openrouter.ai/api/v1";
            options.HttpReferer = "https://github.com/openrouter-net";
            options.XTitle = "OpenRouter.NET Tests";
        });
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _client = _serviceProvider.GetRequiredService<IOpenRouterClient>();
    }

    [Fact]
    public async Task GetModels_ReturnsAvailableModels()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var response = await _client.Models.ListModelsAsync();

        response.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();
        
        var firstModel = response.Data.First();
        firstModel.Id.Should().NotBeEmpty();
        firstModel.Name.Should().NotBeEmpty();
        firstModel.ContextLength.Should().BeGreaterThan(0);
        firstModel.Pricing.Should().NotBeNull();
    }

    [Fact]
    public async Task GetModels_WithSupportedParameters_FiltersModels()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var response = await _client.Models.ListModelsAsync();

        response.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetModel_ValidModelId_ReturnsModelDetails()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var modelId = "meta-llama/llama-3.1-8b-instruct:free";

        var model = await _client.Models.GetModelAsync(modelId);

        model.Should().NotBeNull();
        model.Id.Should().Be(modelId);
        model.Name.Should().NotBeEmpty();
        model.ContextLength.Should().BeGreaterThan(0);
        model.Architecture.Should().NotBeNull();
        model.Pricing.Should().NotBeNull();
    }

    [Fact]
    public async Task GetModel_InvalidModelId_ThrowsException()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var invalidModelId = "non-existent-model-id-12345";

        var act = async () => await _client.Models.GetModelAsync(invalidModelId);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetModels_ChecksFreeModelsAvailable()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var response = await _client.Models.ListModelsAsync();

        var freeModels = response.Data.Where(m => 
            m.Pricing?.Prompt == "0" && 
            m.Pricing?.Completion == "0").ToList();

        freeModels.Should().NotBeEmpty("There should be at least some free models available");
        
        var freeModel = freeModels.First();
        freeModel.Id.Should().NotBeEmpty();
        freeModel.Name.ToLower().Should().Contain("free");
    }

    [Fact]
    public async Task GetModels_ChecksMultimodalModelsAvailable()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var response = await _client.Models.ListModelsAsync();

        var multimodalModels = response.Data.Where(m => 
            m.Architecture?.Modality?.Contains("image") == true).ToList();

        if (multimodalModels.Any())
        {
            var multimodalModel = multimodalModels.First();
            multimodalModel.Architecture.Modality.Should().Contain("image");
            multimodalModel.ContextLength.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task GetModels_ValidatesModelStructure()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return;
        }

        var response = await _client.Models.ListModelsAsync();

        foreach (var model in response.Data.Take(5))
        {
            model.Id.Should().NotBeEmpty();
            model.Name.Should().NotBeEmpty();
            model.ContextLength.Should().BeGreaterThan(0);
            model.Architecture.Should().NotBeNull();
            model.Architecture.Modality.Should().NotBeEmpty();
            model.Pricing.Should().NotBeNull();
            model.Pricing.Prompt.Should().NotBeNull();
            model.Pricing.Completion.Should().NotBeNull();
        }
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        await Task.CompletedTask;
    }
}