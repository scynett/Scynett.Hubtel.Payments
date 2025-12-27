using FluentValidation;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

/// <summary>
/// Validator for PaymentCallback.
/// </summary>
public sealed class PaymentCallbackValidator : AbstractValidator<PaymentCallback>
{
    private static readonly string[] ValidStatuses = 
    [
        "SUCCESS", "SUCCESSFUL", "FAILED", "CANCELLED", "PENDING"
    ];

    public PaymentCallbackValidator()
    {
        RuleFor(x => x.ResponseCode)
            .NotEmpty()
            .WithMessage("Response code is required")
            .Matches(@"^\d{4}$")
            .WithMessage("Response code must be a 4-digit number");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(status => ValidStatuses.Contains(status.ToUpperInvariant()))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");

        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("Transaction ID is required")
            .MaximumLength(100)
            .WithMessage("Transaction ID must not exceed 100 characters");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0");

        RuleFor(x => x.Charges)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Charges must be greater than or equal to 0");

        RuleFor(x => x.CustomerMobileNumber)
            .Matches(@"^\d{12}$")
            .WithMessage("Mobile number must be 12 digits in international format (e.g., 233241234567)")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerMobileNumber));

        RuleFor(x => x.ClientReference)
            .MaximumLength(36)
            .WithMessage("Client reference must not exceed 36 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ClientReference));

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.ExternalTransactionId)
            .MaximumLength(100)
            .WithMessage("External transaction ID must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.ExternalTransactionId));
    }
}
