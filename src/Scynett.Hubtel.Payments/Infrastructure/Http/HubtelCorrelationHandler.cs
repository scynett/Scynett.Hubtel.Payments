using System.Diagnostics;

using Microsoft.AspNetCore.Http;

namespace Scynett.Hubtel.Payments.Infrastructure.Http;

internal sealed class HubtelCorrelationHandler : DelegatingHandler
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HubtelCorrelationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = ResolveCorrelationId();
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.Remove(HeaderName);
            request.Headers.TryAddWithoutValidation(HeaderName, correlationId);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private string? ResolveCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue(HeaderName, out var headerValue) == true &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        var activityTrace = Activity.Current?.TraceId;
        if (activityTrace is { } trace && trace != default)
        {
            return trace.ToString();
        }

        return httpContext?.TraceIdentifier;
    }
}
