namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;

/// <summary>
/// Normalized result returned by the Hubtel gateway
/// after initiating a Direct Receive Money transaction.
/// </summary>
/// <remarks>
/// <c>HttpStatusCode</c> carries the HTTP status Hubtel actually returned when the call did not produce a
/// usable payload (i.e. when <c>ResponseCode</c> is <c>HTTP_ERROR</c>). It is <see langword="null"/> on a
/// normal (2xx, parsed) response, and on a pure transport failure where no response was ever received.
/// It was added so that the HTTP status is never silently lost.
/// </remarks>
public sealed record GatewayInitiateReceiveMoneyResult(
    string ResponseCode,
    string? Message,
    string? TransactionId,
    string? ExternalReference = null,
    string? ExternalTransactionId = null,
    string? OrderId = null,
    string? Description = null,
    decimal? Amount = null,
    decimal? Charges = null,
    decimal? AmountAfterCharges = null,
    decimal? AmountCharged = null,
    decimal? DeliveryFee = null,
    int? HttpStatusCode = null
);
