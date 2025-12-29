using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.Routing;

internal static class RouteGroups
{
    internal static IEndpointRouteBuilder MapHubtelGroup(
        this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGroup("/hubtel");
    }
}