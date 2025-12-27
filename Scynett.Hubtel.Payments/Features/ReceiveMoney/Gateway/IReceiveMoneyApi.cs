using Refit;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

public interface IReceiveMobileMoneyApi
{
    Task<ReceiveMobileMoneyResponse> ReceiveMobileMoneyAsync(
    [Body] ReceiveMobileMoneyRequest request);
}
