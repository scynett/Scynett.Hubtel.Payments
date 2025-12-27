using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Logging;

using System.Net;

namespace Scynett.Hubtel.Payments.Features.TransactionStatus;

internal static partial class Log
{
    [LoggerMessage(
        EventId = HubtelEventIds.StatusCheckStarted,
        Level = LogLevel.Information,
        Message = "Checking transaction status for {identifier}")]
    internal static partial void CheckingStatus(
        ILogger logger,
        string identifier);

    [LoggerMessage(
        EventId = HubtelEventIds.StatusCheckFailed,
        Level = LogLevel.Warning,
        Message = "Failed to check status. HTTP {statusCode}: {error}")]
    internal static partial void FailedToCheckStatus(
        ILogger logger,
        HttpStatusCode statusCode,
        string error);

    [LoggerMessage(
        EventId = HubtelEventIds.StatusCheckError,
        Level = LogLevel.Error,
        Message = "Error checking transaction status for {identifier}")]
    internal static partial void ErrorCheckingStatus(
        ILogger logger,
        Exception exception,
        string identifier);
}
