using FluentValidation;

using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;


internal sealed class TransactionStatusProcessor(
    IHubtelReceiveMoneyGateway gateway,
    IValidator<TransactionStatusRequest> validator,
    ILogger<TransactionStatusProcessor> logger)
{
    public async Task<OperationResult<TransactionStatusResult>> CheckAsync(
        TransactionStatusRequest request,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct)
            .ConfigureAwait(false);
        if (!validation.IsValid)
        {
            logger.InvalidRequest(request.ClientReference);
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Validation("TransactionStatus.InvalidRequest", validation.ToString()));
        }

        try
        {
            logger.CheckingStatus(request.ClientReference);

            var gatewayResult =
                await gateway.GetTransactionStatusAsync(
                    request.ClientReference,
                    ct).ConfigureAwait(false);

            var decision = HubtelResponseDecisionFactory.Create(
                gatewayResult.ResponseCode,
                gatewayResult.Message);

            if (!decision.IsSuccess && decision.IsFinal)
            {
                return OperationResult<TransactionStatusResult>.Failure(
                    Error.Failure(
                        $"TransactionStatus.{decision.Category}",
                        decision.CustomerMessage ?? "Transaction status check failed"));
            }

            var result = TransactionStatusMapping.ToResult(
                gatewayResult,
                decision);

            return OperationResult<TransactionStatusResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.StatusCheckError(ex, request.ClientReference);

            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem(
                    "TransactionStatus.Exception",
                    "An error occurred while checking transaction status"));
        }
    }
}