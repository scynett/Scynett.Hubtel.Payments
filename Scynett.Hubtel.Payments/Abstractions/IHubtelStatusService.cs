using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.Status;

namespace Scynett.Hubtel.Payments.Abstractions;

/// <summary>
/// Processor for checking Hubtel transaction status.
/// </summary>
public interface ITransactionStatusProcessor
{
    /// <summary>
    /// Checks the status of a transaction.
    /// </summary>
    /// <param name="query">The status check request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the transaction status or error.</returns>
    Task<Result<CheckStatusResponse>> CheckStatusAsync(
        StatusRequest query,
        CancellationToken cancellationToken = default);
}
