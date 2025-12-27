using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Models;

namespace Scynett.Hubtel.Payments.AspNetCore.Endpoints;

internal static partial class Log
{
    [LoggerMessage(
        EventId = HubtelEventIds.CallbackReceived,
        Level = LogLevel.Information,
        Message = "Received Hubtel callback for transaction {transactionId}")]
    internal static partial void ReceivedCallback(
        ILogger logger,
        string? transactionId);

    [LoggerMessage(
        EventId = HubtelEventIds.CallbackInvalidData,
        Level = LogLevel.Warning,
        Message = "Received callback with null data")]
    internal static partial void ReceivedCallbackWithNullData(ILogger logger);

    [LoggerMessage(
        EventId = HubtelEventIds.CallbackError,
        Level = LogLevel.Error,
        Message = "Failed to process callback: {error}")]
    internal static partial void CallbackProcessingFailed(
        ILogger logger,
        string error);

    [LoggerMessage(
        EventId = HubtelEventIds.CallbackProcessed,
        Level = LogLevel.Information,
        Message = "Successfully processed callback for transaction {transactionId}")]
    internal static partial void CallbackProcessedSuccessfully(
        ILogger logger,
        string? transactionId);
}
