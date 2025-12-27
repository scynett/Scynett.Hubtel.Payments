using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;

namespace Scynett.Hubtel.Payments.Abstractions;

/// <summary>
/// Processor for Hubtel receive money operations.
/// </summary>
public interface IReceiveMoneyProcessor
{
    /// <summary>
    /// Initiates a receive money transaction.
    /// </summary>
    /// <param name="request">The receive money request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the transaction details or error.</returns>
    Task<Result<ReceiveMoneyResult>> InitAsync(
        ReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a payment callback from Hubtel.
    /// </summary>
    /// <param name="callback">The callback data from Hubtel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> ProcessCallbackAsync(
        PaymentCallback callback,
        CancellationToken cancellationToken = default);
}
