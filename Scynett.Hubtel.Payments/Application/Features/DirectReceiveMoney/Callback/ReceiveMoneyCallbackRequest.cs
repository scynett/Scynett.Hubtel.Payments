using System.Text.Json.Serialization;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public sealed record ReceiveMoneyCallbackRequest(
    [property: JsonPropertyName("ResponseCode")] string ResponseCode,
    [property: JsonPropertyName("Message")] string? Message,
    [property: JsonPropertyName("Data")] ReceiveMoneyCallbackData Data);

public sealed record ReceiveMoneyCallbackData(
    [property: JsonPropertyName("Amount")] decimal Amount,
    [property: JsonPropertyName("Charges")] decimal? Charges,
    [property: JsonPropertyName("AmountAfterCharges")] decimal? AmountAfterCharges,
    [property: JsonPropertyName("AmountCharged")] decimal? AmountCharged,
    [property: JsonPropertyName("Description")] string? Description,

    [property: JsonPropertyName("ClientReference")] string ClientReference,
    [property: JsonPropertyName("TransactionId")] string TransactionId,
    [property: JsonPropertyName("ExternalTransactionId")] string? ExternalTransactionId,
    [property: JsonPropertyName("OrderId")] string? OrderId,
    [property: JsonPropertyName("PaymentDate")] DateTimeOffset? PaymentDate
);