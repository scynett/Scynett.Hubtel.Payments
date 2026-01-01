namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public sealed record PendingTransaction(
    string ClientReference,
    string HubtelTransactionId,
    DateTimeOffset CreatedAtUtc);
