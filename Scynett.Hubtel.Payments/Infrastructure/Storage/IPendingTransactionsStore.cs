namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public interface IPendingTransactionsStore
{
    Task AddAsync(string transactionId, CancellationToken cancellationToken = default);
    Task RemoveAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetAllAsync(CancellationToken cancellationToken = default);
}
