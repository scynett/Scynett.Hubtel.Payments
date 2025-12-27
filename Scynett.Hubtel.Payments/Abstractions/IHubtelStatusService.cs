using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.TransactionStatus;

namespace Scynett.Hubtel.Payments.Abstractions;

/// <summary>
/// Processor for checking Hubtel transaction status.
/// </summary>
public interface ITransactionStatusProcessor
{
    /// <summary>
    /// Checks the status of a transaction.
    /// </summary>
    /// <param name="request">The status check request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the transaction status or error.</returns>
    Task<Result<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusRequest request,
        CancellationToken cancellationToken = default);
}
