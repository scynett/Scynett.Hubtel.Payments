using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Storage;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

public sealed class ReceiveMoneyService : IReceiveMoneyService
{
    private readonly HttpClient _httpClient;
    private readonly HubtelOptions _options;
    private readonly IPendingTransactionsStore _pendingStore;
    private readonly ILogger<ReceiveMoneyService> _logger;

    public ReceiveMoneyService(
        HttpClient httpClient,
        IOptions<HubtelOptions> options,
        IPendingTransactionsStore pendingStore,
        ILogger<ReceiveMoneyService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _pendingStore = pendingStore;
        _logger = logger;
    }

    public async Task<Result<InitReceiveMoneyResponse>> InitAsync(
        InitReceiveMoneyCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var authValue = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            var requestBody = new
            {
                customerName = command.CustomerName,
                customerMobileNumber = command.CustomerMobileNumber,
                channel = command.Channel,
                amount = command.Amount,
                primaryCallbackUrl = command.PrimaryCallbackUrl,
                description = command.Description,
                clientReference = command.ClientReference
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_options.BaseUrl}/v2/merchantaccount/merchants/{_options.MerchantAccountNumber}/receive/mobilemoney",
                requestBody,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to initialize payment: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return Result.Failure<InitReceiveMoneyResponse>(
                    new Error("ReceiveMoney.InitFailed", $"Failed to initialize payment: {response.StatusCode}"));
            }

            var result = await response.Content.ReadFromJsonAsync<HubtelInitResponse>(cancellationToken);
            
            if (result == null)
            {
                return Result.Failure<InitReceiveMoneyResponse>(
                    new Error("ReceiveMoney.NullResponse", "Received null response from Hubtel API"));
            }

            var transactionId = result.Data?.TransactionId ?? string.Empty;
            
            if (!string.IsNullOrEmpty(transactionId))
            {
                await _pendingStore.AddAsync(transactionId, cancellationToken);
            }

            return new InitReceiveMoneyResponse(
                transactionId,
                result.Data?.CheckoutId ?? string.Empty,
                result.Data?.Status ?? string.Empty,
                result.Message ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing receive money transaction");
            return Result.Failure<InitReceiveMoneyResponse>(
                new Error("ReceiveMoney.Exception", ex.Message));
        }
    }

    public async Task<Result> ProcessCallbackAsync(
        ReceiveMoneyCallbackCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing callback for transaction {TransactionId} with status {Status}",
                command.TransactionId, command.Status);

            await _pendingStore.RemoveAsync(command.TransactionId, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing callback for transaction {TransactionId}",
                command.TransactionId);
            return Result.Failure(new Error("ReceiveMoney.CallbackFailed", ex.Message));
        }
    }

    private sealed record HubtelInitResponse(string? Message, HubtelInitData? Data);
    private sealed record HubtelInitData(string? TransactionId, string? CheckoutId, string? Status);
}
