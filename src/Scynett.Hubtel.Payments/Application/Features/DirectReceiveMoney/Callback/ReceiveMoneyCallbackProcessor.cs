using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using FluentValidation;

using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Infrastructure.Storage;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public sealed class ReceiveMoneyCallbackProcessor(
    IPendingTransactionsStore pendingStore,
    ICallbackAuditStore auditStore,
    IValidator<ReceiveMoneyCallbackRequest> validator,
    ILogger<ReceiveMoneyCallbackProcessor> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

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

        var rawPayload = JsonSerializer.Serialize(callback, SerializerOptions);
        var payloadHash = ComputePayloadHash(rawPayload);
        var startResult = await auditStore.TryStartAsync(
                callback.Data.TransactionId,
                payloadHash,
                rawPayload,
                DateTimeOffset.UtcNow,
                ct)
            .ConfigureAwait(false);

        if (!startResult.CanProcess)
        {
            if (startResult.ExistingResult is not null)
            {
                return OperationResult<ReceiveMoneyCallbackResult>.Success(startResult.ExistingResult);
            }

            return OperationResult<ReceiveMoneyCallbackResult>.Failure(
                Error.Conflict(
                    "Hubtel.Callback.InFlight",
                    "Callback is already being processed."));
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

            if (decision.IsFinal)
            {
                await pendingStore.RemoveAsync(callback.Data.TransactionId, ct).ConfigureAwait(false);

                ReceiveMoneyCallbackLogMessages.PendingRemoved(
                    logger,
                    callback.Data.TransactionId,
                    callback.Data.ClientReference);
            }

            var result = ReceiveMoneyCallbackMapping.ToResult(callback, decision);

            await auditStore.SaveResultAsync(
                    callback.Data.TransactionId,
                    result,
                    decision.IsSuccess,
                    callback.ResponseCode,
                    DateTimeOffset.UtcNow,
                    ct)
                .ConfigureAwait(false);

            return OperationResult<ReceiveMoneyCallbackResult>.Success(result);
        }
        catch (Exception ex)
        {
            await auditStore.MarkFailureAsync(callback.Data.TransactionId, ct).ConfigureAwait(false);

            ReceiveMoneyCallbackLogMessages.ProcessingFailed(
                logger,
                ex,
                callback.Data.TransactionId,
                callback.Data.ClientReference);

            return OperationResult<ReceiveMoneyCallbackResult>.Failure(
                Error.Problem(
                        "Hubtel.Callback.Exception",
                        "An error occurred while processing the Hubtel callback.")
                    .WithMetadata("exception", ex.GetType().Name));
        }
    }

    private static string ComputePayloadHash(string rawPayload)
    {
        var bytes = Encoding.UTF8.GetBytes(rawPayload);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
