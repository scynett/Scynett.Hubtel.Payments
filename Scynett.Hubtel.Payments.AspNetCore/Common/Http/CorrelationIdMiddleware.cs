using Microsoft.AspNetCore.Http;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.Http;

internal sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string HeaderName = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var id))
        {
            id = Guid.NewGuid().ToString();
            context.Request.Headers[HeaderName] = id;
        }

        context.Response.Headers[HeaderName] = id!;
        await _next(context).ConfigureAwait(false);
    }
}