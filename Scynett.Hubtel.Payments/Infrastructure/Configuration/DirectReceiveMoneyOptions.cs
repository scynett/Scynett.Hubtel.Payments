namespace Scynett.Hubtel.Payments.Infrastructure.Configuration;

public sealed class DirectReceiveMoneyOptions
{
    /// <summary>
    /// Default callback URL used when the request does not explicitly specify one.
    /// </summary>
    public string DefaultCallbackAddress { get; init; } = string.Empty;

    /// <summary>
    /// Optional override for the POS Sales ID for Direct Receive Money.
    /// If empty, the global HubtelOptions.PosSalesId is used.
    /// </summary>
    public string? PosSalesId { get; init; }
}