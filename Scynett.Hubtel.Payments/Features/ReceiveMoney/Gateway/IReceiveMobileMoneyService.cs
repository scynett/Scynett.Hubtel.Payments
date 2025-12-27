namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

internal interface IReceiveMobileMoneyService
{
    Task<ReceiveMobileMoneyResponse> InitiateReceiveMoney(
       ReceiveMobileMoneyRequest request,
       CancellationToken ct);
}
