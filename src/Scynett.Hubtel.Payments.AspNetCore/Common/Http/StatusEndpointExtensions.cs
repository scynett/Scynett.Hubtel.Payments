using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.Http;

public static class StatusEndpointExtensions
{
    public static IEndpointRouteBuilder MapStatusEndpoint(this IEndpointRouteBuilder endpoints, string pattern = "/status")
    {
        endpoints.MapGet(pattern, async context =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"status\":\"ok\"}").ConfigureAwait(false);
        })
        .WithName("Status")
        .WithTags("Status");
        return endpoints;
    }
}
