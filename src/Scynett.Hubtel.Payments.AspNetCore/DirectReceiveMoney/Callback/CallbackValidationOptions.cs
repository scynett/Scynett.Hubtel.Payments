using System.Collections.Generic;
using System.Linq;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

/// <summary>
/// Options for the (optional) guard on the Hubtel callback endpoint.
/// </summary>
/// <remarks>
/// Hubtel does not sign its callbacks. These options configure a pre-shared secret header and an IP allow
/// list; neither authenticates the callback <i>body</i>. Always confirm a payment with the Transaction
/// Status API before treating it as money received. See <see cref="CallbackValidator"/>.
/// </remarks>
public sealed class CallbackValidationOptions
{
    private string[] _allowedCidrs = Array.Empty<string>();

    /// <summary>
    /// Enables the shared-secret and IP checks. Off by default.
    /// </summary>
    public bool EnableValidation { get; set; }

    /// <summary>
    /// Pre-shared secret that the caller must echo back in <see cref="SignatureHeaderName"/>.
    /// Compared in constant time. This is a shared secret, not a signature — it does not authenticate
    /// the payload.
    /// </summary>
    public string? SharedSecret { get; set; }

    /// <summary>
    /// Header carrying <see cref="SharedSecret"/>. Named "signature" for historical reasons; Hubtel
    /// provides no callback signature.
    /// </summary>
    public string SignatureHeaderName { get; set; } = "X-Hubtel-Callback-Secret";

    public IReadOnlyList<string> AllowedCidrs
    {
        get => _allowedCidrs;
        set => _allowedCidrs = value is null ? Array.Empty<string>() : value.ToArray();
    }
}
