namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

internal interface IReceiveMobileMoneyService
{
    Task<ReceiveMobileMoneyGatewayResponse> InitiateReceiveMoney(
       ReceiveMobileMoneyGatewayRequest request,
       CancellationToken cancellationToken = default);
}
