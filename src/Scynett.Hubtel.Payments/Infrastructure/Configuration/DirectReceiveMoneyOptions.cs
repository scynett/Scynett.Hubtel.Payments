namespace Scynett.Hubtel.Payments.Options;

public sealed class DirectReceiveMoneyOptions
{
    /// <summary>
    /// Default callback URL used when the request does not explicitly specify one.
    /// </summary>
    public string DefaultCallbackAddress { get; set; } = string.Empty;

    /// <summary>
    /// Optional override for the POS Sales ID for Direct Receive Money.
    /// If empty, the global HubtelOptions.PosSalesId is used.
    /// </summary>
    public string? PosSalesId { get; set; }

    /// <summary>
    /// Optional override of the mobile money channels accepted by
    /// <c>InitiateReceiveMoneyRequestValidator</c>.
    /// <para>
    /// Leave <see langword="null"/> or empty to use the SDK defaults
    /// (<c>mtn-gh</c>, <c>telecel-gh</c>, <c>at-gh</c>, <c>airteltigo</c>, plus the legacy
    /// <c>vodafone-gh</c> and <c>tigo-gh</c>). Set it when Hubtel adds or renames a channel and you
    /// do not want to wait for an SDK release. Comparison is case-insensitive.
    /// </para>
    /// </summary>
    public IReadOnlyList<string>? AllowedChannels { get; set; }
}
