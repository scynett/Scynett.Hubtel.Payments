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
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(x => x.Data!.ClientReference)
                    .NotEmpty()
                    .MaximumLength(36);

                RuleFor(x => x.Data!.TransactionId)
                    .NotEmpty();

                RuleFor(x => x.Data!.Amount)
                    .GreaterThan(0);

                RuleFor(x => x.Data!.PaymentDate)
                    .Must(_ => true);
            });
    }
}
