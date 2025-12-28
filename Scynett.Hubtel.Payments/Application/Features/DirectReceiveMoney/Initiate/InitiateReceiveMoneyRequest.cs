namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

// <summary>
/// Request to initiate a receive money transaction.
// </summary>

public sealed record InitiateReceiveMoneyRequest(
    string? CustomerName,
    string CustomerMobileNumber,
    string Channel,
    decimal Amount,
    string Description,
    string ClientReference,
    string PrimaryCallbackEndPoint);
