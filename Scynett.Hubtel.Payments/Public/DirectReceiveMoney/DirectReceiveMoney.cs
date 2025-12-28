using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

namespace Scynett.Hubtel.Payments.Public.DirectReceiveMoney;

internal class DirectReceiveMoney(InitiateReceiveMoneyProcessor initiate) : IDirectReceiveMoney
{
    public Task<OperationResult<InitiateReceiveMoneyResult>> InitiateAsync(InitiateReceiveMoneyRequest request, CancellationToken cancellationToken = default) 
        => initiate.ExecuteAsync(request, cancellationToken);

}
