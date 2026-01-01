using System.Text.Json.Serialization;

namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

internal record InitiateReceiveMoneyRequestDto(
    string CustomerName,
    string CustomerMsisdn,
    string CustomerEmail,
    string Channel,
    string Amount,
    [property: JsonPropertyName("PrimaryCallbackUrl")] string PrimaryCallbackUrl,
    string Description,
    string ClientReference);
