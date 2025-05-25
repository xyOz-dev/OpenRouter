using FluentAssertions;
using OpenRouter.Authentication;
using OpenRouter.Exceptions;
using Xunit;

namespace OpenRouter.Tests.Unit.Authentication;

public class ApiKeyProviderTests
{
    [Fact]
    public void Constructor_ValidApiKey_CreatesProvider()
    {
        var apiKey = "sk-or-v1-test-key";
        var provider = new ApiKeyProvider(apiKey);

        provider.IsValid.Should().BeTrue();
        provider.AuthenticationScheme.Should().Be("X-API-Key");
        provider.CanRefresh.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_InvalidApiKey_ThrowsArgumentException(string? apiKey)
    {
        var act = () => new ApiKeyProvider(apiKey!);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*API key*");
    }

    [Fact]
    public async Task GetAuthHeaderAsync_ValidProvider_ReturnsApiKey()
    {
        var apiKey = "sk-or-v1-test-key";
        var provider = new ApiKeyProvider(apiKey);

        var result = await provider.GetAuthHeaderAsync();

        result.Should().Be(apiKey);
    }

    [Fact]
    public async Task GetAuthHeaderAsync_WithCancellation_CompletesSuccessfully()
    {
        var apiKey = "sk-or-v1-test-key";
        var provider = new ApiKeyProvider(apiKey);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await provider.GetAuthHeaderAsync(cts.Token);

        result.Should().Be(apiKey);
    }

    [Theory]
    [InlineData("sk-or-v1-abc123")]
    [InlineData("sk-or-v1-xyz789")]
    [InlineData("custom-api-key-format")]
    public async Task GetAuthHeaderAsync_DifferentKeyFormats_ReturnsCorrectKey(string apiKey)
    {
        var provider = new ApiKeyProvider(apiKey);

        var result = await provider.GetAuthHeaderAsync();

        result.Should().Be(apiKey);
    }

    [Fact]
    public async Task GetAuthHeaderAsync_MultipleCallsSameInstance_ReturnsConsistentResults()
    {
        var apiKey = "sk-or-v1-consistent-key";
        var provider = new ApiKeyProvider(apiKey);

        var result1 = await provider.GetAuthHeaderAsync();
        var result2 = await provider.GetAuthHeaderAsync();

        result1.Should().Be(apiKey);
        result2.Should().Be(apiKey);
        result1.Should().Be(result2);
    }

    [Fact]
    public void IsValid_ValidApiKey_ReturnsTrue()
    {
        var provider = new ApiKeyProvider("valid-key");
        provider.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CanRefresh_ApiKeyProvider_ReturnsFalse()
    {
        var provider = new ApiKeyProvider("test-key");
        provider.CanRefresh.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshAsync_ApiKeyProvider_ThrowsNotSupportedException()
    {
        var provider = new ApiKeyProvider("test-key");

        var act = async () => await provider.RefreshAsync();

        await act.Should().ThrowAsync<NotSupportedException>();
    }
}