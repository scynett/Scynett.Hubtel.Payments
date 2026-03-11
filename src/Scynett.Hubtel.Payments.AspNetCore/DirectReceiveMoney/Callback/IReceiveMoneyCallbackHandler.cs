using System.Threading;
using System.Threading.Tasks;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

/// <summary>
/// Allows consuming applications to react to successfully processed callbacks.
/// </summary>
public interface IReceiveMoneyCallbackHandler
{
    Task OnCompletedAsync(
        ReceiveMoneyCallbackResult result,
        CancellationToken cancellationToken = default);
}
