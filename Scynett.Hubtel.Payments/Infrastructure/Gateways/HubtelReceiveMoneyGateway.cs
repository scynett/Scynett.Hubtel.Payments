using Refit;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

using System.Text.Json;

namespace Scynett.Hubtel.Payments.Infrastructure.Gateways;

/// <summary>
/// Infrastructure implementation of IHubtelReceiveMoneyGateway.
/// Responsible for HTTP, DTO mapping, and transport error handling.
/// </summary>
internal sealed class HubtelReceiveMoneyGateway(
    IHubtelDirectReceiveMoneyApi api)
    : IHubtelReceiveMoneyGateway
{
    public async Task<GatewayInitiateReceiveMoneyResult> InitiateAsync(
        GatewayInitiateReceiveMoneyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = new InitiateReceiveMoneyRequestDto(
                CustomerName: request.CustomerName,
                CustomerMsisdn: request.CustomerMsisdn,
                Channel: request.Channel,
                CustomerEmail: request.CustomeeEmail,
                Amount: request.Amount,
                PrimaryCallbackEndpoint: request.CallbackUrl,
                Description: request.Description,
                ClientReference: request.ClientReference);

            var response = await api.InitiateAsync(
                request.PosSalesId,
                dto,
                cancellationToken).ConfigureAwait(false);

            var content = response.Content
                ?? throw new InvalidOperationException("Hubtel returned empty response body.");

            return new GatewayInitiateReceiveMoneyResult(
                ResponseCode: content.ResponseCode,
                Message: content.Message,
                TransactionId: content.Data?.TransactionId,
                ExternalReference: content.Data?.ClientReference,
                Description: content.Data?.Description,
                Amount: content.Data?.Amount,
                Charges: content.Data?.Charges,
                AmountAfterCharges: content.Data?.AmountAfterCharges,
                AmountCharged: content.Data?.AmountCharged,
                DeliveryFee: content.Data?.DeliveryFee);
        }
        catch (ApiException ex)
        {
            var parsed = TryParseError(ex);

            return new GatewayInitiateReceiveMoneyResult(
                ResponseCode: "HTTP_ERROR",
                Message: parsed?.Message ?? ex.Message,
                TransactionId: null,
                ExternalReference: null);
        }
    }

    private static HubtelApiErrorDto? TryParseError(ApiException ex)
    {
        if (string.IsNullOrWhiteSpace(ex.Content))
            return null;

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            return JsonSerializer.Deserialize<HubtelApiErrorDto>(ex.Content);
        }
        catch
        {
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}