using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Infrastructure.Configuration;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.Infrastructure.Gateways;

internal sealed class HubtelTransactionStatusGateway(
    IHubtelTransactionStatusApi api,
    IOptions<DirectReceiveMoneyOptions> directReceiveMoneyOptions)
    : IHubtelTransactionStatusGateway
{
    public async Task<OperationResult<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusQuery query,
        CancellationToken ct = default)
    {
        var posSalesId = directReceiveMoneyOptions.Value.PosSalesId;
        if (string.IsNullOrWhiteSpace(posSalesId))
        {
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem("Config.PosSalesId", "POS Sales ID is not configured."));
        }

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
                    response.Message ?? "Status check failed"));
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
}
