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
        Log.WorkerStarted(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPendingTransactionsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.ErrorCheckingPendingTransactions(_logger, ex);
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        Log.WorkerStopped(_logger);
    }

    private async Task CheckPendingTransactionsAsync(CancellationToken cancellationToken)
    {
        var pendingTransactions = await _pendingStore.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var transactionList = pendingTransactions.ToList();

        if (transactionList.Count == 0)
        {
            Log.NoPendingTransactions(_logger);
            return;
        }

        Log.CheckingPendingTransactions(_logger, transactionList.Count);

        foreach (var transactionId in transactionList)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Use ByHubtelTransactionId since we're querying by Hubtel-generated transaction ID
                var statusResult = await _statusService.CheckStatusAsync(
                    StatusRequest.ByHubtelTransactionId(transactionId),
                    cancellationToken).ConfigureAwait(false);

                if (statusResult.IsFailure)
                {
                    Log.FailedToCheckTransactionStatus(_logger, transactionId, statusResult.Error.Message);
                    continue;
                }

                var transactionStatus = statusResult.Value.Status.ToUpperInvariant();

                if (transactionStatus is "SUCCESS" or "SUCCESSFUL" or "FAILED" or "CANCELLED")
                {
                    Log.TransactionCompleted(_logger, transactionId, transactionStatus);

                    var callbackCommand = new PaymentCallback(
                        transactionStatus == "SUCCESS" || transactionStatus == "SUCCESSFUL" ? "0000" : "9999",
                        transactionStatus,
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
                break;
            }
            catch (Exception ex)
            {
                Log.ErrorCheckingTransaction(_logger, ex, transactionId);
            }
        }
    }
}
