namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.InitPayment;

public sealed record InitPaymentRequest(
    string CustomerName,
    string CustomerMobileNumber,
    string Channel,
    decimal Amount,
    string Description,
    string? ClientReference = null,
    string? PrimaryCallbackEndPoint = null);
