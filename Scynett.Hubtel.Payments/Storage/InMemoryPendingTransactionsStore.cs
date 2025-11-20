using System.Collections.Concurrent;

namespace Scynett.Hubtel.Payments.Storage;

public sealed class InMemoryPendingTransactionsStore : IPendingTransactionsStore
{
    private readonly ConcurrentDictionary<string, byte> _transactions = new();

    public Task AddAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _transactions.TryAdd(transactionId, 0);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        _transactions.TryRemove(transactionId, out _);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<string>>(_transactions.Keys.ToList());
    }
}
