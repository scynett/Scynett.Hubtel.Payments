using FluentValidation;

using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Infrastructure.Storage;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public sealed class ReceiveMoneyCallbackProcessor(
    IPendingTransactionsStore pendingStore,
    IValidator<ReceiveMoneyCallbackRequest> validator,
    ILogger<ReceiveMoneyCallbackProcessor> logger)
{
    public async Task<OperationResult<ReceiveMoneyCallbackResult>> ExecuteAsync(
        ReceiveMoneyCallbackRequest callback,
        CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(callback, ct).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            ReceiveMoneyCallbackLogMessages.ValidationFailed(
                logger,
                callback.Data?.TransactionId ?? string.Empty,
                callback.Data?.ClientReference ?? string.Empty);

            return OperationResult<ReceiveMoneyCallbackResult>.Failure(
                Error.Validation("Hubtel.Callback.Validation", validation.ToString()));
        }

        try
        {
            ReceiveMoneyCallbackLogMessages.CallbackReceived(
                logger,
                callback.Data.ClientReference,
                callback.Data.TransactionId,
                callback.ResponseCode);

            var messageForDecision = ReceiveMoneyCallbackMapping.BuildDecisionMessage(callback);

            var decision = HubtelResponseDecisionFactory.Create(
                callback.ResponseCode,
                messageForDecision);

            ReceiveMoneyCallbackLogMessages.CallbackDecision(
                logger,
                decision.Code,
                decision.Category.ToString(),
                decision.IsFinal,
                decision.NextAction.ToString());

            // For callbacks, response is final for 0000 and 2001; but we still follow decision.IsFinal.
            if (decision.IsFinal)
            {
                // Remove pending by TransactionId (Hubtel callback always contains TransactionId).
                await pendingStore.RemoveAsync(callback.Data.TransactionId, ct).ConfigureAwait(false);

                ReceiveMoneyCallbackLogMessages.PendingRemoved(
                    logger,
                    callback.Data.TransactionId,
                    callback.Data.ClientReference);
            }

            var result = ReceiveMoneyCallbackMapping.ToResult(callback, decision);

            // Decide whether to treat non-success callbacks as failures:
            // - If you want the endpoint to return 200 always (recommended), still return Success here,
            //   but include IsSuccess=false in the result. However, OperationResult should reflect
            //   whether *processing* succeeded, not payment success.
            return OperationResult<ReceiveMoneyCallbackResult>.Success(result);
        }
        catch (Exception ex)
        {
            ReceiveMoneyCallbackLogMessages.ProcessingFailed(
                logger,
                ex,
                callback.Data.TransactionId,
                callback.Data.ClientReference);

            return OperationResult<ReceiveMoneyCallbackResult>.Failure(
                Error.Problem("Hubtel.Callback.Exception", "An error occurred while processing the Hubtel callback."));
        }
    }
}