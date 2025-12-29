namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

public sealed record TransactionStatusQuery(
    string? ClientReference = null,
    string? HubtelTransactionId = null,
    string? NetworkTransactionId = null)
{
    public bool HasAnyIdentifier =>
        !string.IsNullOrWhiteSpace(ClientReference) ||
        !string.IsNullOrWhiteSpace(HubtelTransactionId) ||
        !string.IsNullOrWhiteSpace(NetworkTransactionId);
}