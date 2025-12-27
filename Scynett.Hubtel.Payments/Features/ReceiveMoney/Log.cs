using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;
using Scynett.Hubtel.Payments.Logging;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

internal static partial class Log
{
    [LoggerMessage(
        EventId = HubtelEventIds.PaymentInitiating,
        Level = LogLevel.Information,
        Message = "Initiating payment for {customerName} - Amount: {amount}, Channel: {channel}")]
    internal static partial void InitiatingPayment(
        ILogger logger,
        string customerName,
        decimal amount,
        string channel);

    [LoggerMessage(
        EventId = HubtelEventIds.PaymentInitResponse,
        Level = LogLevel.Information,
        Message = "Payment init response - Code: {code}, Category: {category}, Message: {message}")]
    internal static partial void PaymentInitResponse(
        ILogger logger,
        string code,
        ResponseCategory category,
        string message);

    [LoggerMessage(
        EventId = HubtelEventIds.TransactionPending,
        Level = LogLevel.Information,
        Message = "Transaction {transactionId} added to pending store")]
    internal static partial void TransactionAddedToPendingStore(
        ILogger logger,
        string transactionId);

    [LoggerMessage(
        EventId = HubtelEventIds.PaymentInitError,
        Level = LogLevel.Error,
        Message = "Error initiating payment for {customerName}")]
    internal static partial void ErrorInitiatingPayment(
        ILogger logger,
        Exception exception,
        string customerName);

    [LoggerMessage(
        EventId = HubtelEventIds.CallbackProcessing,
        Level = LogLevel.Information,
        Message = "Processing callback for transaction {transactionId} - Status: {status}")]
    internal static partial void ProcessingCallback(
        ILogger logger,
        string transactionId,
        string status);

    [LoggerMessage(
        EventId = HubtelEventIds.CallbackProcessed,
        Level = LogLevel.Information,
        Message = "Callback decision - Code: {code}, Category: {category}, IsFinal: {isFinal}")]
    internal static partial void CallbackDecision(
        ILogger logger,
        string code,
        ResponseCategory category,
        bool isFinal);

    [LoggerMessage(
        EventId = HubtelEventIds.TransactionCompleted,
        Level = LogLevel.Information,
        Message = "Transaction {transactionId} removed from pending store")]
    internal static partial void TransactionRemovedFromPendingStore(
        ILogger logger,
        string transactionId);

    [LoggerMessage(
        EventId = HubtelEventIds.CallbackError,
        Level = LogLevel.Error,
        Message = "Error processing callback for transaction {transactionId}")]
    internal static partial void ErrorProcessingCallback(
        ILogger logger,
        Exception exception,
        string transactionId);
}
