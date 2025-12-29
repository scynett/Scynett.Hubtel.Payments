namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

public sealed record TransactionStatusResult(
    string Status,
    string? ClientReference,
    string? TransactionId,
    string? ExternalTransactionId,
    decimal? Amount,
    decimal? Charges,
    decimal? AmountAfterCharges,
    DateTimeOffset? Date,
    string? RawResponseCode,
    string? RawMessage);