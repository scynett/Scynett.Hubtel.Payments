using FluentValidation;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Validation;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace Scynett.Hubtel.Payments.Features.Status;

public sealed class HubtelStatusService : IHubtelStatusService
{
    private readonly HttpClient _httpClient;
    private readonly HubtelSettings _options;
    private readonly ILogger<HubtelStatusService> _logger;
    private readonly IValidator<StatusRequest> _validator;

    public HubtelStatusService(
        HttpClient httpClient,
        IOptions<HubtelSettings> options,
        ILogger<HubtelStatusService> logger,
        IValidator<StatusRequest> validator)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<CheckStatusResponse>> CheckStatusAsync(
        StatusRequest query,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            var error = validationResult.ToError();
            var identifier = query.ClientReference ?? query.HubtelTransactionId ?? query.NetworkTransactionId ?? "Unknown";
            Log.ErrorCheckingStatus(_logger, new ValidationException(validationResult.Errors), identifier);
            return Result.Failure<CheckStatusResponse>(error);
        }

        try
        {
            // Set authorization header
            var authValue = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            // Build query string based on provided identifier
            var queryString = BuildQueryString(query);
            
            // Use Hubtel Status API endpoint
            var uri = new Uri(
                $"https://api-txnstatus.hubtel.com/transactions/{_options.MerchantAccountNumber}/status?{queryString}",
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
            var identifier = query.ClientReference ?? query.HubtelTransactionId ?? query.NetworkTransactionId ?? "Unknown";
            Log.ErrorCheckingStatus(_logger, ex, identifier);
            return Result.Failure<CheckStatusResponse>(
                new Error("Status.Exception", ex.Message));
        }
    }

    private static string BuildQueryString(StatusRequest query)
    {
        // ClientReference is preferred per Hubtel documentation
        if (!string.IsNullOrWhiteSpace(query.ClientReference))
            return $"clientReference={Uri.EscapeDataString(query.ClientReference)}";

        if (!string.IsNullOrWhiteSpace(query.HubtelTransactionId))
            return $"hubtelTransactionId={Uri.EscapeDataString(query.HubtelTransactionId)}";

        if (!string.IsNullOrWhiteSpace(query.NetworkTransactionId))
            return $"networkTransactionId={Uri.EscapeDataString(query.NetworkTransactionId)}";

        // This should never happen due to validation
        return string.Empty;
    }

    private sealed record HubtelStatusApiResponse(string? Message, HubtelStatusData? Data);
    private sealed record HubtelStatusData(
        string? TransactionId,
        string? Status,
        decimal? Amount,
        decimal? Charges,
        string? CustomerMobileNumber);
}
