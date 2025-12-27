namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/// <summary>
/// Internal service interface for Hubtel receive money gateway operations.
/// </summary>
internal interface IReceiveMobileMoneyService
{
    /// <summary>
    /// Initiates a receive money transaction via Hubtel gateway.
    /// </summary>
    Task<HubtelReceiveMoneyResponse> InitiateReceiveMoney(
       HubtelReceiveMoneyRequest request,
       CancellationToken cancellationToken = default);
}
