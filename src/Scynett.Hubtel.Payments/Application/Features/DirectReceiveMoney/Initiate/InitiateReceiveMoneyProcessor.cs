using System.Globalization;

using FluentValidation;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.Storage;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

internal sealed class InitiateReceiveMoneyProcessor(
    IHubtelReceiveMoneyGateway gateway,
    IPendingTransactionsStore pendingStore,
    IOptions<HubtelOptions> hubtelOptions,
    IOptions<DirectReceiveMoneyOptions> directReceiveMoneyOptions,
    IValidator<InitiateReceiveMoneyRequest> validator,
    ILogger<InitiateReceiveMoneyProcessor> logger)
{
    public async Task<OperationResult<InitiateReceiveMoneyResult>> ExecuteAsync(
        InitiateReceiveMoneyRequest request,
        CancellationToken ct = default)
    {
        // 1) Validate
        var validation = await validator.ValidateAsync(request, ct).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            var message = validation.Errors.Count > 0
                ? validation.Errors[0].ErrorMessage
                : "Validation failed.";

            InitiateReceiveMoneyLogMessages.ValidationFailed(
                logger,
                request.ClientReference,
                message);

            return OperationResult<InitiateReceiveMoneyResult>.Failure(
                Error.Validation("DirectReceiveMoney.ValidationFailed", message));
        }

        try
        {
            InitiateReceiveMoneyLogMessages.Initiating(
                logger,
                request.ClientReference,
                request.Amount,
                request.Channel,
                MaskMsisdn(request.CustomerMobileNumber));

            // Determine which POS Sales ID to use
            var posSalesId = !string.IsNullOrWhiteSpace(directReceiveMoneyOptions.Value.PosSalesId)
                ? directReceiveMoneyOptions.Value.PosSalesId
                : hubtelOptions.Value.MerchantAccountNumber;

            if (string.IsNullOrWhiteSpace(posSalesId))
            {
                return OperationResult<InitiateReceiveMoneyResult>.Failure(
                    Error.Validation(
                        "DirectReceiveMoney.MissingPosSalesId",
                        "POS Sales ID is not configured. Please set either HubtelOptions.MerchantAccountNumber or DirectReceiveMoneyOptions.PosSalesIdOverride."));
            }

            // 2) Map request -> gateway request
            var gatewayRequest = new GatewayInitiateReceiveMoneyRequest(
                CustomerName: request.CustomerName ?? string.Empty,
                PosSalesId: posSalesId,
                CustomerMsisdn: request.CustomerMobileNumber,
                CustomerEmail: request.CustomerEmail ?? string.Empty,
                Channel: request.Channel,
                Amount: request.Amount.ToString("F2", CultureInfo.InvariantCulture),
                CallbackUrl: request.PrimaryCallbackEndPoint,
                Description: request.Description,
                ClientReference: request.ClientReference);

            // 3) Call gateway - returns GatewayInitiateReceiveMoneyResult directly
            var gatewayResponse = await gateway.InitiateAsync(gatewayRequest, ct)
                .ConfigureAwait(false);

            // 4) Decision (your factory)
            var decision = HubtelResponseDecisionFactory.Create(
                gatewayResponse.ResponseCode,
                gatewayResponse.Message);

            InitiateReceiveMoneyLogMessages.DecisionComputed(
                logger,
                request.ClientReference,
                decision.Code,
                decision.Category.ToString(),
                decision.NextAction.ToString(),
                decision.IsFinal);

            // 5) Persist pending when waiting for callback
            if (decision.NextAction == NextAction.WaitForCallback)
            {
                if (!string.IsNullOrWhiteSpace(gatewayResponse.TransactionId))
                {
                    await pendingStore.AddAsync(
                        gatewayResponse.TransactionId,
                        DateTimeOffset.UtcNow,
                        ct).ConfigureAwait(false);

                    InitiateReceiveMoneyLogMessages.PendingStored(
                        logger,
                        request.ClientReference,
                        gatewayResponse.TransactionId);
                }
                else
                {
                    InitiateReceiveMoneyLogMessages.PendingButMissingTransactionId(
                        logger,
                        request.ClientReference,
                        decision.Code);
                    // Just logginthis .What do I have to do????
                }
            }

            // 6) Build result using mapper
            var result = InitiateReceiveMoneyMapping.ToResult(
                request,
                gatewayResponse,
                decision);

            // 7) Check if this is a failure scenario that should return failure
            if (!decision.IsSuccess && decision.IsFinal)
            {
                InitiateReceiveMoneyLogMessages.GatewayFailed(
                    logger,
                    request.ClientReference,
                    gatewayResponse.ResponseCode,
                    gatewayResponse.Message ?? "No message provided");

                return OperationResult<InitiateReceiveMoneyResult>.Failure(
                    Error.Failure(
                        $"DirectReceiveMoney.{decision.Category}",
                        decision.CustomerMessage ?? gatewayResponse.Message ?? "Payment initialization failed"));
            }

            // SDK-friendly: Success if we got a Hubtel response; consumer interprets decision via result
            return OperationResult<InitiateReceiveMoneyResult>.Success(result);
        }
#pragma warning disable CA1031 // Do not catch general exception types - Required for SDK error handling
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            InitiateReceiveMoneyLogMessages.UnhandledException(
                logger,
                ex,
                request.ClientReference);

            return OperationResult<InitiateReceiveMoneyResult>.Failure(
                Error.Problem(
                    "DirectReceiveMoney.UnhandledException",
                    "An unexpected error occurred while initiating the payment."));
        }
    }

    private static string DetermineStatus(HandlingDecision decision)
    {
        if (decision.IsSuccess)
            return "Success";

        if (decision.IsFinal)
            return "Failed";

        return "Pending";
    }

    private static string MaskMsisdn(string msisdn)
    {
        if (string.IsNullOrWhiteSpace(msisdn) || msisdn.Length < 6)
            return "****";

        // 0241234567 -> 024***567
        var prefix = msisdn[..3];
        var suffix = msisdn[^3..];
        return $"{prefix}***{suffix}";
    }
}

