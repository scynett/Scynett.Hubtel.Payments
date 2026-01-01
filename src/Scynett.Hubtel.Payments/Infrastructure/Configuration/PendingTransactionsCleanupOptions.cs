namespace Scynett.Hubtel.Payments.Options;

public sealed class PendingTransactionsCleanupOptions
{
    public bool Enabled { get; set; } = true;

    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(30);

    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(6);
}
