using Scynett.Hubtel.Payments.Common;

namespace Scynett.Hubtel.Payments.Features.Status;

public interface IHubtelStatusService
{
    Task<Result<CheckStatusResponse>> CheckStatusAsync(CheckStatusQuery query, CancellationToken cancellationToken = default);
}
