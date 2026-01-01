using Microsoft.Extensions.Logging;

namespace Scynett.Hubtel.Payments.Storage.PostgreSql;

internal static partial class PostgreSqlStorageLogMessages
{
    [LoggerMessage(
        EventId = 10001,
        Level = LogLevel.Information,
        Message = "PostgreSQL schema initialized for table {TableName}")]
    public static partial void SchemaInitialized(ILogger logger, string tableName);

    [LoggerMessage(
        EventId = 10002,
        Level = LogLevel.Debug,
        Message = "Added pending transaction {TransactionId} to PostgreSQL store")]
    public static partial void TransactionAdded(ILogger logger, string transactionId);

    [LoggerMessage(
        EventId = 10003,
        Level = LogLevel.Debug,
        Message = "Removed pending transaction {TransactionId} from PostgreSQL store")]
    public static partial void TransactionRemoved(ILogger logger, string transactionId);

    [LoggerMessage(
        EventId = 10004,
        Level = LogLevel.Error,
        Message = "PostgreSQL storage operation failed for transaction {TransactionId}")]
    public static partial void OperationFailed(ILogger logger, Exception exception, string transactionId);
}
