using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scynett.Hubtel.Payments.Infrastructure.Configuration;

public sealed class PendingTransactionsWorkerOptions
{
    /// <summary>
    /// Polling interval for checking pending transactions. Default: 1 minute.
    /// </summary>
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Max number of pending items to process per run (prevents long loops).
    /// </summary>
    public int BatchSize { get; init; } = 200;

    /// <summary>
    /// How long to wait for the primary callback before polling status.
    /// </summary>
    public TimeSpan CallbackGracePeriod { get; init; } = TimeSpan.FromMinutes(5);
}
