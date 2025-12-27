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

namespace Scynett.Hubtel.Payments.Features.TransactionStatus;

/// <summary>
/// Processor for checking Hubtel transaction status.
/// </summary>
public sealed class TransactionStatusProcessor : ITransactionStatusProcessor
{
    private readonly HttpClient _httpClient;
    private readonly HubtelOptions _options;
    private readonly ILogger<TransactionStatusProcessor> _logger;
    private readonly IValidator<TransactionStatusRequest> _validator;

    public TransactionStatusProcessor(
        HttpClient httpClient,
        IOptions<HubtelOptions> options,
        ILogger<TransactionStatusProcessor> logger,
        IValidator<TransactionStatusRequest> validator)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            var error = validationResult.ToError();
            var identifier = request.ClientReference ?? request.HubtelTransactionId ?? request.NetworkTransactionId ?? "Unknown";
            LogMessages.ErrorCheckingStatus(_logger, new ValidationException(validationResult.Errors), identifier);
            return Result.Failure<TransactionStatusResult>(error);
        }

        try
        {
            // Set authorization header
            var authValue = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            // Build query string based on provided identifier
            var queryString = BuildQueryString(request);
            
            // Use Hubtel Status API endpoint
            var uri = new Uri(
                $"https://api-txnstatus.hubtel.com/transactions/{_options.MerchantAccountNumber}/status?{queryString}",
                UriKind.Absolute);

            var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                LogMessages.FailedToCheckStatus(_logger, response.StatusCode, errorContent);
                return Result.Failure<TransactionStatusResult>(
                    new Error("Status.CheckFailed", $"Failed to check status: {response.StatusCode}"));
            }

            var result = await response.Content.ReadFromJsonAsync<HubtelStatusApiResponse>(cancellationToken).ConfigureAwait(false);

            if (result == null || result.Data == null)
            {
                return Result.Failure<TransactionStatusResult>(
                    new Error("Status.NullResponse", "Received null response from Hubtel API"));
            }

            return new TransactionStatusResult(
                result.Data.TransactionId ?? string.Empty,
                result.Data.Status ?? string.Empty,
                result.Message ?? string.Empty,
                result.Data.Amount ?? 0,
                result.Data.Charges ?? 0,
                result.Data.CustomerMobileNumber ?? string.Empty);
        }
        catch (Exception ex)
        {
            var identifier = request.ClientReference ?? request.HubtelTransactionId ?? request.NetworkTransactionId ?? "Unknown";
            LogMessages.ErrorCheckingStatus(_logger, ex, identifier);
            return Result.Failure<TransactionStatusResult>(
                new Error("Status.Exception", ex.Message));
        }
    }

    private static string BuildQueryString(TransactionStatusRequest request)
    {
        // ClientReference is preferred per Hubtel documentation
        if (!string.IsNullOrWhiteSpace(request.ClientReference))
            return $"clientReference={Uri.EscapeDataString(request.ClientReference)}";

        if (!string.IsNullOrWhiteSpace(request.HubtelTransactionId))
            return $"hubtelTransactionId={Uri.EscapeDataString(request.HubtelTransactionId)}";

        if (!string.IsNullOrWhiteSpace(request.NetworkTransactionId))
            return $"networkTransactionId={Uri.EscapeDataString(request.NetworkTransactionId)}";

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
