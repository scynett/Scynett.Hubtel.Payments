namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

/// <summary>
/// Raw response from Hubtel Transaction Status API.
/// </summary>
internal sealed class TransactionStatusResponseDto
{
    public string? Message { get; init; }        // "Successful"
    public string? ResponseCode { get; init; }   // "0000"
    public TransactionStatusDataDto? Data { get; init; }
}

/// <summary>
/// Data payload returned by Hubtel for a transaction status.
/// </summary>
internal sealed class TransactionStatusDataDto
{
    public DateTimeOffset? Date { get; init; }

    /// <summary>
    /// Paid | Unpaid | Refunded
    /// </summary>
    public string? Status { get; init; }

    public string? TransactionId { get; init; }
    public string? ExternalTransactionId { get; init; }

    public string? PaymentMethod { get; init; }      // "mobilemoney"
    public string? ClientReference { get; init; }

    public string? CurrencyCode { get; init; }

    public decimal? Amount { get; init; }
    public decimal? Charges { get; init; }
    public decimal? AmountAfterCharges { get; init; }

    public bool? IsFulfilled { get; init; }
}
