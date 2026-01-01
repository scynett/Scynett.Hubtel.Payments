using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.Options;

namespace Scynett.Hubtel.Payments.Infrastructure.BackgroundWorkers;

internal sealed class PendingTransactionsCleanupService(
    IPendingTransactionsStore store,
    IOptions<PendingTransactionsCleanupOptions> options,
    ILogger<PendingTransactionsCleanupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cleanupOptions = options.Value;
        if (!cleanupOptions.Enabled)
        {
            return;
        }

        PendingTransactionsCleanupLogMessages.Started(logger, cleanupOptions.RetentionPeriod, cleanupOptions.CleanupInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCleanupAsync(cleanupOptions, stoppingToken).ConfigureAwait(false);

            try
            {
                await Task.Delay(cleanupOptions.CleanupInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cleanup must continue even if a cycle fails.")]
    private async Task RunCleanupAsync(PendingTransactionsCleanupOptions cleanupOptions, CancellationToken stoppingToken)
    {
        try
        {
            var cutoff = DateTimeOffset.UtcNow - cleanupOptions.RetentionPeriod;
            await store.RemoveOlderThanAsync(cutoff, stoppingToken).ConfigureAwait(false);
            PendingTransactionsCleanupLogMessages.Completed(logger, cutoff);
        }
        catch (Exception ex)
        {
            PendingTransactionsCleanupLogMessages.Failed(logger, ex);
        }
    }
}
