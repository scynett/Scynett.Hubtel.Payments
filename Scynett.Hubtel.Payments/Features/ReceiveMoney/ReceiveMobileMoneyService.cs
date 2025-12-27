using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;
using Scynett.Hubtel.Payments.Storage;

using System.Globalization;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

public sealed class ReceiveMobileMoneyService(IReceiveMobileMoneyApi api,
        IOptions<HubtelSettings> settings,
        IPendingTransactionsStore pendingStore,
        ILogger<ReceiveMobileMoneyService> logger) : IReceiveMoneyService
{
    public async Task<Result<InitPaymentResponse>> InitAsync(
        InitPaymentRequest command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Log.InitiatingPayment(logger, command.CustomerName, command.Amount, command.Channel);

            var gatewayRequest = new ReceiveMobileMoneyGatewayRequest(
                CustomerName: command.CustomerName,
                CustomerMsisdn: command.CustomerMobileNumber,
                CustomerEmail: string.Empty,
                Channel: command.Channel,
                Amount: command.Amount.ToString("F2", CultureInfo.InvariantCulture),
                PrimaryCallbackEndpoint: command.PrimaryCallbackEndPoint ?? settings.Value.PrimaryCallbackEndPoint,
                Description: command.Description,
                ClientReference: command.ClientReference ?? Guid.NewGuid().ToString());

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
            Log.ErrorInitiatingPayment(logger, ex, command.CustomerName);
            return Result.Failure<InitPaymentResponse>(
                new Error("Payment.InitException", "An error occurred while initiating the payment"));
        }
    }

    public async Task<Result> ProcessCallbackAsync(
        PaymentCallback command,
        CancellationToken cancellationToken = default)
    {
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
