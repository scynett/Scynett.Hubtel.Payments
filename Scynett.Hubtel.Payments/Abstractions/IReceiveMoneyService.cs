using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.InitPayment;

namespace Scynett.Hubtel.Payments.Abstractions;

public interface IReceiveMoneyService
{
    Task<Result<InitPaymentResponse>> InitAsync(InitPaymentRequest command, CancellationToken cancellationToken = default);
    Task<Result> ProcessCallbackAsync(PaymentCallback command, CancellationToken cancellationToken = default);
}
