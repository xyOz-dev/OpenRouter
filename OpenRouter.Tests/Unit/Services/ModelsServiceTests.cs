using FluentAssertions;
using Moq;
using OpenRouter.Core;
using OpenRouter.Models.Responses;
using OpenRouter.Tests.TestHelpers;
using Xunit;

namespace OpenRouter.Tests.Unit.Services;

public class ModelsServiceTests : IDisposable
{
    private readonly Mock<IOpenRouterClient> _mockClient;
    private readonly MockHttpClientFactory _mockHttpFactory;

    public ModelsServiceTests()
    {
        _mockClient = new Mock<IOpenRouterClient>();
        _mockHttpFactory = new MockHttpClientFactory();
    }

    [Fact]
    public async Task ListModelsAsync_ReturnsAllModels()
    {
        var expectedResponse = TestDataHelper.LoadTestData<ModelResponse>("ModelsResponse.json");
        
        _mockClient.Setup(x => x.Models.ListModelsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _mockClient.Object.Models.ListModelsAsync();

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data[0].Id.Should().Be("meta-llama/llama-3.1-8b-instruct:free");
        result.Data[1].Id.Should().Be("openai/gpt-4o");

        _mockClient.Verify(x => x.Models.ListModelsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetModelAsync_ValidModelId_ReturnsModelDetails()
    {
        var modelId = "openai/gpt-4o";
        var expectedModel = new ModelDetailsResponse
        {
            Id = modelId,
            Name = "OpenAI GPT-4o",
            Description = "GPT-4o: multimodal flagship model",
            ContextLength = 128000,
            Architecture = new ModelArchitecture
            {
                Modality = "text+image",
                Tokenizer = "cl100k_base"
            },
            Pricing = new ModelPricing
            {
                Prompt = "0.000005",
                Completion = "0.000015"
            }
        };

        _mockClient.Setup(x => x.Models.GetModelAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedModel);

        var result = await _mockClient.Object.Models.GetModelAsync(modelId);

        result.Should().NotBeNull();
        result.Id.Should().Be(modelId);
        result.Name.Should().Be("OpenAI GPT-4o");
        result.ContextLength.Should().Be(128000);
        result.Architecture?.Modality.Should().Be("text+image");

        _mockClient.Verify(x => x.Models.GetModelAsync(modelId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListEndpointsAsync_ReturnsEndpoints()
    {
        var expectedEndpoints = new[] { "openai", "anthropic", "meta-llama" };

        _mockClient.Setup(x => x.Models.ListEndpointsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEndpoints);

        var result = await _mockClient.Object.Models.ListEndpointsAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain("openai");

        _mockClient.Verify(x => x.Models.ListEndpointsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetModelAsync_WithCancellation_PropagatesCancellationToken()
    {
        var modelId = "test-model";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockClient.Setup(x => x.Models.GetModelAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = async () => await _mockClient.Object.Models.GetModelAsync(modelId, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ListModelsAsync_EmptyResponse_ReturnsEmptyList()
    {
        var expectedResponse = new ModelResponse { Data = Array.Empty<Model>() };

        _mockClient.Setup(x => x.Models.ListModelsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _mockClient.Object.Models.ListModelsAsync();

        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    public void Dispose()
    {
        _mockHttpFactory.Dispose();
    }
}