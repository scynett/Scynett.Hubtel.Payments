using System.Globalization;

using Microsoft.Extensions.Options;

using Refit;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.Infrastructure.Gateways;

internal sealed class HubtelTransactionStatusGateway(
    IHubtelTransactionStatusApi api,
    IOptions<DirectReceiveMoneyOptions> directReceiveMoneyOptions,
    IOptions<HubtelOptions> hubtelOptions)
    : IHubtelTransactionStatusGateway
{
    public async Task<OperationResult<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusQuery query,
        CancellationToken ct = default)
    {
        var posSalesId = !string.IsNullOrWhiteSpace(directReceiveMoneyOptions.Value.PosSalesId)
            ? directReceiveMoneyOptions.Value.PosSalesId
            : hubtelOptions.Value.MerchantAccountNumber;

        if (string.IsNullOrWhiteSpace(posSalesId))
        {
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem("Config.PosSalesId", "POS Sales ID is not configured."));
        }

        try
        {
            var response = await api.GetStatusAsync(
                posSalesId,
                clientReference: query.ClientReference,
                hubtelTransactionId: query.HubtelTransactionId,
                networkTransactionId: query.NetworkTransactionId,
                ct).ConfigureAwait(false);

            if (!string.Equals(response.ResponseCode, "0000", StringComparison.OrdinalIgnoreCase))
            {
                return OperationResult<TransactionStatusResult>.Failure(
                    Error.Failure(
                            "Hubtel.StatusCheckFailed",
                            response.Message ?? "Status check failed")
                        .WithProvider(response.ResponseCode, response.Message));
            }

            var data = response.Data;

            return OperationResult<TransactionStatusResult>.Success(
                new TransactionStatusResult(
                    Status: data?.Status ?? "Unknown",
                    ClientReference: data?.ClientReference,
                    TransactionId: data?.TransactionId,
                    ExternalTransactionId: data?.ExternalTransactionId,
                    PaymentMethod: data?.PaymentMethod,
                    CurrencyCode: data?.CurrencyCode,
                    IsFulfilled: data?.IsFulfilled,
                    Amount: data?.Amount,
                    Charges: data?.Charges,
                    AmountAfterCharges: data?.AmountAfterCharges,
                    Date: data?.Date,
                    RawResponseCode: response.ResponseCode,
                    RawMessage: response.Message));
        }
        catch (ApiException apiEx)
        {
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem("Hubtel.Http.Status",
                        "Failed to contact Hubtel status endpoint.")
                    .WithMetadata("statusCode", ((int)apiEx.StatusCode).ToString(CultureInfo.InvariantCulture))
                    .WithMetadata("reason", apiEx.ReasonPhrase ?? "unknown"));
        }
#pragma warning disable CA1031 // We intentionally convert all exceptions into OperationResult failures for transport resiliency
        catch (Exception ex)
        {
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem("Hubtel.Http.Status",
                        "Failed to contact Hubtel status endpoint.")
                    .WithMetadata("exception", ex.GetType().Name));
        }
#pragma warning restore CA1031
    }
}

