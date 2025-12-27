using FluentValidation;

using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.InitPayment;
using Scynett.Hubtel.Payments.Storage;
using Scynett.Hubtel.Payments.Validation;

using System.Globalization;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

public sealed class ReceiveMobileMoneyService(
    IReceiveMobileMoneyApi api,
    IPendingTransactionsStore pendingStore,
    ILogger<ReceiveMobileMoneyService> logger,
    IValidator<InitPaymentRequest> initPaymentValidator,
    IValidator<PaymentCallback> callbackValidator) : IReceiveMoneyService
{
    public async Task<Result<InitPaymentResponse>> InitAsync(
        InitPaymentRequest command,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await initPaymentValidator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            var error = validationResult.ToError();
            Log.ErrorInitiatingPayment(logger, new ValidationException(validationResult.Errors), command.CustomerName ?? "Unknown");
            return Result.Failure<InitPaymentResponse>(error);
        }

        try
        {
            Log.InitiatingPayment(logger, command.CustomerName ?? "Unknown", command.Amount, command.Channel);

            // Note: Validation ensures these are not null/empty, but we use null-forgiving operator
            // to satisfy the compiler since the validator guarantees these values are present
            var gatewayRequest = new ReceiveMobileMoneyGatewayRequest(
                CustomerName: command.CustomerName ?? string.Empty,
                CustomerMsisdn: command.CustomerMobileNumber, // Mandatory - validated
                CustomerEmail: string.Empty,
                Channel: command.Channel, // Mandatory - validated
                Amount: command.Amount.ToString("F2", CultureInfo.InvariantCulture),
                PrimaryCallbackEndpoint: command.PrimaryCallbackEndPoint!, // Mandatory - validated
                Description: command.Description, // Mandatory - validated
                ClientReference: command.ClientReference!); // Mandatory - validated

            var gatewayResponse = await api
                .ReceiveMobileMoneyAsync(gatewayRequest, cancellationToken)
                .ConfigureAwait(false);

            var decision = HubtelResponseDecisionFactory.Create(
                gatewayResponse.ResponseCode,
                gatewayResponse.Message);

            Log.PaymentInitResponse(logger, decision.Code, decision.Category, decision.CustomerMessage ?? string.Empty);

            if (decision.Category == ResponseCategory.Pending)
            {
                var transactionId = gatewayResponse.Data?.TransactionId ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(transactionId))
                {
                    await pendingStore.AddAsync(transactionId, cancellationToken).ConfigureAwait(false);
                    Log.TransactionAddedToPendingStore(logger, transactionId);
                }
            }

            if (!decision.IsSuccess && decision.IsFinal)
            {
                return Result.Failure<InitPaymentResponse>(
                    new Error(
                        $"Payment.{decision.Category}",
                        decision.CustomerMessage ?? "Payment initialization failed"));
            }

            return new InitPaymentResponse(
                TransactionId: gatewayResponse.Data?.TransactionId ?? string.Empty,
                CheckoutId: gatewayResponse.Data?.ClientReference ?? string.Empty,
                Status: decision.Category.ToString(),
                Message: decision.CustomerMessage ?? gatewayResponse.Message);
        }
        catch (Exception ex)
        {
            Log.ErrorInitiatingPayment(logger, ex, command.CustomerName ?? "Unknown");
            return Result.Failure<InitPaymentResponse>(
                new Error("Payment.InitException", "An error occurred while initiating the payment"));
        }
    }

    public async Task<Result> ProcessCallbackAsync(
        PaymentCallback command,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await callbackValidator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            var error = validationResult.ToError();
            Log.ErrorProcessingCallback(logger, new ValidationException(validationResult.Errors), command.TransactionId);
            return Result.Failure(error);
        }

        try
        {
            Log.ProcessingCallback(logger, command.TransactionId, command.Status);

            var decision = HubtelResponseDecisionFactory.Create(
                command.ResponseCode,
                command.Status);

            Log.CallbackDecision(logger, decision.Code, decision.Category, decision.IsFinal);

            if (decision.IsFinal)
            {
                await pendingStore.RemoveAsync(command.TransactionId, cancellationToken).ConfigureAwait(false);
                Log.TransactionRemovedFromPendingStore(logger, command.TransactionId);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            Log.ErrorProcessingCallback(logger, ex, command.TransactionId);
            return Result.Failure(
                new Error("Payment.CallbackException", "An error occurred while processing the callback"));
        }
    }
}
