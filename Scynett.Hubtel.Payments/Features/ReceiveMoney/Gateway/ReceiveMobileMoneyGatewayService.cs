using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Models;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/// <summary>
/// Gateway for interacting with Hubtel Receive Money API.
/// Response Codes:
/// 0000 → success
/// 0001 → accepted/pending (wait for callback)
/// 2001 → customer/payment rail issues (PIN, insufficient funds, limits, USSD timeout, invalid transaction id, etc.)
/// 4000 → validation problems (your request is wrong)
/// 4070 → fee setup / minimum amount / retry later
/// 4101 → business/auth/scopes/POS issues
/// 4103 → permission/channel not allowed
/// </summary>
public sealed class HubtelReceiveMoneyGateway(
   IOptions<HubtelOptions> options,
    IHubtelReceiveMoneyClient client) : IReceiveMobileMoneyService
{
    /// <summary>
    /// Initiates a receive money transaction.
    /// </summary>
    public async Task<HubtelReceiveMoneyResponse> InitiateReceiveMoney(
        HubtelReceiveMoneyRequest request,
        CancellationToken cancellationToken = default)
    {
       return await client.ReceiveMobileMoneyAsync(
            new HubtelReceiveMoneyRequest(
               request.CustomerName,
               request.CustomerMsisdn,
               request.CustomerEmail,
               request.Channel.ToString(),
               request.Amount,
               options.Value.PrimaryCallbackEndPoint,
               request.Description,
               Guid.NewGuid().ToString()), 
            cancellationToken).ConfigureAwait(false);
    }
}
