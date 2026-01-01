using Microsoft.Extensions.Logging;

namespace Scynett.Hubtel.Payments.Application.Common;

internal static partial class PendingTransactionsCleanupLogMessages
{
    [LoggerMessage(EventId = 7801, Level = LogLevel.Information,
        Message = "PendingTransactionsCleanupService started. Retention={Retention} CleanupInterval={CleanupInterval}")]
    public static partial void Started(ILogger logger, TimeSpan retention, TimeSpan cleanupInterval);

    [LoggerMessage(EventId = 7802, Level = LogLevel.Information,
        Message = "Pending transactions cleanup completed. Removed entries older than {CutoffUtc}.")]
    public static partial void Completed(ILogger logger, DateTimeOffset cutoffUtc);

    [LoggerMessage(EventId = 7803, Level = LogLevel.Error,
        Message = "Pending transactions cleanup failed.")]
    public static partial void Failed(ILogger logger, Exception exception);
}
