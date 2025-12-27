using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Models;

namespace Scynett.Hubtel.Payments.AspNetCore.Workers;

internal static partial class Log
{
    [LoggerMessage(
        EventId = HubtelEventIds.WorkerStarted,
        Level = LogLevel.Information,
        Message = "Pending Transactions Worker started")]
    internal static partial void WorkerStarted(ILogger logger);

    [LoggerMessage(
        EventId = HubtelEventIds.WorkerStopped,
        Level = LogLevel.Information,
        Message = "Pending Transactions Worker stopped")]
    internal static partial void WorkerStopped(ILogger logger);

    [LoggerMessage(
        EventId = HubtelEventIds.WorkerError,
        Level = LogLevel.Error,
        Message = "Error occurred while checking pending transactions")]
    internal static partial void ErrorCheckingPendingTransactions(
        ILogger logger,
        Exception exception);

    [LoggerMessage(
        EventId = HubtelEventIds.WorkerNoPendingTransactions,
        Level = LogLevel.Debug,
        Message = "No pending transactions to check")]
    internal static partial void NoPendingTransactions(ILogger logger);

    [LoggerMessage(
        EventId = HubtelEventIds.WorkerCheckingTransactions,
        Level = LogLevel.Information,
        Message = "Checking {count} pending transactions")]
    internal static partial void CheckingPendingTransactions(
        ILogger logger,
        int count);

    [LoggerMessage(
        EventId = HubtelEventIds.WorkerTransactionCheckFailed,
        Level = LogLevel.Warning,
        Message = "Failed to check status for transaction {transactionId}: {error}")]
    internal static partial void FailedToCheckTransactionStatus(
        ILogger logger,
        string transactionId,
        string error);

    [LoggerMessage(
        EventId = HubtelEventIds.TransactionCompleted,
        Level = LogLevel.Information,
        Message = "Transaction {transactionId} completed with status: {status}")]
    internal static partial void TransactionCompleted(
        ILogger logger,
        string transactionId,
        string status);

    [LoggerMessage(
        EventId = HubtelEventIds.WorkerTransactionError,
        Level = LogLevel.Error,
        Message = "Error checking transaction {transactionId}")]
    internal static partial void ErrorCheckingTransaction(
        ILogger logger,
        Exception exception,
        string transactionId);
}
