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
    string? RawMessage
);