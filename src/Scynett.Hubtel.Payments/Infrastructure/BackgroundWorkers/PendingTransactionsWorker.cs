using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.Options;

using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Scynett.Hubtel.Payments.Infrastructure.BackgroundWorkers;

internal sealed class PendingTransactionsWorker : BackgroundService
{
    private readonly IPendingTransactionsStore _store;
    private readonly IOptions<PendingTransactionsWorkerOptions> _options;
    private readonly ILogger<PendingTransactionsWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PendingTransactionsWorker(
        IPendingTransactionsStore store,
        IOptions<PendingTransactionsWorkerOptions> options,
        ILogger<PendingTransactionsWorker> logger,
        IServiceProvider serviceProvider)
    {
        _store = store;
        _options = options;
        _logger = logger;
        _serviceProvider = serviceProvider; // Store the service provider
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollInterval = _options.Value.PollInterval;

        PendingTransactionsWorkerLogMessages.Started(_logger, pollInterval);

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

        var callbackWait = _options.Value.CallbackGracePeriod;

        try
        {
            var pending = await _store.GetAllAsync(stoppingToken).ConfigureAwait(false);
            PendingTransactionsWorkerLogMessages.Polling(_logger, pending.Count);

            var batchSize = _options.Value.BatchSize;
            var items = batchSize > 0 ? pending.Take(batchSize) : pending;

            foreach (var transaction in items)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    if (DateTimeOffset.UtcNow - transaction.CreatedAtUtc < callbackWait)
                    {
                        PendingTransactionsWorkerLogMessages.TooEarly(_logger, transaction.HubtelTransactionId);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(transaction.HubtelTransactionId))
                    {
                        PendingTransactionsWorkerLogMessages.StatusFailed(
                            _logger,
                            "unknown",
                            "MissingTransactionId",
                            "Pending transaction is missing HubtelTransactionId.");
                        continue;
                    }

                    var query = new TransactionStatusQuery(HubtelTransactionId: transaction.HubtelTransactionId);

                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var directReceiveMoney = scope.ServiceProvider.GetRequiredService<IDirectReceiveMoney>();

                        var result = await directReceiveMoney
                            .CheckStatusAsync(query, stoppingToken)
                            .ConfigureAwait(false);

                        if (result.IsFailure)
                        {
                            PendingTransactionsWorkerLogMessages.StatusFailed(
                                _logger,
                                transaction.HubtelTransactionId,
                                result.Error?.Code,
                                result.Error?.Description);

                            continue;
                        }

                        var status = result.Value?.Status ?? "Unknown";

                        if (IsFinal(status))
                        {
                            await _store.RemoveAsync(transaction.HubtelTransactionId, stoppingToken).ConfigureAwait(false);
                            PendingTransactionsWorkerLogMessages.Completed(_logger, transaction.HubtelTransactionId, status);
                        }
                        else
                        {
                            PendingTransactionsWorkerLogMessages.StillPending(_logger, transaction.HubtelTransactionId, status);
                        }
                    }
                }
                catch (Exception ex)
                {
                    PendingTransactionsWorkerLogMessages.ProcessingError(_logger, ex, transaction.HubtelTransactionId);
                }
            }
        }
        catch (Exception ex)
        {
            PendingTransactionsWorkerLogMessages.LoopError(_logger, ex);
        }
    }

    private static bool IsFinal(string status)
        => status.Equals("success", StringComparison.OrdinalIgnoreCase)
        || status.Equals("failed", StringComparison.OrdinalIgnoreCase)
        || status.Equals("paid", StringComparison.OrdinalIgnoreCase)
        || status.Equals("unpaid", StringComparison.OrdinalIgnoreCase)
        || status.Equals("refunded", StringComparison.OrdinalIgnoreCase);
}