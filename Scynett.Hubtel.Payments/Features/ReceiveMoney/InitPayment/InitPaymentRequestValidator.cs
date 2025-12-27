using FluentValidation;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

/// <summary>
/// Validator for ReceiveMoneyRequest based on Hubtel API specifications.
/// </summary>
public sealed class ReceiveMoneyRequestValidator : AbstractValidator<ReceiveMoneyRequest>
{
    private static readonly string[] ValidChannels = ["mtn-gh", "vodafone-gh", "tigo-gh"];

    public ReceiveMoneyRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .WithMessage("Customer name must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerName));

        RuleFor(x => x.CustomerMobileNumber)
            .NotEmpty()
            .WithMessage("Customer mobile number is required (Mandatory)")
            .Matches(@"^\d{12}$")
            .WithMessage("Mobile number must be 12 digits in international format (e.g., 233241234567)")
            .Must(number => number.StartsWith("233", StringComparison.Ordinal))
            .WithMessage("Mobile number must start with Ghana country code 233");

        RuleFor(x => x.Channel)
            .NotEmpty()
            .WithMessage("Payment channel is required (Mandatory)")
            .Must(channel => ValidChannels.Any(vc => vc.Equals(channel, StringComparison.OrdinalIgnoreCase)))
            .WithMessage($"Channel must be one of: {string.Join(", ", ValidChannels)}");

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
            .NotEmpty()
            .WithMessage("Primary callback URL is required (Mandatory)")
            .Must(BeAValidUrl)
            .WithMessage("Callback endpoint must be a valid HTTP or HTTPS URL")
            .When(x => !string.IsNullOrWhiteSpace(x.PrimaryCallbackEndPoint));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
