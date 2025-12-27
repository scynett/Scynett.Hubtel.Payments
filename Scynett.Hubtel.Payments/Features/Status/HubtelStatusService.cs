using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Configuration;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace Scynett.Hubtel.Payments.Features.Status;

public sealed class HubtelStatusService : IHubtelStatusService
{
    private readonly HttpClient _httpClient;
    private readonly HubtelSettings _options;
    private readonly ILogger<HubtelStatusService> _logger;

    public HubtelStatusService(
        HttpClient httpClient,
        IOptions<HubtelSettings> options,
        ILogger<HubtelStatusService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<CheckStatusResponse>> CheckStatusAsync(
        StatusRequest query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authValue = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            var uri = new Uri(
                $"{_options.BaseUrl}/v2/merchantaccount/merchants/{_options.MerchantAccountNumber}/transactions/{query.TransactionId}",
                UriKind.Absolute);

            var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                Log.FailedToCheckStatus(_logger, response.StatusCode, errorContent);
                return Result.Failure<CheckStatusResponse>(
                    new Error("Status.CheckFailed", $"Failed to check status: {response.StatusCode}"));
            }

            var result = await response.Content.ReadFromJsonAsync<HubtelStatusApiResponse>(cancellationToken).ConfigureAwait(false);

            if (result == null || result.Data == null)
            {
                return Result.Failure<CheckStatusResponse>(
                    new Error("Status.NullResponse", "Received null response from Hubtel API"));
            }

            return new CheckStatusResponse(
                result.Data.TransactionId ?? string.Empty,
                result.Data.Status ?? string.Empty,
                result.Message ?? string.Empty,
                result.Data.Amount ?? 0,
                result.Data.Charges ?? 0,
                result.Data.CustomerMobileNumber ?? string.Empty);
        }
        catch (Exception ex)
        {
            Log.ErrorCheckingStatus(_logger, ex, query.TransactionId);
            return Result.Failure<CheckStatusResponse>(
                new Error("Status.Exception", ex.Message));
        }
    }

    private sealed record HubtelStatusApiResponse(string? Message, HubtelStatusData? Data);
    private sealed record HubtelStatusData(
        string? TransactionId,
        string? Status,
        decimal? Amount,
        decimal? Charges,
        string? CustomerMobileNumber);
}
