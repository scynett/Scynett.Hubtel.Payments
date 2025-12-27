namespace Scynett.Hubtel.Payments.Models;

/// <summary>
/// Represents the status of a Hubtel transaction.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// Transaction is pending confirmation.
    /// </summary>
    Pending,

    /// <summary>
    /// Transaction failed.
    /// </summary>
    Failed
}
