namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

public sealed record TransactionStatusResult(
    string Status,
    string? ClientReference,
    string? TransactionId,
    string? ExternalTransactionId,
    string? PaymentMethod,
    string? CurrencyCode,
    bool? IsFulfilled,
    decimal? Amount,
    decimal? Charges,
    decimal? AmountAfterCharges,
    DateTimeOffset? Date,
    string? RawResponseCode,
    string? RawMessage);
