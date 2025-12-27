namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

/// <summary>
/// Request to initiate a receive money transaction.
/// </summary>
public sealed record ReceiveMoneyRequest(
    string? CustomerName,
    string CustomerMobileNumber,
    string Channel,
    decimal Amount,
    string Description,
    string ClientReference,
    string PrimaryCallbackEndPoint);
