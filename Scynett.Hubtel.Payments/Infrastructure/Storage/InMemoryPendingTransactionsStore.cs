using System.Collections.Concurrent;

namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public sealed class InMemoryPendingTransactionsStore : IPendingTransactionsStore
{
    private readonly ConcurrentDictionary<string, byte> _transactions = new(StringComparer.OrdinalIgnoreCase);

    public Task AddAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(transactionId))
            _transactions.TryAdd(transactionId, 0);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(transactionId))
            _transactions.TryRemove(transactionId, out _);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>(_transactions.Keys.ToList());
}
