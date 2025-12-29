using Microsoft.Extensions.Logging;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

internal static partial class TransactionStatusLogMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Invalid transaction status request: {ClientReference}")]
    public static partial void InvalidRequest(this ILogger logger, string clientReference);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Checking Hubtel transaction status for ClientReference={ClientReference}")]
    public static partial void CheckingStatus(this ILogger logger, string clientReference);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Error while checking transaction status for {ClientReference}")]
    public static partial void StatusCheckError(this ILogger logger, Exception exception, string clientReference);
}
