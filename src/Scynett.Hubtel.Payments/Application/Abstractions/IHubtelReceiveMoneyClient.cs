using Refit;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

namespace Scynett.Hubtel.Payments.Application.Abstractions;

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
    Task<InitiateReceiveMoneyResult> ReceiveMobileMoneyAsync(
        [Body] InitiateReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);
}
