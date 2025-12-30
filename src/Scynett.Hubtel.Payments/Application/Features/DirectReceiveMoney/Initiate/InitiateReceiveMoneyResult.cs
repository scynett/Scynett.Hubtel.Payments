namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

/// <summary>
/// Result of initiating a Direct Receive Money transaction.
/// </summary>
public sealed record InitiateReceiveMoneyResult(
    string ClientReference,
    string HubtelTransactionId,
    string? ExternalTransactionId,
    string? OrderId,
    string Status,              // Pending / Success / Failed
    decimal Amount,
    decimal Charges,
    decimal AmountAfterCharges,
    decimal AmountCharged,
    string Network,
    string RawResponseCode,
    string? Message = null,
    string? Description = null,
    decimal? DeliveryFee = null);
