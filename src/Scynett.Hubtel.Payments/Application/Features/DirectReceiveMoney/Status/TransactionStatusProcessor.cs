using System.Diagnostics;

using FluentValidation;

using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Infrastructure.Diagnostics;

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
            using var activity = HubtelDiagnostics.ActivitySource.StartActivity("DirectReceiveMoney.Status");
            activity?.SetTag("hubtel.clientReference", query.ClientReference ?? string.Empty);
            activity?.SetTag("hubtel.transactionId", query.HubtelTransactionId ?? string.Empty);
            activity?.SetTag("hubtel.networkTransactionId", query.NetworkTransactionId ?? string.Empty);

            var key = GetLogKey(query);
            logger.CheckingStatus(key);

            var gatewayResult = await gateway.CheckStatusAsync(query, ct).ConfigureAwait(false);

            if (gatewayResult.IsFailure)
            {
                activity?.SetStatus(ActivityStatusCode.Error, gatewayResult.Error?.Description);
                return OperationResult<TransactionStatusResult>.Failure(gatewayResult.Error!);
            }

            var decision = HubtelResponseDecisionFactory.Create(
                gatewayResult.Value!.RawResponseCode,
                gatewayResult.Value!.RawMessage);

            if (!decision.IsSuccess && decision.IsFinal)
            {
                activity?.SetStatus(ActivityStatusCode.Error, decision.CustomerMessage ?? gatewayResult.Value!.RawMessage);
                return OperationResult<TransactionStatusResult>.Failure(
                    Error.Failure(
                            $"TransactionStatus.{decision.Category}",
                            decision.CustomerMessage ?? "Transaction status check failed")
                        .WithProvider(gatewayResult.Value!.RawResponseCode, gatewayResult.Value!.RawMessage));
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            return OperationResult<TransactionStatusResult>.Success(gatewayResult.Value!);
        }
        catch (Exception ex)
        {
            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
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
