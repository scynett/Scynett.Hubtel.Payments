namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;

/// <summary>
/// Normalized request for initiating a Direct Receive Money transaction
/// through the Hubtel gateway.
/// </summary>
public sealed record GatewayInitiateReceiveMoneyRequest(
    string CustomerName,
    string PosSalesId,
    string CustomerMsisdn,
    string CustomeeEmail,
    string Channel,
    string Amount,
#pragma warning disable CA1054 // URI-like parameters should not be strings
#pragma warning disable CA1056 // URI-like properties should not be strings
    string CallbackUrl,
#pragma warning restore CA1056 // URI-like properties should not be strings
#pragma warning restore CA1054 // URI-like parameters should not be strings
    string Description,
    string ClientReference
);