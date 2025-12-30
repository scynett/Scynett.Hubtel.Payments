using FluentValidation;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public sealed class ReceiveMoneyCallbackRequestValidator : AbstractValidator<ReceiveMoneyCallbackRequest>
{
    public ReceiveMoneyCallbackRequestValidator()
    {
        RuleFor(x => x.ResponseCode)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Data)
            .NotNull();

        RuleFor(x => x.Data.ClientReference)
            .NotEmpty()
            .MaximumLength(36);

        RuleFor(x => x.Data.TransactionId)
            .NotEmpty();

        RuleFor(x => x.Data.Amount)
            .GreaterThan(0);

        // Optional, but nice: if PaymentDate is present, it should be valid
        RuleFor(x => x.Data.PaymentDate)
            .Must(_ => true);
    }
}