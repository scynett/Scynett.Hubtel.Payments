# Scynett.Hubtel.Payments.Storage.PostgreSql Classes

Generated on 2026-01-01T09:04:47Z

## src\Scynett.Hubtel.Payments.Storage.PostgreSql\PostgreSqlPendingTransactionsStore.cs

 ```csharp 
using System.Data;

using Dapper;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using Scynett.Hubtel.Payments.Infrastructure.Storage;

namespace Scynett.Hubtel.Payments.Storage.PostgreSql;

/// <summary>
/// PostgreSQL-backed implementation of <see cref="IPendingTransactionsStore"/>.
/// Provides durable storage for pending transactions that survives application restarts.
/// </summary>
public sealed class PostgreSqlPendingTransactionsStore : IPendingTransactionsStore, IAsyncDisposable
{
    private readonly PostgreSqlStorageOptions _options;
    private readonly ILogger<PostgreSqlPendingTransactionsStore> _logger;
    private readonly NpgsqlDataSource _dataSource;
    private bool _initialized;

    public PostgreSqlPendingTransactionsStore(
        IOptions<PostgreSqlStorageOptions> options,
        ILogger<PostgreSqlPendingTransactionsStore> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException(
                $"PostgreSQL connection string is required. Configure '{PostgreSqlStorageOptions.SectionName}:ConnectionString'.");
        }

        _dataSource = NpgsqlDataSource.Create(_options.ConnectionString);
    }

    /// <summary>
    /// Ensures the schema and table exist. Called automatically on first operation if AutoCreateSchema is enabled.
    /// Can also be called explicitly during application startup.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
            return;

        if (!_options.AutoCreateSchema)
        {
            _initialized = true;
            return;
        }

        var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var createSchemaSql = $"""
                CREATE SCHEMA IF NOT EXISTS "{_options.SchemaName}";
                """;

            var createTableSql = $"""
                CREATE TABLE IF NOT EXISTS {_options.FullTableName} (
                    hubtel_transaction_id VARCHAR(100) PRIMARY KEY,
                    client_reference VARCHAR(100),
                    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                    updated_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW()
                );

                CREATE INDEX IF NOT EXISTS ix_{_options.TableName}_created_at 
                    ON {_options.FullTableName} (created_at_utc);
                """;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    createSchemaSql,
                    commandTimeout: _options.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    createTableSql,
                    commandTimeout: _options.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
        }

        PostgreSqlStorageLogMessages.SchemaInitialized(_logger, _options.FullTableName);
        _initialized = true;
    }

    public async Task AddAsync(string transactionId, DateTimeOffset createdAtUtc, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return;

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var sql = $"""
                INSERT INTO {_options.FullTableName} (hubtel_transaction_id, client_reference, created_at_utc, updated_at_utc)
                VALUES (@TransactionId, @TransactionId, @CreatedAtUtc, @CreatedAtUtc)
                ON CONFLICT (hubtel_transaction_id) DO NOTHING;
                """;

            var parameters = new
            {
                TransactionId = transactionId,
                CreatedAtUtc = createdAtUtc.UtcDateTime
            };

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    parameters,
                    commandTimeout: _options.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
        }

        PostgreSqlStorageLogMessages.TransactionAdded(_logger, transactionId);
    }

    public async Task RemoveAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return;

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var sql = $"""
                DELETE FROM {_options.FullTableName}
                WHERE hubtel_transaction_id = @TransactionId;
                """;

            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new { TransactionId = transactionId },
                    commandTimeout: _options.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                PostgreSqlStorageLogMessages.TransactionRemoved(_logger, transactionId);
            }
        }
    }

    public async Task<IReadOnlyList<PendingTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var sql = $"""
                SELECT 
                    client_reference AS ClientReference,
                    hubtel_transaction_id AS HubtelTransactionId,
                    created_at_utc AS CreatedAtUtc
                FROM {_options.FullTableName}
                ORDER BY created_at_utc ASC;
                """;

            var results = await connection.QueryAsync<PendingTransactionRow>(
                new CommandDefinition(
                    sql,
                    commandTimeout: _options.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

            return results
                .Select(r => new PendingTransaction(
                    r.ClientReference ?? r.HubtelTransactionId ?? string.Empty,
                    r.HubtelTransactionId,
                    new DateTimeOffset(r.CreatedAtUtc, TimeSpan.Zero)))
                .ToList();
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            await InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _dataSource.DisposeAsync().ConfigureAwait(false);
    }

#pragma warning disable CA1812 // Internal class instantiated by Dapper
    private sealed record PendingTransactionRow(
        string? ClientReference,
        string? HubtelTransactionId,
        DateTime CreatedAtUtc);
#pragma warning restore CA1812
}
 ``` 

## src\Scynett.Hubtel.Payments.Storage.PostgreSql\PostgreSqlStorageLogMessages.cs

 ```csharp 
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
 ``` 

## src\Scynett.Hubtel.Payments.Storage.PostgreSql\PostgreSqlStorageOptions.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Storage.PostgreSql;

/// <summary>
/// Configuration options for PostgreSQL pending transactions store.
/// </summary>
public sealed class PostgreSqlStorageOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Hubtel:Storage:PostgreSql";

    /// <summary>
    /// PostgreSQL connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Schema name for the pending transactions table. Defaults to "hubtel".
    /// </summary>
    public string SchemaName { get; set; } = "hubtel";

    /// <summary>
    /// Table name for storing pending transactions. Defaults to "pending_transactions".
    /// </summary>
    public string TableName { get; set; } = "pending_transactions";

    /// <summary>
    /// Whether to automatically create the schema and table on startup. Defaults to true.
    /// </summary>
    public bool AutoCreateSchema { get; set; } = true;

    /// <summary>
    /// Command timeout in seconds. Defaults to 30.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the fully qualified table name (schema.table).
    /// </summary>
    public string FullTableName => $"\"{SchemaName}\".\"{TableName}\"";
}
 ``` 

## src\Scynett.Hubtel.Payments.Storage.PostgreSql\ServiceCollectionExtensions.cs

 ```csharp 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Scynett.Hubtel.Payments.Infrastructure.Storage;

namespace Scynett.Hubtel.Payments.Storage.PostgreSql;

/// <summary>
/// Extension methods for registering PostgreSQL pending transactions storage.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PostgreSQL-backed pending transactions storage to the service collection.
    /// This replaces the default in-memory store with a durable PostgreSQL implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure PostgreSQL storage options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Call this method BEFORE calling AddHubtelPayments() to ensure the PostgreSQL store
    /// is used instead of the default in-memory store.
    /// 
    /// Example:
    /// <code>
    /// services.AddHubtelPostgreSqlStorage(options =>
    /// {
    ///     options.ConnectionString = "Host=localhost;Database=hubtel;Username=user;Password=pass";
    ///     options.SchemaName = "hubtel";
    ///     options.TableName = "pending_transactions";
    /// });
    /// services.AddHubtelPayments();
    /// </code>
    /// </remarks>
    public static IServiceCollection AddHubtelPostgreSqlStorage(
        this IServiceCollection services,
        Action<PostgreSqlStorageOptions>? configure = null)
    {
        services.AddOptions<PostgreSqlStorageOptions>();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        // Remove any existing IPendingTransactionsStore registrations
        services.RemoveAll<IPendingTransactionsStore>();

        // Register PostgreSQL store as singleton (uses connection pooling internally)
        services.AddSingleton<IPendingTransactionsStore, PostgreSqlPendingTransactionsStore>();

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL-backed pending transactions storage using a connection string.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">PostgreSQL connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHubtelPostgreSqlStorage(
        this IServiceCollection services,
        string connectionString)
    {
        return services.AddHubtelPostgreSqlStorage(options =>
        {
            options.ConnectionString = connectionString;
        });
    }

    /// <summary>
    /// Binds PostgreSQL storage options from configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section to bind from.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHubtelPostgreSqlStorage(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<PostgreSqlStorageOptions>()
            .Bind(configuration.GetSection(PostgreSqlStorageOptions.SectionName));

        // Remove any existing IPendingTransactionsStore registrations
        services.RemoveAll<IPendingTransactionsStore>();

        // Register PostgreSQL store
        services.AddSingleton<IPendingTransactionsStore, PostgreSqlPendingTransactionsStore>();

        return services;
    }
}
 ``` 

