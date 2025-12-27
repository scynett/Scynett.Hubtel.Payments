using FluentValidation;

namespace Scynett.Hubtel.Payments.Features.TransactionStatus;

/// <summary>
/// Validator for TransactionStatusRequest based on Hubtel Status API specifications.
/// </summary>
public sealed class TransactionStatusRequestValidator : AbstractValidator<TransactionStatusRequest>
{
    public TransactionStatusRequestValidator()
    {
        // At least one identifier must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.ClientReference) ||
                      !string.IsNullOrWhiteSpace(x.HubtelTransactionId) ||
                      !string.IsNullOrWhiteSpace(x.NetworkTransactionId))
            .WithMessage("At least one identifier must be provided: ClientReference (preferred), HubtelTransactionId, or NetworkTransactionId");

        // Validate ClientReference when provided
        RuleFor(x => x.ClientReference)
            .MaximumLength(36)
            .WithMessage("Client reference must not exceed 36 characters")
            .Matches(@"^[a-zA-Z0-9\-_]+$")
            .WithMessage("Client reference should contain only alphanumeric characters, hyphens, and underscores")
            .When(x => !string.IsNullOrWhiteSpace(x.ClientReference));

        // Validate HubtelTransactionId when provided
        RuleFor(x => x.HubtelTransactionId)
            .MaximumLength(100)
            .WithMessage("Hubtel transaction ID must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.HubtelTransactionId));

        // Validate NetworkTransactionId when provided
        RuleFor(x => x.NetworkTransactionId)
            .MaximumLength(100)
            .WithMessage("Network transaction ID must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.NetworkTransactionId));
    }
}
