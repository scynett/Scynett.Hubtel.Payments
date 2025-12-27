using Refit;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/// <summary>
/// Refit client for Hubtel Receive Money API.
/// </summary>
public interface IHubtelReceiveMoneyClient
{
    /// <summary>
    /// Initiates a receive money transaction.
    /// </summary>
    /// <param name="request">The receive money request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from Hubtel API.</returns>
    [Post("/receive/mobilemoney")]
    Task<HubtelReceiveMoneyResponse> ReceiveMobileMoneyAsync(
        [Body] HubtelReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);
}
