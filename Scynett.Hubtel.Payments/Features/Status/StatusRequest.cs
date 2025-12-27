namespace Scynett.Hubtel.Payments.Features.TransactionStatus;

/// <summary>
/// Request to check the status of a transaction.
/// You must provide at least one of: ClientReference (preferred), HubtelTransactionId, or NetworkTransactionId.
/// </summary>
public sealed record TransactionStatusRequest
{
    /// <summary>
    /// The client reference of the transaction (preferred - mandatory if others not provided).
    /// </summary>
    public string? ClientReference { get; init; }

    /// <summary>
    /// Transaction ID from Hubtel after successful payment (optional).
    /// </summary>
    public string? HubtelTransactionId { get; init; }

    /// <summary>
    /// The transaction reference from the mobile money provider (optional).
    /// </summary>
    public string? NetworkTransactionId { get; init; }

    /// <summary>
    /// Creates a status request using client reference (preferred).
    /// </summary>
    public static TransactionStatusRequest ByClientReference(string clientReference) =>
        new() { ClientReference = clientReference };

    /// <summary>
    /// Creates a status request using Hubtel transaction ID.
    /// </summary>
    public static TransactionStatusRequest ByHubtelTransactionId(string hubtelTransactionId) =>
        new() { HubtelTransactionId = hubtelTransactionId };

    /// <summary>
    /// Creates a status request using network transaction ID.
    /// </summary>
    public static TransactionStatusRequest ByNetworkTransactionId(string networkTransactionId) =>
        new() { NetworkTransactionId = networkTransactionId };
}
