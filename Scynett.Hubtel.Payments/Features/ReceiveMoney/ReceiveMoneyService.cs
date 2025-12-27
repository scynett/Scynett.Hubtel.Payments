using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;
using Scynett.Hubtel.Payments.Storage;

using System.Globalization;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

public sealed class ReceiveMoneyService : IReceiveMoneyService
{
    private readonly IReceiveMobileMoneyApi _api;
    private readonly HubtelSettings _settings;
    private readonly IPendingTransactionsStore _pendingStore;
    private readonly ILogger<ReceiveMoneyService> _logger;

    public ReceiveMoneyService(
        IReceiveMobileMoneyApi api,
        IOptions<HubtelSettings> settings,
        IPendingTransactionsStore pendingStore,
        ILogger<ReceiveMoneyService> logger)
    {
        _api = api;
        _settings = settings.Value;
        _pendingStore = pendingStore;
        _logger = logger;
    }

    public async Task<Result<InitPaymentResponse>> InitAsync(
        InitPaymentRequest command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Initiating payment for {CustomerName} - Amount: {Amount}, Channel: {Channel}",
                command.CustomerName, command.Amount, command.Channel);

            var gatewayRequest = new ReceiveMobileMoneyRequest(
                CustomerName: command.CustomerName,
                CustomerMsisdn: command.CustomerMobileNumber,
                CustomerEmail: string.Empty,
                Channel: command.Channel,
                Amount: command.Amount.ToString("F2", CultureInfo.InvariantCulture),
                PrimaryCallbackEndpoint: command.PrimaryCallbackEndPoint ?? _settings.PrimaryCallbackEndPoint,
                Description: command.Description,
                ClientReference: command.ClientReference ?? Guid.NewGuid().ToString());

            var gatewayResponse = await _api
                .ReceiveMobileMoneyAsync(gatewayRequest)
                .ConfigureAwait(false);

            var decision = HubtelResponseDecisionFactory.Create(
                gatewayResponse.ResponseCode,
                gatewayResponse.Message);

            _logger.LogInformation(
                "Payment init response - Code: {Code}, Category: {Category}, Message: {Message}",
                decision.Code, decision.Category, decision.CustomerMessage);

            if (decision.Category == ResponseCategory.Pending)
            {
                var transactionId = gatewayResponse.Data?.TransactionId ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(transactionId))
                {
                    await _pendingStore.AddAsync(transactionId, cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("Transaction {TransactionId} added to pending store", transactionId);
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
            _logger.LogError(ex, "Error initiating payment for {CustomerName}", command.CustomerName);
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
            _logger.LogInformation(
                "Processing callback for transaction {TransactionId} - Status: {Status}",
                command.TransactionId, command.Status);

            var decision = HubtelResponseDecisionFactory.Create(
                command.ResponseCode,
                command.Status);

            _logger.LogInformation(
                "Callback decision - Code: {Code}, Category: {Category}, IsFinal: {IsFinal}",
                decision.Code, decision.Category, decision.IsFinal);

            if (decision.IsFinal)
            {
                await _pendingStore.RemoveAsync(command.TransactionId, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation(
                    "Transaction {TransactionId} removed from pending store",
                    command.TransactionId);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing callback for transaction {TransactionId}", command.TransactionId);
            return Result.Failure(
                new Error("Payment.CallbackException", "An error occurred while processing the callback"));
        }
    }
}
