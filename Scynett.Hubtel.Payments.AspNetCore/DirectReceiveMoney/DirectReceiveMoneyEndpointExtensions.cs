using Microsoft.AspNetCore.Routing;

using Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney;

public static class DirectReceiveMoneyEndpointExtensions
{
    public static IEndpointRouteBuilder MapHubtelDirectReceiveMoney(
        this IEndpointRouteBuilder endpoints)
    {
        HubtelReceiveMoneyCallbackEndpoint.Map(endpoints);
        return endpoints;
    }
}