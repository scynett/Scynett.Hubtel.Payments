using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;

internal interface IHubtelTransactionStatusGateway
{
    Task<OperationResult<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusQuery query,
        CancellationToken ct = default);
}