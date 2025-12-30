using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Infrastructure.Configuration;
using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.Public.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.Infrastructure.BackgroundWorkers;

internal sealed class PendingTransactionsWorker(
    IPendingTransactionsStore store,
    IDirectReceiveMoney directReceiveMoney,
    IOptions<PendingTransactionsWorkerOptions> options,
    ILogger<PendingTransactionsWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollInterval = options.Value.PollInterval;

        PendingTransactionsWorkerLogMessages.Started(logger, pollInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);

            try
            {
                await Task.Delay(pollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Background worker must swallow exceptions and continue polling.")]
    internal async Task ProcessBatchAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        var callbackWait = options.Value.CallbackGracePeriod;

        try
        {
            var pending = await store.GetAllAsync(stoppingToken).ConfigureAwait(false);
            PendingTransactionsWorkerLogMessages.Polling(logger, pending.Count);

            foreach (var transaction in pending)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    if (DateTimeOffset.UtcNow - transaction.CreatedAtUtc < callbackWait)
                    {
                        PendingTransactionsWorkerLogMessages.TooEarly(logger, transaction.HubtelTransactionId ?? "unknown");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(transaction.HubtelTransactionId))
                    {
                        PendingTransactionsWorkerLogMessages.StatusFailed(
                            logger,
                            "unknown",
                            "MissingTransactionId",
                            "Pending transaction is missing HubtelTransactionId.");
                        continue;
                    }

                    var query = new TransactionStatusQuery(HubtelTransactionId: transaction.HubtelTransactionId);

                    var result = await directReceiveMoney
                        .CheckStatusAsync(query, stoppingToken)
                        .ConfigureAwait(false);

                    if (result.IsFailure)
                    {
                        PendingTransactionsWorkerLogMessages.StatusFailed(
                            logger,
                            transaction.HubtelTransactionId,
                            result.Error?.Code,
                            result.Error?.Description);

                        continue;
                    }

                    var status = result.Value?.Status ?? "Unknown";

                    if (IsFinal(status))
                    {
                        await store.RemoveAsync(transaction.HubtelTransactionId, stoppingToken).ConfigureAwait(false);
                        PendingTransactionsWorkerLogMessages.Completed(logger, transaction.HubtelTransactionId, status);
                    }
                    else
                    {
                        PendingTransactionsWorkerLogMessages.StillPending(logger, transaction.HubtelTransactionId, status);
                    }
                }
                catch (Exception ex)
                {
                    PendingTransactionsWorkerLogMessages.ProcessingError(logger, ex, transaction.HubtelTransactionId ?? "unknown");
                }
            }
        }
        catch (Exception ex)
        {
            PendingTransactionsWorkerLogMessages.LoopError(logger, ex);
        }
    }

    private static bool IsFinal(string status)
        => status.Equals("success", StringComparison.OrdinalIgnoreCase)
        || status.Equals("failed", StringComparison.OrdinalIgnoreCase)
        || status.Equals("paid", StringComparison.OrdinalIgnoreCase)
        || status.Equals("unpaid", StringComparison.OrdinalIgnoreCase)
        || status.Equals("refunded", StringComparison.OrdinalIgnoreCase);
}
