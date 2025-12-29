

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Public.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.Public.DirectReceiveMoney;

internal sealed class DirectReceiveMoney(
    InitiateReceiveMoneyProcessor initiateProcessor,
    ReceiveMoneyCallbackProcessor callbackProcessor,
    TransactionStatusProcessor statusProcessor) : IDirectReceiveMoney
{
    public Task<OperationResult<InitiateReceiveMoneyResult>> InitiateAsync(
        InitiateReceiveMoneyRequest request,
        CancellationToken cancellationToken = default)
        => initiateProcessor.ExecuteAsync(request, cancellationToken);

    public Task<OperationResult<ReceiveMoneyCallbackResult>> HandleCallbackAsync(
        ReceiveMoneyCallbackRequest callback,
        CancellationToken cancellationToken = default)
        => callbackProcessor.ExecuteAsync(callback, cancellationToken);

    public Task<OperationResult<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusQuery query,
        CancellationToken cancellationToken = default)
        => statusProcessor.CheckAsync(query, cancellationToken);
}