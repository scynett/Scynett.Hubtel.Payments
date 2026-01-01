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

    public async Task AddAsync(
        string hubtelTransactionId,
        string clientReference,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hubtelTransactionId))
            return;

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var sql = $"""
                INSERT INTO {_options.FullTableName} (hubtel_transaction_id, client_reference, created_at_utc, updated_at_utc)
                VALUES (@TransactionId, @ClientReference, @CreatedAtUtc, @CreatedAtUtc)
                ON CONFLICT (hubtel_transaction_id) DO NOTHING;
                """;

            var parameters = new
            {
                TransactionId = hubtelTransactionId,
                ClientReference = clientReference,
                CreatedAtUtc = createdAtUtc.UtcDateTime
            };

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    parameters,
                    commandTimeout: _options.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
        }

        PostgreSqlStorageLogMessages.TransactionAdded(_logger, hubtelTransactionId);
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
                    r.HubtelTransactionId ?? string.Empty,
                    new DateTimeOffset(r.CreatedAtUtc, TimeSpan.Zero)))
                .ToList();
        }
    }

    public async Task RemoveOlderThanAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            var sql = $"""
                DELETE FROM {_options.FullTableName}
                WHERE created_at_utc < @CutoffUtc;
                """;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new { CutoffUtc = cutoffUtc.UtcDateTime },
                    commandTimeout: _options.CommandTimeoutSeconds,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
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
