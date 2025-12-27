using FluentValidation;

using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;
using Scynett.Hubtel.Payments.Storage;
using Scynett.Hubtel.Payments.Validation;

using System.Globalization;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

/// <summary>
/// Processor for Hubtel receive money operations.
/// </summary>
public sealed class ReceiveMoneyProcessor(
    IHubtelReceiveMoneyClient client,
    IPendingTransactionsStore pendingStore,
    ILogger<ReceiveMoneyProcessor> logger,
    IValidator<ReceiveMoneyRequest> requestValidator,
    IValidator<PaymentCallback> callbackValidator) : IReceiveMoneyProcessor
{
    public async Task<Result<ReceiveMoneyResult>> InitAsync(
        ReceiveMoneyRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await requestValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            var error = validationResult.ToError();
            LogMessages.ErrorInitiatingPayment(logger, new ValidationException(validationResult.Errors), request.CustomerName ?? "Unknown");
            return Result.Failure<ReceiveMoneyResult>(error);
        }

        try
        {
            LogMessages.InitiatingPayment(logger, request.CustomerName ?? "Unknown", request.Amount, request.Channel);

            // Note: Validation ensures these are not null/empty, but we use null-forgiving operator
            // to satisfy the compiler since the validator guarantees these values are present
            var gatewayRequest = new HubtelReceiveMoneyRequest(
                CustomerName: request.CustomerName ?? string.Empty,
                CustomerMsisdn: request.CustomerMobileNumber, // Mandatory - validated
                CustomerEmail: string.Empty,
                Channel: request.Channel, // Mandatory - validated
                Amount: request.Amount.ToString("F2", CultureInfo.InvariantCulture),
                PrimaryCallbackEndpoint: request.PrimaryCallbackEndPoint!, // Mandatory - validated
                Description: request.Description, // Mandatory - validated
                ClientReference: request.ClientReference!); // Mandatory - validated

            var gatewayResponse = await client
                .ReceiveMobileMoneyAsync(gatewayRequest, cancellationToken)
                .ConfigureAwait(false);

            var decision = HubtelResponseDecisionFactory.Create(
                gatewayResponse.ResponseCode,
                gatewayResponse.Message);

            LogMessages.PaymentInitResponse(logger, decision.Code, decision.Category, decision.CustomerMessage ?? string.Empty);

            if (decision.Category == ResponseCategory.Pending)
            {
                var transactionId = gatewayResponse.Data?.TransactionId ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(transactionId))
                {
                    await pendingStore.AddAsync(transactionId, cancellationToken).ConfigureAwait(false);
                    LogMessages.TransactionAddedToPendingStore(logger, transactionId);
                }
            }

            if (!decision.IsSuccess && decision.IsFinal)
            {
                return Result.Failure<ReceiveMoneyResult>(
                    new Error(
                        $"Payment.{decision.Category}",
                        decision.CustomerMessage ?? "Payment initialization failed"));
            }

            return new ReceiveMoneyResult(
                TransactionId: gatewayResponse.Data?.TransactionId ?? string.Empty,
                CheckoutId: gatewayResponse.Data?.ClientReference ?? string.Empty,
                Status: decision.Category.ToString(),
                Message: decision.CustomerMessage ?? gatewayResponse.Message);
        }
        catch (Exception ex)
        {
            LogMessages.ErrorInitiatingPayment(logger, ex, request.CustomerName ?? "Unknown");
            return Result.Failure<ReceiveMoneyResult>(
                new Error("Payment.InitException", "An error occurred while initiating the payment"));
        }
    }

    public async Task<Result> ProcessCallbackAsync(
        PaymentCallback callback,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await callbackValidator.ValidateAsync(callback, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            var error = validationResult.ToError();
            LogMessages.ErrorProcessingCallback(logger, new ValidationException(validationResult.Errors), callback.TransactionId);
            return Result.Failure(error);
        }

        try
        {
            LogMessages.ProcessingCallback(logger, callback.TransactionId, callback.Status);

            var decision = HubtelResponseDecisionFactory.Create(
                callback.ResponseCode,
                callback.Status);

            LogMessages.CallbackDecision(logger, decision.Code, decision.Category, decision.IsFinal);

            if (decision.IsFinal)
            {
                await pendingStore.RemoveAsync(callback.TransactionId, cancellationToken).ConfigureAwait(false);
                LogMessages.TransactionRemovedFromPendingStore(logger, callback.TransactionId);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogMessages.ErrorProcessingCallback(logger, ex, callback.TransactionId);
            return Result.Failure(
                new Error("Payment.CallbackException", "An error occurred while processing the callback"));
        }
    }
}
