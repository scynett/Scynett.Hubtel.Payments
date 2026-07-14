using FluentValidation;

using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Options;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;


/// <summary>
/// Validator for ReceiveMoneyRequest based on Hubtel API specifications.
/// </summary>
/// <remarks>
/// <para>
/// <b>Behaviour change (SDK hardening):</b> the accepted channel list was stale
/// (<c>mtn-gh</c>, <c>vodafone-gh</c>, <c>tigo-gh</c>) and rejected Hubtel's current channels.
/// Vodafone Ghana became Telecel (<c>telecel-gh</c>) and Tigo became AirtelTigo
/// (<c>at-gh</c> / <c>airteltigo</c>). Those are now accepted. The legacy values remain accepted, so
/// nothing that used to validate stops validating. Override the list with
/// <see cref="DirectReceiveMoneyOptions.AllowedChannels"/> if Hubtel changes it again.
/// </para>
/// </remarks>
public class InitiateReceiveMoneyRequestValidator : AbstractValidator<InitiateReceiveMoneyRequest>
{
    /// <summary>
    /// Channels accepted out of the box: Hubtel's current channels plus the superseded ones,
    /// which are kept for backward compatibility.
    /// </summary>
    public static readonly IReadOnlyList<string> DefaultValidChannels =
    [
        // Current Hubtel channels
        "mtn-gh",
        "telecel-gh",   // replaced vodafone-gh
        "at-gh",        // replaced tigo-gh
        "airteltigo",   // alias used by some Hubtel accounts
        // Legacy channels - still accepted so existing callers never start failing
        "vodafone-gh",
        "tigo-gh",
    ];

    /// <summary>
    /// Creates a validator using <see cref="DefaultValidChannels"/>.
    /// </summary>
    public InitiateReceiveMoneyRequestValidator()
        : this(channels: null)
    {
    }

    /// <summary>
    /// Creates a validator honouring <see cref="DirectReceiveMoneyOptions.AllowedChannels"/> when set.
    /// This overload is the one the DI container resolves.
    /// </summary>
    public InitiateReceiveMoneyRequestValidator(IOptions<DirectReceiveMoneyOptions> options)
        : this(options?.Value?.AllowedChannels)
    {
    }

    private InitiateReceiveMoneyRequestValidator(IReadOnlyList<string>? channels)
    {
        IReadOnlyList<string> validChannels = channels is { Count: > 0 }
            ? channels
            : DefaultValidChannels;

        RuleFor(x => x.CustomerName)
        .MaximumLength(100)
        .WithMessage("Customer name must not exceed 100 characters")
        .When(x => !string.IsNullOrWhiteSpace(x.CustomerName));

        RuleFor(x => x.CustomerEmail)
            .MaximumLength(256)
            .WithMessage("Customer email must not exceed 256 characters")
            .EmailAddress()
            .WithMessage("Customer email must be a valid email address")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            .WithMessage("Customer email must be a valid email address")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail));

        RuleFor(x => x.CustomerMobileNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Customer mobile number is required (Mandatory)")
            .Matches(@"^[0-9]{12}$")
            .WithMessage("Mobile number must be 12 digits in international format (e.g., 233241234567)")
            .Must(number => number.StartsWith("233", StringComparison.Ordinal))
            .WithMessage("Mobile number must start with Ghana country code 233");

        RuleFor(x => x.Channel)
            .NotEmpty()
            .WithMessage("Payment channel is required (Mandatory)")
            .Must(channel => validChannels.Any(vc => vc.Equals(channel, StringComparison.OrdinalIgnoreCase)))
            .WithMessage($"Channel must be one of: {string.Join(", ", validChannels)}");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0 (Mandatory)")
            .PrecisionScale(10, 2, ignoreTrailingZeros: true)
            .WithMessage("Amount must have at most 2 decimal places (e.g., 0.50)");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required (Mandatory)")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.ClientReference)
            .NotEmpty()
            .WithMessage("Client reference is required (Mandatory) and must be unique for every transaction")
            .MaximumLength(36)
            .WithMessage("Client reference must not exceed 36 characters")
            .Matches(@"^[a-zA-Z0-9]+$")
            .WithMessage("Client reference should preferably be alphanumeric characters");

        RuleFor(x => x.PrimaryCallbackEndPoint)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Primary callback URL is required (Mandatory)")
            .Must(BeAValidUrl)
            .WithMessage("Callback endpoint must be a valid HTTP or HTTPS URL");
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
