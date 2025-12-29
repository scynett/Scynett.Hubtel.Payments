using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scynett.Hubtel.Payments.Infrastructure.Configuration;

public sealed class PendingTransactionsWorkerOptions
{
    /// <summary>
    /// Polling interval for checking pending transactions. Default: 5 minutes.
    /// </summary>
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Max number of pending items to process per run (prevents long loops).
    /// </summary>
    public int BatchSize { get; init; } = 200;
}