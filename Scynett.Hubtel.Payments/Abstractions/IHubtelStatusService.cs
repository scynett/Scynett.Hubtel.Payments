using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.Status;

namespace Scynett.Hubtel.Payments.Abstractions;

public interface IHubtelStatusService
{
    Task<Result<CheckStatusResponse>> CheckStatusAsync(StatusRequest query, CancellationToken cancellationToken = default);
}
