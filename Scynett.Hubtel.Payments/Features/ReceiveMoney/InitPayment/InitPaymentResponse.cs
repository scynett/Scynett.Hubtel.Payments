namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.InitPayment;

public sealed record InitPaymentResponse(
    string TransactionId,
    string CheckoutId,
    string Status,
    string Message);
