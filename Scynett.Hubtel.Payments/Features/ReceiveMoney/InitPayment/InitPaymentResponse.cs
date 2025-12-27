namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

/// <summary>
/// Result of a receive money transaction initiation.
/// </summary>
public sealed record ReceiveMoneyResult(
    string TransactionId,
    string CheckoutId,
    string Status,
    string Message);
