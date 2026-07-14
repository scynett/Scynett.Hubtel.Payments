using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.Http;

internal sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Guarded + hoisted: PathString.ToString() allocates, and must not run when the level is
        // disabled (CA1873).
        if (_logger.IsEnabled(LogLevel.Information))
        {
            var method = context.Request.Method;
            var path = context.Request.Path.ToString();

            RequestLoggingLogMessages.HttpRequest(_logger, method, path);
        }

        await _next(context).ConfigureAwait(false);
    }
}

internal static partial class RequestLoggingLogMessages
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path}")]
    public static partial void HttpRequest(
        ILogger logger,
        string method,
        string path);
}