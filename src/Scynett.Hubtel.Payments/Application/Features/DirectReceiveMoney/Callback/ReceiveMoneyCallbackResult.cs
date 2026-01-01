using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public sealed record ReceiveMoneyCallbackResult(
    string ClientReference,
    string TransactionId,
    string ResponseCode,
    ResponseCategory Category,
    NextAction NextAction,
    bool IsFinal,
    bool IsSuccess,
    string? CustomerMessage,
    string? RawMessage,
    decimal Amount = 0m,
    decimal? Charges = null,
    decimal? AmountAfterCharges = null,
    decimal? AmountCharged = null,
    string? Description = null,
    string? ExternalTransactionId = null,
    string? OrderId = null,
    DateTimeOffset? PaymentDate = null
);
