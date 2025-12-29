using FluentValidation;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;


internal sealed class TransactionStatusRequestValidator
    : AbstractValidator<TransactionStatusRequest>
{
    public TransactionStatusRequestValidator()
    {
        RuleFor(x => x.ClientReference)
            .NotEmpty()
            .MaximumLength(36);
    }
}