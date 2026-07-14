using FluentValidation;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

/// <summary>
/// Validates an inbound Hubtel Direct Receive Money callback payload.
/// </summary>
/// <remarks>
/// <para>
/// <b>Behaviour change (SDK hardening):</b> <c>Amount</c> was required to be <c>&gt; 0</c>, which rejected
/// legitimate FAILURE callbacks — Hubtel sends <c>Amount: 0</c> when a transaction did not go through.
/// Those callbacks were failing validation and being answered with a 4xx, so the pending transaction was
/// never resolved. The rule is now <c>&gt;= 0</c>. A zero amount is a valid callback; whether the payment
/// succeeded is decided by the response code, not the amount.
/// </para>
/// <para>
/// The no-op rule <c>RuleFor(x =&gt; x.Data!.PaymentDate).Must(_ =&gt; true)</c> was removed; it validated
/// nothing.
/// </para>
/// </remarks>
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

                // Hubtel sends Amount: 0 on failure callbacks - accept them, do not drop them.
                RuleFor(x => x.Data!.Amount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Amount must not be negative.");
            });
    }
}
