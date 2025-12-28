namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;

/// <summary>
/// Normalized result returned by the Hubtel gateway
/// after initiating a Direct Receive Money transaction.
/// </summary>
public sealed record GatewayInitiateReceiveMoneyResult(
    string ResponseCode,
    string? Message,
    string? TransactionId,
    string? ExternalReference = null
);