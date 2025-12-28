using Microsoft.Extensions.Logging;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

internal static partial class InitiateReceiveMoneyLogMessages
{
    [LoggerMessage(
        EventId = 41001,
        Level = LogLevel.Warning,
        Message = "DirectReceiveMoney validation failed. ClientReference={ClientReference}. Error={Error}")]
    public static partial void ValidationFailed(
        ILogger logger,
        string clientReference,
        string error);

    [LoggerMessage(
        EventId = 41002,
        Level = LogLevel.Information,
        Message = "Initiating DirectReceiveMoney. ClientReference={ClientReference}, Amount={Amount}, Network={Network}, Msisdn={Msisdn}")]
    public static partial void Initiating(
        ILogger logger,
        string clientReference,
        decimal amount,
        string network,
        string msisdn);

    [LoggerMessage(
        EventId = 41003,
        Level = LogLevel.Error,
        Message = "DirectReceiveMoney gateway call failed. ClientReference={ClientReference}. Error={ErrorCode} {ErrorDescription}")]
    public static partial void GatewayFailed(
        ILogger logger,
        string clientReference,
        string errorCode,
        string errorDescription);

    [LoggerMessage(
        EventId = 41004,
        Level = LogLevel.Information,
        Message = "DirectReceiveMoney decision computed. ClientReference={ClientReference}, Code={Code}, Category={Category}, NextAction={NextAction}, IsFinal={IsFinal}")]
    public static partial void DecisionComputed(
        ILogger logger,
        string clientReference,
        string code,
        string category,
        string nextAction,
        bool isFinal);

    [LoggerMessage(
        EventId = 41005,
        Level = LogLevel.Information,
        Message = "Stored pending DirectReceiveMoney transaction. ClientReference={ClientReference}, TransactionId={TransactionId}")]
    public static partial void PendingStored(
        ILogger logger,
        string clientReference,
        string transactionId);

    [LoggerMessage(
        EventId = 41006,
        Level = LogLevel.Warning,
        Message = "DirectReceiveMoney pending decision returned but TransactionId is missing. ClientReference={ClientReference}, Code={Code}")]
    public static partial void PendingButMissingTransactionId(
        ILogger logger,
        string clientReference,
        string code);

    [LoggerMessage(
        EventId = 41007,
        Level = LogLevel.Error,
        Message = "Unhandled exception while initiating DirectReceiveMoney. ClientReference={ClientReference}")]
    public static partial void UnhandledException(
        ILogger logger,
        Exception exception,
        string clientReference);
}
