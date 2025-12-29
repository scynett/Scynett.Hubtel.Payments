using FluentValidation;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

internal sealed class TransactionStatusQueryValidator : AbstractValidator<TransactionStatusQuery>
{
    public TransactionStatusQueryValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.ClientReference) ||
                !string.IsNullOrWhiteSpace(x.HubtelTransactionId) ||
                !string.IsNullOrWhiteSpace(x.NetworkTransactionId))
            .WithMessage("At least one identifier is required: clientReference, hubtelTransactionId, or networkTransactionId.");

        RuleFor(x => x.ClientReference)
            .MaximumLength(36)
            .When(x => !string.IsNullOrWhiteSpace(x.ClientReference));

        // Optional: keep these sane (Hubtel IDs are usually 32 hex, but don’t over-restrict)
        RuleFor(x => x.HubtelTransactionId)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.HubtelTransactionId));

        RuleFor(x => x.NetworkTransactionId)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.NetworkTransactionId));
    }
}