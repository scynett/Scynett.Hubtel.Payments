using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Models;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/*
0000 → success
0001 → accepted/pending (wait for callback)
2001 → customer/payment rail issues (PIN, insufficient funds, limits, USSD timeout, invalid transaction id, etc.)
4000 → validation problems (your request is wrong)
4070 → fee setup / minimum amount / retry later
4101 → business/auth/scopes/POS issues
4103 → permission/channel not allowed
 */
public sealed class ReceiveMobileMoneyService(
   IOptions<HubtelSettings> options,
    IReceiveMobileMoneyApi api) : IReceiveMobileMoneyService
{
    public async Task<ReceiveMobileMoneyResponse> InitiateReceiveMoney(ReceiveMobileMoneyRequest request, CancellationToken ct)
    {
       return await api.ReceiveMobileMoneyAsync(
            new ReceiveMobileMoneyRequest(
               request.CustomerName,
               request.CustomerMsisdn,
               request.CustomerEmail,
               request.Channel.ToString(),
               request.Amount,
               options.Value.PrimaryCallbackEndPoint,
               request.Description,
               Guid.NewGuid().ToString())).ConfigureAwait(false);

    }
}
