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
