namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

public sealed record TransactionStatusResult(
    string ClientReference,
    string Status,
    decimal Amount,
    decimal Charges,
    decimal AmountAfterCharges,
    string? HubtelTransactionId,
    string? ExternalTransactionId,
    string PaymentMethod,
    DateTimeOffset? PaymentDate);