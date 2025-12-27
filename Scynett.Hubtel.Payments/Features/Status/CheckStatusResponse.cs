namespace Scynett.Hubtel.Payments.Features.TransactionStatus;

/// <summary>
/// Result of a transaction status check.
/// </summary>
public sealed record TransactionStatusResult(
    string TransactionId,
    string Status,
    string Message,
    decimal Amount,
    decimal Charges,
    string CustomerMobileNumber);
