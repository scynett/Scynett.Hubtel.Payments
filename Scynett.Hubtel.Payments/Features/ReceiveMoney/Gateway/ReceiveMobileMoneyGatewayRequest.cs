namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/// <summary>
/// Request model for Hubtel Receive Money API.
/// </summary>
public sealed record HubtelReceiveMoneyRequest(
    string CustomerName,
    string CustomerMsisdn,
    string CustomerEmail,
    string Channel,
    string Amount,
    string PrimaryCallbackEndpoint,
    string Description,
    string ClientReference);