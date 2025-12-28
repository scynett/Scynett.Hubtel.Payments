using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

namespace Scynett.Hubtel.Payments.Public.DirectReceiveMoney;
/// <summary>
/// Direct Mobile Money receive operations (MoMo Debit).
/// </summary>

public interface IDirectReceiveMoney
{
    /// <summary>
    /// Initiates a Mobile Money debit request.
    /// </summary>
    Task<OperationResult<InitiateReceiveMoneyResult>> InitiateAsync(
        InitiateReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);

    /*
     
     
      /// <summary>
    /// Queries transaction status when a callback was not received.
    /// </summary>
    Task<OperationResult<QueryReceiveMoneyStatusResult>> QueryStatusAsync(
        QueryReceiveMoneyStatusRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles Hubtel callback payload for receive money transactions.
    /// Intended to be invoked by a webhook endpoint.
    /// </summary>
    Task<OperationResult> HandleCallbackAsync(
        ReceiveMoneyCallbackPayload payload,
        CancellationToken cancellationToken = default);
     
     */

}