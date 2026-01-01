using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "ConfigureAwait not required in unit tests.")]
public sealed class InMemoryPendingTransactionsStoreTests : UnitTestBase
{
    [Fact]
    public async Task AddAsync_ShouldIgnore_WhenTransactionIdIsNullOrEmpty()
    {
        var store = new InMemoryPendingTransactionsStore();

        await store.AddAsync(string.Empty, "client", DateTimeOffset.UtcNow);
        await store.AddAsync(null!, "client", DateTimeOffset.UtcNow);

        var all = await store.GetAllAsync();
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveAsync_ShouldNoOp_WhenTransactionIdIsNullOrEmpty()
    {
        var store = new InMemoryPendingTransactionsStore();
        await store.AddAsync("txn-1", "client-1", DateTimeOffset.UtcNow);

        await store.RemoveAsync(string.Empty);
        await store.RemoveAsync(null!);

        var all = await store.GetAllAsync();
        all.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_ShouldBeIdempotent_WhenSameIdAddedTwice()
    {
        var store = new InMemoryPendingTransactionsStore();
        var timestamp = DateTimeOffset.UtcNow;

        await store.AddAsync("txn-2", "client-2", timestamp);
        await store.AddAsync("txn-2", "client-3", timestamp.AddMinutes(1));

        var all = await store.GetAllAsync();
        all.Should().HaveCount(1);
        all.Single().CreatedAtUtc.Should().Be(timestamp);
    }

    [Fact]
    public async Task RemoveAsync_ShouldBeIdempotent_WhenSameIdRemovedTwice()
    {
        var store = new InMemoryPendingTransactionsStore();
        await store.AddAsync("txn-3", "client-3", DateTimeOffset.UtcNow);

        await store.RemoveAsync("txn-3");
        await store.RemoveAsync("txn-3");

        var all = await store.GetAllAsync();
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoTransactionsExist()
    {
        var store = new InMemoryPendingTransactionsStore();

        var all = await store.GetAllAsync();

        all.Should().BeEmpty();
    }

    [Fact]
    public async Task Store_ShouldKeepTimestampsStable_ForExistingIds()
    {
        var store = new InMemoryPendingTransactionsStore();
        var timestamp = DateTimeOffset.UtcNow;
        await store.AddAsync("txn-4", "client-4", timestamp);

        await store.AddAsync("txn-4", "client-4", timestamp.AddHours(1));

        var all = await store.GetAllAsync();
        all.Single().CreatedAtUtc.Should().Be(timestamp);
    }

    [Fact]
    public async Task Store_ShouldHandleConcurrentAddAndRemove_ForSameId()
    {
        var store = new InMemoryPendingTransactionsStore();
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(async () =>
            {
                await store.AddAsync("txn-5", "client-5", DateTimeOffset.UtcNow);
                await store.RemoveAsync("txn-5");
            }));

        await Task.WhenAll(tasks);

        var all = await store.GetAllAsync();
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveOlderThanAsync_ShouldDeleteExpiredEntries()
    {
        var store = new InMemoryPendingTransactionsStore();
        var oldTimestamp = DateTimeOffset.UtcNow.AddDays(-10);
        await store.AddAsync("txn-old", "client-old", oldTimestamp);
        await store.AddAsync("txn-new", "client-new", DateTimeOffset.UtcNow);

        await store.RemoveOlderThanAsync(DateTimeOffset.UtcNow.AddDays(-5));

        var all = await store.GetAllAsync();
        all.Select(p => p.HubtelTransactionId).Should().ContainSingle(id => id == "txn-new");
    }
}
