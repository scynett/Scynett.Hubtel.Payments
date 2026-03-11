using System.Threading;
using System.Threading.Tasks;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

/// <summary>
/// Allows consuming applications to react to successfully processed callbacks.
/// Hooked the callback endpoint into a new handler extension point so consuming apps can observe successful callbacks.
/// </summary>
public interface IReceiveMoneyCallbackHandler
{
    /// <summary>
    /// Invoked when a "Receive Money" callback has been successfully processed.
    /// </summary>
    /// <param name="result">The result of the callback containing relevant information.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task OnCompletedAsync(
        ReceiveMoneyCallbackResult result,
        CancellationToken cancellationToken = default);
}
