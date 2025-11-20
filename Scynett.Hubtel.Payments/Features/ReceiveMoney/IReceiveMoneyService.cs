using Scynett.Hubtel.Payments.Common;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

public interface IReceiveMoneyService
{
    Task<Result<InitReceiveMoneyResponse>> InitAsync(InitReceiveMoneyCommand command, CancellationToken cancellationToken = default);
    Task<Result> ProcessCallbackAsync(ReceiveMoneyCallbackCommand command, CancellationToken cancellationToken = default);
}
