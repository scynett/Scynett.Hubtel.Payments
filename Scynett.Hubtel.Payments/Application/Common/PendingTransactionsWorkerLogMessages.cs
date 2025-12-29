using Microsoft.Extensions.Logging;

namespace Scynett.Hubtel.Payments.Application.Common;

internal static partial class PendingTransactionsWorkerLogMessages
{
    [LoggerMessage(EventId = 7701, Level = LogLevel.Information,
        Message = "PendingTransactionsWorker started. PollInterval={PollInterval}")]
    public static partial void Started(ILogger logger, TimeSpan pollInterval);

    [LoggerMessage(EventId = 7702, Level = LogLevel.Debug,
        Message = "Polling pending transactions. Count={Count}")]
    public static partial void Polling(ILogger logger, int count);

    [LoggerMessage(EventId = 7703, Level = LogLevel.Information,
        Message = "Pending transaction completed. TransactionId={TransactionId} Status={Status}. Removed.")]
    public static partial void Completed(ILogger logger, string transactionId, string status);

    [LoggerMessage(EventId = 7704, Level = LogLevel.Debug,
        Message = "Pending transaction still pending. TransactionId={TransactionId} Status={Status}")]
    public static partial void StillPending(ILogger logger, string transactionId, string status);

    [LoggerMessage(EventId = 7708, Level = LogLevel.Debug,
        Message = "Skipping transaction {TransactionId} - waiting for callback window to elapse.")]
    public static partial void TooEarly(ILogger logger, string transactionId);

    [LoggerMessage(EventId = 7705, Level = LogLevel.Warning,
        Message = "Status check failed. TransactionId={TransactionId} Code={Code} Message={Message}")]
    public static partial void StatusFailed(ILogger logger, string transactionId, string? code, string? message);

    [LoggerMessage(EventId = 7706, Level = LogLevel.Error,
        Message = "Error processing pending transaction. TransactionId={TransactionId}")]
    public static partial void ProcessingError(ILogger logger, Exception ex, string transactionId);

    [LoggerMessage(EventId = 7707, Level = LogLevel.Error,
        Message = "PendingTransactionsWorker loop error.")]
    public static partial void LoopError(ILogger logger, Exception ex);
}
