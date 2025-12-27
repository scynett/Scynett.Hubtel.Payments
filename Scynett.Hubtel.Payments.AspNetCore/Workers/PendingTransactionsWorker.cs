using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;
using Scynett.Hubtel.Payments.Features.Status;
using Scynett.Hubtel.Payments.Storage;

namespace Scynett.Hubtel.Payments.AspNetCore.Workers;

public sealed class PendingTransactionsWorker : BackgroundService
{
    private readonly IPendingTransactionsStore _pendingStore;
    private readonly IHubtelStatusService _statusService;
    private readonly IReceiveMoneyService _receiveMoneyService;
    private readonly ILogger<PendingTransactionsWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public PendingTransactionsWorker(
        IPendingTransactionsStore pendingStore,
        IHubtelStatusService statusService,
        IReceiveMoneyService receiveMoneyService,
        ILogger<PendingTransactionsWorker> logger)
    {
        _pendingStore = pendingStore;
        _statusService = statusService;
        _receiveMoneyService = receiveMoneyService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Pending Transactions Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPendingTransactionsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking pending transactions");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
        }

        _logger.LogInformation("Pending Transactions Worker stopped");
    }

    private async Task CheckPendingTransactionsAsync(CancellationToken cancellationToken)
    {
        var pendingTransactions = await _pendingStore.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var transactionList = pendingTransactions.ToList();

        if (transactionList.Count == 0)
        {
            _logger.LogDebug("No pending transactions to check");
            return;
        }

        _logger.LogInformation("Checking {Count} pending transactions", transactionList.Count);

        foreach (var transactionId in transactionList)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var statusResult = await _statusService.CheckStatusAsync(
                    new StatusRequest(transactionId),
                    cancellationToken).ConfigureAwait(false);

                if (statusResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Failed to check status for transaction {TransactionId}: {Error}",
                        transactionId, statusResult.Error.Message);
                    continue;
                }

                var status = statusResult.Value.Status.ToUpperInvariant();

                if (status is "SUCCESS" or "SUCCESSFUL" or "FAILED" or "CANCELLED")
                {
                    _logger.LogInformation(
                        "Transaction {TransactionId} completed with status: {Status}",
                        transactionId, status);

                    var callbackCommand = new PaymentCallback(
                        status == "SUCCESS" || status == "SUCCESSFUL" ? "0000" : "9999",
                        status,
                        transactionId,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        statusResult.Value.Amount,
                        statusResult.Value.Charges,
                        statusResult.Value.CustomerMobileNumber);

                    await _receiveMoneyService.ProcessCallbackAsync(callbackCommand, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking transaction {TransactionId}", transactionId);
            }
        }
    }
}
