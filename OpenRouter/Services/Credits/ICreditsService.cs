using OpenRouter.Models.Requests;
using OpenRouter.Models.Responses;

namespace OpenRouter.Services.Credits;

public interface ICreditsService
{
    Task<CreditsResponse> GetCreditsAsync(CancellationToken cancellationToken = default);
    Task<CoinbasePaymentResponse> CreateCoinbasePaymentAsync(CoinbasePaymentRequest request, CancellationToken cancellationToken = default);
    Task<UsageResponse> GetUsageAsync(CancellationToken cancellationToken = default);
}