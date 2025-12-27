namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/// <summary>
/// Response model from Hubtel Receive Money API.
/// </summary>
public sealed record HubtelReceiveMoneyResponse(
    string ResponseCode,
    string Message,
    HubtelReceiveMoneyData? Data);

/// <summary>
/// Transaction data from Hubtel Receive Money API response.
/// </summary>
public sealed record HubtelReceiveMoneyData(
    string? TransactionId,
    string? ClientReference);
