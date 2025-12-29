using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Common;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

internal static partial class ReceiveMoneyCallbackLogMessages
{
    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackReceived,
        Level = LogLevel.Information,
        Message = "Hubtel callback received. ClientReference={ClientReference}, TransactionId={TransactionId}, ResponseCode={ResponseCode}")]
    public static partial void CallbackReceived(
        ILogger logger,
        string clientReference,
        string transactionId,
        string responseCode);

    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackDecision,
        Level = LogLevel.Information,
        Message = "Hubtel callback decision. Code={Code}, Category={Category}, IsFinal={IsFinal}, NextAction={NextAction}")]
    public static partial void CallbackDecision(
        ILogger logger,
        string code,
        string category,
        bool isFinal,
        string nextAction);

    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackPendingRemoved,
        Level = LogLevel.Information,
        Message = "Pending transaction removed. TransactionId={TransactionId}, ClientReference={ClientReference}")]
    public static partial void PendingRemoved(
        ILogger logger,
        string transactionId,
        string clientReference);

    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackValidationFailed,
        Level = LogLevel.Warning,
        Message = "Hubtel callback validation failed. TransactionId={TransactionId}, ClientReference={ClientReference}")]
    public static partial void ValidationFailed(
        ILogger logger,
        string transactionId,
        string clientReference);

    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackProcessingFailed,
        Level = LogLevel.Error,
        Message = "Hubtel callback processing failed. TransactionId={TransactionId}, ClientReference={ClientReference}")]
    public static partial void ProcessingFailed(
        ILogger logger,
        Exception exception,
        string transactionId,
        string clientReference);
}