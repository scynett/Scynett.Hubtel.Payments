using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.Storage.PostgreSql;
using Scynett.Hubtel.Payments.Tests.Fixtures;
using System.Diagnostics.CodeAnalysis;

using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Scynett.Hubtel.Payments.Tests.IntegrationTests;

[Collection(PostgreSqlContainerCollectionDefinition.CollectionName)]
[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "xUnit runs tests without a synchronization context.")]
public sealed class PostgreSqlPendingTransactionsStoreTests : IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _fixture;
    private PostgreSqlPendingTransactionsStore? _store;
    private string _tableName = string.Empty;

    public PostgreSqlPendingTransactionsStoreTests(PostgreSqlContainerFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task AddAsync_ShouldPersistTransaction()
    {
        var timestamp = DateTimeOffset.UtcNow;
        await _store!.AddAsync("txn-add", "client-add", timestamp);

        var pending = await _store.GetAllAsync();

        var entry = pending.Should().ContainSingle(p => p.HubtelTransactionId == "txn-add").Subject;
        entry.ClientReference.Should().Be("client-add");
        entry.CreatedAtUtc.Should().BeCloseTo(timestamp, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteTransaction()
    {
        await _store!.AddAsync("txn-remove", "client-remove", DateTimeOffset.UtcNow);

        await _store.RemoveAsync("txn-remove");

        var pending = await _store.GetAllAsync();
        pending.Should().NotContain(p => p.HubtelTransactionId == "txn-remove");
    }

    [Fact]
    public async Task RemoveOlderThanAsync_ShouldDeleteExpiredRows()
    {
        await _store!.AddAsync("txn-old", "client-old", DateTimeOffset.UtcNow.AddDays(-40));
        await _store.AddAsync("txn-new", "client-new", DateTimeOffset.UtcNow);

        await _store.RemoveOlderThanAsync(DateTimeOffset.UtcNow.AddDays(-30));

        var pending = await _store.GetAllAsync();
        pending.Should().ContainSingle(p => p.HubtelTransactionId == "txn-new");
    }

    public async Task InitializeAsync()
    {
        _tableName = $"pending_{Guid.NewGuid():N}";
        _store = CreateStore(_fixture.ConnectionString, _tableName);
        await _store.InitializeAsync();
    }

    public Task DisposeAsync()
        => _store is null ? Task.CompletedTask : _store.DisposeAsync().AsTask();

    private static PostgreSqlPendingTransactionsStore CreateStore(string connectionString, string tableName)
    {
        var options = OptionsFactory.Create(new PostgreSqlStorageOptions
        {
            ConnectionString = connectionString,
            SchemaName = "hubtel",
            TableName = tableName,
            AutoCreateSchema = true
        });

        return new PostgreSqlPendingTransactionsStore(
            options,
            NullLogger<PostgreSqlPendingTransactionsStore>.Instance);
    }
}
