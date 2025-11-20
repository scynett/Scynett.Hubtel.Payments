namespace Scynett.Hubtel.Payments.Features.Status;

public sealed record CheckStatusResponse(
    string TransactionId,
    string Status,
    string Message,
    decimal Amount,
    decimal Charges,
    string CustomerMobileNumber);
