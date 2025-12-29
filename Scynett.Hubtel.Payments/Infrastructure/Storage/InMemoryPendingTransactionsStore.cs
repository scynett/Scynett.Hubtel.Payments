using System.Collections.Concurrent;

namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public sealed class InMemoryPendingTransactionsStore : IPendingTransactionsStore
{
    private sealed record Entry(DateTimeOffset CreatedAtUtc);

    private readonly ConcurrentDictionary<string, Entry> _transactions = new(StringComparer.OrdinalIgnoreCase);

    public Task AddAsync(string transactionId, DateTimeOffset createdAtUtc, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(transactionId))
            _transactions.TryAdd(transactionId, new Entry(createdAtUtc));

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(transactionId))
            _transactions.TryRemove(transactionId, out _);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PendingTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = _transactions.Select(pair =>
            new PendingTransaction(
                ClientReference: pair.Key,
                HubtelTransactionId: pair.Key,
                CreatedAtUtc: pair.Value.CreatedAtUtc)).ToList();

        return Task.FromResult<IReadOnlyList<PendingTransaction>>(results);
    }
}
