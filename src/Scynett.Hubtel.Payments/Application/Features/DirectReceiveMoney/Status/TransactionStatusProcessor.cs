using FluentValidation;

using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

internal sealed class TransactionStatusProcessor(
    IHubtelTransactionStatusGateway gateway, 
    IValidator<TransactionStatusQuery> validator,
    ILogger<TransactionStatusProcessor> logger)
{
    public async Task<OperationResult<TransactionStatusResult>> CheckAsync(
        TransactionStatusQuery query,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(query, ct).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            logger.InvalidRequest(GetLogKey(query));
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Validation("TransactionStatus.InvalidQuery", validation.ToString()));
        }

        try
        {
            var key = GetLogKey(query);
            logger.CheckingStatus(key);

            var gatewayResult = await gateway.CheckStatusAsync(query, ct).ConfigureAwait(false);

            if (gatewayResult.IsFailure)
                return OperationResult<TransactionStatusResult>.Failure(gatewayResult.Error!);

            // If your status endpoint returns "responseCode" always 0000 for successful call
            // you can still apply decisions if you want, but typically it’s not needed.
            // Keeping it here is fine if you already have code mapping logic.
            var decision = HubtelResponseDecisionFactory.Create(
                gatewayResult.Value!.RawResponseCode,
                gatewayResult.Value!.RawMessage);

            if (!decision.IsSuccess && decision.IsFinal)
            {
                return OperationResult<TransactionStatusResult>.Failure(
                    Error.Failure(
                            $"TransactionStatus.{decision.Category}",
                            decision.CustomerMessage ?? "Transaction status check failed")
                        .WithProvider(gatewayResult.Value!.RawResponseCode, gatewayResult.Value!.RawMessage));
            }

            return OperationResult<TransactionStatusResult>.Success(gatewayResult.Value!);
        }
        catch (Exception ex)
        {
            logger.StatusCheckError(ex, GetLogKey(query));
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem("TransactionStatus.Exception",
                        "An error occurred while checking transaction status")
                    .WithMetadata("exception", ex.GetType().Name));
        }
    }

    private static string GetLogKey(TransactionStatusQuery q)
        => q.ClientReference
        ?? q.HubtelTransactionId
        ?? q.NetworkTransactionId
        ?? "unknown";
}
