using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Logging;

using System.Net;

namespace Scynett.Hubtel.Payments.Features.Status;

internal static partial class Log
{
    [LoggerMessage(
        EventId = HubtelEventIds.StatusCheckFailed,
        Level = LogLevel.Error,
        Message = "Failed to check status: {statusCode} - {error}")]
    internal static partial void FailedToCheckStatus(
        ILogger logger,
        HttpStatusCode statusCode,
        string error);

    [LoggerMessage(
        EventId = HubtelEventIds.StatusCheckError,
        Level = LogLevel.Error,
        Message = "Error checking status for transaction {transactionId}")]
    internal static partial void ErrorCheckingStatus(
        ILogger logger,
        Exception exception,
        string transactionId);
}
