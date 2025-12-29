using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

internal static class ReceiveMoneyCallbackMapping
{
    internal static ReceiveMoneyCallbackResult ToResult(
        ReceiveMoneyCallbackRequest callback,
        HandlingDecision decision)
    {
        return new ReceiveMoneyCallbackResult(
            ClientReference: callback.Data.ClientReference,
            TransactionId: callback.Data.TransactionId,
            ResponseCode: callback.ResponseCode,
            Category: decision.Category,
            NextAction: decision.NextAction,
            IsFinal: decision.IsFinal,
            IsSuccess: decision.IsSuccess,
            CustomerMessage: decision.CustomerMessage,
            RawMessage: callback.Message);
    }

    internal static string BuildDecisionMessage(ReceiveMoneyCallbackRequest callback)
    {
        // Hubtel often puts the most useful “variant text” in Data.Description.
        // Use Message + Description to refine 2001 variants.
        var msg = callback.Message?.Trim();
        var desc = callback.Data.Description?.Trim();

        return string.IsNullOrWhiteSpace(desc)
            ? (msg ?? string.Empty)
            : string.IsNullOrWhiteSpace(msg)
                ? desc
                : $"{msg}. {desc}";
    }
}