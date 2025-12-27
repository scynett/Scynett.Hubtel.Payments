using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.InitPayment;

namespace Scynett.Hubtel.Payments.Abstractions;

/// <summary>
/// Processor for Hubtel receive money operations.
/// </summary>
public interface IReceiveMoneyProcessor
{
    /// <summary>
    /// Initiates a receive money transaction.
    /// </summary>
    /// <param name="command">The payment initiation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the payment response or error.</returns>
    Task<Result<InitPaymentResponse>> InitAsync(
        InitPaymentRequest command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a payment callback from Hubtel.
    /// </summary>
    /// <param name="command">The callback data from Hubtel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> ProcessCallbackAsync(
        PaymentCallback command,
        CancellationToken cancellationToken = default);
}
