namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways;

public sealed record GatewayTransactionStatusResult(
    string ResponseCode,
    string Message,
    string ClientReference,
    string Status,
    decimal Amount,
    decimal Charges,
    decimal AmountAfterCharges,
    string? HubtelTransactionId,
    string? ExternalTransactionId,
    string? PaymentMethod,
    string? CurrencyCode,
    bool? IsFulfilled,
    DateTimeOffset? PaymentDate);
