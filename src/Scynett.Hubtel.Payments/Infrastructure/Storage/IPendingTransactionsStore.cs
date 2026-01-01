namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public interface IPendingTransactionsStore
{
    Task AddAsync(
        string hubtelTransactionId,
        string clientReference,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all pending transaction IDs to poll.
    /// </summary>
    Task<IReadOnlyList<PendingTransaction>> GetAllAsync(CancellationToken cancellationToken = default);

    Task RemoveOlderThanAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken = default);
}
