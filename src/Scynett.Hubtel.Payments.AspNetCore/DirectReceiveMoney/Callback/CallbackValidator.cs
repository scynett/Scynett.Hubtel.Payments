using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

public sealed class CallbackValidator : ICallbackValidator
{
    private readonly CallbackValidationOptions _options;
    private readonly ILogger<CallbackValidator> _logger;

    public CallbackValidator(
        IOptions<CallbackValidationOptions> options,
        ILogger<CallbackValidator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<CallbackValidationResult> ValidateAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        if (!_options.EnableValidation)
        {
            return Task.FromResult(CallbackValidationResult.Success);
        }

        if (!string.IsNullOrWhiteSpace(_options.SharedSecret))
        {
            if (!context.Request.Headers.TryGetValue(_options.SignatureHeaderName, out var headerValue) ||
                !string.Equals(headerValue.ToString(), _options.SharedSecret, StringComparison.Ordinal))
            {
                _logger.LogWarning("Callback validation failed due to invalid shared secret.");
                return Task.FromResult(CallbackValidationResult.Failure(
                    "Hubtel.Callback.InvalidSignature",
                    "Callback signature validation failed."));
            }
        }

        if (_options.AllowedCidrs.Count > 0)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp is null || !IsIpAllowed(remoteIp))
            {
                _logger.LogWarning("Callback validation failed due to disallowed IP {RemoteIp}", remoteIp);
                return Task.FromResult(CallbackValidationResult.Failure(
                    "Hubtel.Callback.InvalidSource",
                    "Callback source IP is not allowed."));
            }
        }

        return Task.FromResult(CallbackValidationResult.Success);
    }

    private bool IsIpAllowed(IPAddress remoteIp)
    {
        foreach (var cidr in _options.AllowedCidrs)
        {
            if (string.IsNullOrWhiteSpace(cidr))
                continue;

            if (TryParseCidr(cidr.Trim(), out var network, out var maskBytes))
            {
                var addressBytes = remoteIp.GetAddressBytes();
                if (addressBytes.Length == maskBytes.Length)
                {
                    var matches = true;
                    for (var i = 0; i < maskBytes.Length; i++)
                    {
                        if ((addressBytes[i] & maskBytes[i]) != (network[i] & maskBytes[i]))
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (matches)
                        return true;
                }
            }
            else if (IPAddress.TryParse(cidr, out var allowedIp) &&
                     allowedIp.Equals(remoteIp))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryParseCidr(string cidr, out byte[] networkBytes, out byte[] maskBytes)
    {
        networkBytes = Array.Empty<byte>();
        maskBytes = Array.Empty<byte>();

        var parts = cidr.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return false;

        if (!IPAddress.TryParse(parts[0], out var network) ||
            !int.TryParse(parts[1], out var prefixLength))
            return false;

        networkBytes = network.GetAddressBytes();
        maskBytes = new byte[networkBytes.Length];

        if (prefixLength < 0 || prefixLength > networkBytes.Length * 8)
            return false;

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        for (var i = 0; i < fullBytes; i++)
        {
            maskBytes[i] = 0xFF;
        }

        if (remainingBits > 0 && fullBytes < maskBytes.Length)
        {
            maskBytes[fullBytes] = (byte)(0xFF << (8 - remainingBits));
        }

        return true;
    }
}
