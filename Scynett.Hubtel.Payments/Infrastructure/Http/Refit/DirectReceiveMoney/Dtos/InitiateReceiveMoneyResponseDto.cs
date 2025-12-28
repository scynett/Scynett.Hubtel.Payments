namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;


/// <summary>
/// Response model from Hubtel Receive Money API.
/// </summary>
public sealed record InitiateReceiveMoneyResponseDto(
    string ResponseCode,
    string Message,
    HubtelReceiveMoneyData? Data);

/// <summary>
/// Transaction data from Hubtel Receive Money API response.
/// </summary>
public sealed record HubtelReceiveMoneyData(
    string? TransactionId,
    string? ClientReference);
