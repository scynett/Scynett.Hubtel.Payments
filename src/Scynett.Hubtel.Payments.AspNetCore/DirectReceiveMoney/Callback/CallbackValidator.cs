using System.Net;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

/// <summary>
/// Guards the Hubtel callback endpoint with an optional pre-shared secret header and an optional
/// source-IP allow list.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is NOT signature verification.</b> Hubtel does <b>not</b> sign its callbacks: there is no HMAC,
/// no digest of the body, and nothing tying the payload to your account. The "signature" naming used by
/// <see cref="CallbackValidationOptions.SignatureHeaderName"/> and by the
/// <c>Hubtel.Callback.InvalidSignature</c> error code is historical and misleading. All this class can do
/// is check that the caller echoes back a shared secret you configured (and, optionally, that it came from
/// an expected IP range) — it proves nothing about the <i>content</i> of the callback, which is not
/// authenticated and could have been tampered with in transit by anyone able to reach your endpoint.
/// </para>
/// <para>
/// <b>Therefore: a callback is a hint, not proof of payment.</b> Callers MUST independently verify the
/// transaction against Hubtel's Transaction Status API before crediting a wallet, releasing goods, or
/// otherwise treating money as received. Never settle on the strength of a callback body alone.
/// </para>
/// <para>
/// <b>Behaviour change (SDK hardening):</b> the shared-secret comparison now uses
/// <see cref="CryptographicOperations.FixedTimeEquals(ReadOnlySpan{byte}, ReadOnlySpan{byte})"/> over the
/// UTF-8 bytes instead of <see cref="string.Equals(string, string, StringComparison)"/>, which short-circuits
/// on the first differing character and therefore leaks the secret to a timing attack. Correct secrets are
/// still accepted and wrong secrets still rejected — only the timing profile changes.
/// </para>
/// </remarks>
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

    /// <summary>
    /// Checks the shared secret header and source IP of an inbound Hubtel callback.
    /// A successful result means "this request looks like it came from your Hubtel integration",
    /// NOT "this payment happened" — verify with the Transaction Status API before treating it as money.
    /// </summary>
    public Task<CallbackValidationResult> ValidateAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!_options.EnableValidation)
        {
            return Task.FromResult(CallbackValidationResult.Success);
        }

        if (!string.IsNullOrWhiteSpace(_options.SharedSecret))
        {
            if (!context.Request.Headers.TryGetValue(_options.SignatureHeaderName, out var headerValue) ||
                !SecretsMatch(headerValue.ToString(), _options.SharedSecret))
            {
                _logger.LogWarning("Callback validation failed due to invalid shared secret.");
                return Task.FromResult(CallbackValidationResult.Failure(
                    "Hubtel.Callback.InvalidSignature",
                    "Callback shared-secret validation failed."));
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

    /// <summary>
    /// Constant-time comparison of the presented secret against the configured one.
    /// Lengths are compared in the clear (an attacker learns only the secret's length, not its bytes).
    /// </summary>
    private static bool SecretsMatch(string? presented, string expected)
    {
        var presentedBytes = Encoding.UTF8.GetBytes(presented ?? string.Empty);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return CryptographicOperations.FixedTimeEquals(presentedBytes, expectedBytes);
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
