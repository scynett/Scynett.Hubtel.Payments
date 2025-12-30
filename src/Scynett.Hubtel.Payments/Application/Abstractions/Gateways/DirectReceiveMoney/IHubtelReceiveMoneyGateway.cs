namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;


/// <summary>
/// Application boundary for Hubtel Direct Receive Money.
/// </summary>
internal interface IHubtelReceiveMoneyGateway
{
    Task<GatewayInitiateReceiveMoneyResult> InitiateAsync(
        GatewayInitiateReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);
}
