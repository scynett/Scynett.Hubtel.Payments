namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

public sealed record InitReceiveMoneyResponse(
    string TransactionId,
    string CheckoutId,
    string Status,
    string Message);
