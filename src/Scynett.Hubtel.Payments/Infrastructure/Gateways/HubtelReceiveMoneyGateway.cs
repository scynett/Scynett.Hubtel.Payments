using Polly.CircuitBreaker;
using Polly.Timeout;

using Refit;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways;
using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

using System.Globalization;
using System.Text.Json;

namespace Scynett.Hubtel.Payments.Infrastructure.Gateways;

/// <summary>
/// Infrastructure implementation of IHubtelReceiveMoneyGateway.
/// Responsible for HTTP, DTO mapping, and transport error handling.
/// </summary>
/// <remarks>
/// <para>
/// <b>Behaviour change (SDK hardening):</b> the Refit API returns <see cref="ApiResponse{T}"/>, which does
/// <b>not</b> throw on a non-2xx status. Previously a 4xx/5xx from Hubtel left <c>Content == null</c> and this
/// class threw <c>InvalidOperationException("Hubtel returned empty response body.")</c>, discarding the HTTP
/// status entirely (and making the <c>catch (ApiException)</c> branch largely unreachable).
/// The status is now inspected explicitly: any non-2xx response, transport failure, or missing body is mapped
/// to the transient <see cref="HubtelResponseDecisionFactory.HttpErrorCode"/> path and the real HTTP status is
/// carried on <see cref="GatewayInitiateReceiveMoneyResult.HttpStatusCode"/> so it is never lost.
/// </para>
/// </remarks>
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
                CustomerEmail: request.CustomerEmail,
                Amount: request.Amount,
                PrimaryCallbackUrl: request.CallbackUrl,
                Description: request.Description,
                ClientReference: request.ClientReference);

            using var response = await api.InitiateAsync(
                request.PosSalesId,
                dto,
                cancellationToken).ConfigureAwait(false);

            // ApiResponse<T> does NOT throw on non-2xx: inspect the status ourselves.
            if (!response.IsSuccessStatusCode)
            {
                return HttpErrorResult(
                    statusCode: (int)response.StatusCode,
                    message: DescribeFailure(response));
            }

            if (response.Content is null)
            {
                // 2xx but no/unparseable body - we still do not know the transaction state.
                return HttpErrorResult(
                    statusCode: (int)response.StatusCode,
                    message: "Hubtel returned an empty response body.");
            }

            var content = response.Content;

            return new GatewayInitiateReceiveMoneyResult(
                ResponseCode: content.ResponseCode,
                Message: content.Message,
                TransactionId: content.Data?.TransactionId,
                ExternalReference: content.Data?.ClientReference,
                ExternalTransactionId: content.Data?.ExternalTransactionId,
                OrderId: content.Data?.OrderId,
                Description: content.Data?.Description,
                Amount: content.Data?.Amount,
                Charges: content.Data?.Charges,
                AmountAfterCharges: content.Data?.AmountAfterCharges,
                AmountCharged: content.Data?.AmountCharged,
                DeliveryFee: content.Data?.DeliveryFee);
        }
        catch (ApiException ex)
        {
            // Defensive: Refit only throws this when the interface returns a bare T, but keep the mapping
            // correct in case the API surface changes.
            var parsed = TryParseError(ex.Content);

            return HttpErrorResult(
                statusCode: (int)ex.StatusCode,
                message: parsed?.Message ?? ex.Message);
        }
        catch (HttpRequestException ex)
        {
            // Pure transport failure: no response was ever received, so there is no status code to report.
            return HttpErrorResult(statusCode: null, message: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Client-side timeout: the request may well have reached Hubtel. Never final.
            return HttpErrorResult(statusCode: null, message: ex.Message);
        }
        catch (TimeoutRejectedException ex)
        {
            // The Polly timeout, NOT HttpClient's. AddHubtelPayments wraps this client in
            // AddStandardResilienceHandler, whose timeout strategy throws this rather than
            // TaskCanceledException — so without this catch, a timed-out request escaped as an
            // unhandled exception and the caller was told the payment FAILED. That is the precise
            // scenario this whole class exists to stop: Hubtel may already have debited the customer.
            return HttpErrorResult(statusCode: null, message: ex.Message);
        }
        catch (BrokenCircuitException ex)
        {
            // Same resilience pipeline, same reasoning. An open circuit means we did not send this
            // request — but "we did not send it" is still not "the payment failed", and the circuit
            // opened because earlier requests were failing, some of which may have reached Hubtel.
            // Non-final and retryable.
            return HttpErrorResult(statusCode: null, message: ex.Message);
        }
    }

    private static GatewayInitiateReceiveMoneyResult HttpErrorResult(int? statusCode, string? message)
    {
        var describedMessage = statusCode is null
            ? message
            : $"HTTP {statusCode.Value.ToString(CultureInfo.InvariantCulture)}: {message}";

        return new GatewayInitiateReceiveMoneyResult(
            ResponseCode: HubtelResponseDecisionFactory.HttpErrorCode,
            Message: describedMessage,
            TransactionId: null,
            ExternalReference: null,
            HttpStatusCode: statusCode);
    }

    private static string DescribeFailure(ApiResponse<InitiateReceiveMoneyResponseDto> response)
    {
        var parsed = TryParseError(response.Error?.Content);
        if (!string.IsNullOrWhiteSpace(parsed?.Message))
            return parsed.Message;

        if (!string.IsNullOrWhiteSpace(response.Error?.Message))
            return response.Error.Message;

        return response.ReasonPhrase ?? "Hubtel returned an unsuccessful HTTP status.";
    }

    private static HubtelApiErrorDto? TryParseError(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            return JsonSerializer.Deserialize<HubtelApiErrorDto>(content);
        }
        catch
        {
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
