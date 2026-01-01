using System.Collections.Concurrent;

namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public sealed class InMemoryPendingTransactionsStore : IPendingTransactionsStore
{
    private sealed record Entry(string ClientReference, DateTimeOffset CreatedAtUtc);

    private readonly ConcurrentDictionary<string, Entry> _transactions = new(StringComparer.OrdinalIgnoreCase);

    public Task AddAsync(
        string hubtelTransactionId,
        string clientReference,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(hubtelTransactionId))
        {
            var entry = new Entry(clientReference, createdAtUtc);
            _transactions.TryAdd(hubtelTransactionId, entry);
        }

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
                ClientReference: pair.Value.ClientReference,
                HubtelTransactionId: pair.Key,
                CreatedAtUtc: pair.Value.CreatedAtUtc)).ToList();

        return Task.FromResult<IReadOnlyList<PendingTransaction>>(results);
    }

    public Task RemoveOlderThanAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken = default)
    {
        foreach (var (key, entry) in _transactions)
        {
            if (entry.CreatedAtUtc < cutoffUtc)
                _transactions.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
