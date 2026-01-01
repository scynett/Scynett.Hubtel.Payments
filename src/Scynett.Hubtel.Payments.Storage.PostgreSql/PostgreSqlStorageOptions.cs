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
