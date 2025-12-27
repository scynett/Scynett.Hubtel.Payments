using Refit;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

public interface IReceiveMobileMoneyApi
{
    [Post("/receive/mobilemoney")]
    Task<ReceiveMobileMoneyGatewayResponse> ReceiveMobileMoneyAsync(
    [Body] ReceiveMobileMoneyGatewayRequest request,
     CancellationToken cancellationToken = default);
}
