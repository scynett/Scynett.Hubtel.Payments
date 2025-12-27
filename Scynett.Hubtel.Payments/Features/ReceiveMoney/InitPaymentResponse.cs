namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

public sealed record InitPaymentResponse(
    string TransactionId,
    string CheckoutId,
    string Status,
    string Message);
