using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

namespace Scynett.Hubtel.Payments.Public.DirectReceiveMoney;


/// <summary>
/// Direct Mobile Money receive operations (MoMo Debit).
/// </summary>
public interface IDirectReceiveMoney
{
    Task<OperationResult<InitiateReceiveMoneyResult>> InitiateAsync(
          InitiateReceiveMoneyRequest request,
          CancellationToken cancellationToken = default);

    Task<OperationResult<ReceiveMoneyCallbackResult>> HandleCallbackAsync(
        ReceiveMoneyCallbackRequest callback,
        CancellationToken cancellationToken = default);

    Task<OperationResult<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusQuery query,
        CancellationToken cancellationToken = default);
}