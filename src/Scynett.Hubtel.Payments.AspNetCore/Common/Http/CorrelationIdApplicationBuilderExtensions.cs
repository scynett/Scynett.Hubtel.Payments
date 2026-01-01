using Microsoft.AspNetCore.Builder;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.Http;

public static class CorrelationIdApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHubtelCorrelation(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
