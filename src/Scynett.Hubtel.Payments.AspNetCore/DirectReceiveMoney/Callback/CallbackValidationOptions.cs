using System.Collections.Generic;
using System.Linq;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

public sealed class CallbackValidationOptions
{
    private string[] _allowedCidrs = Array.Empty<string>();

    public bool EnableValidation { get; set; }

    public string? SharedSecret { get; set; }

    public string SignatureHeaderName { get; set; } = "X-Hubtel-Callback-Secret";

    public IReadOnlyList<string> AllowedCidrs
    {
        get => _allowedCidrs;
        set => _allowedCidrs = value is null ? Array.Empty<string>() : value.ToArray();
    }
}
