using FluentAssertions;

using Microsoft.AspNetCore.Http;

using Scynett.Hubtel.Payments.AspNetCore.Common.Http;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class CorrelationIdMiddlewareTests : UnitTestBase
{
    [Fact]
    public async Task Middleware_ShouldGenerateCorrelationId_WhenMissing()
    {
        var context = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers.TryGetValue("X-Correlation-Id", out var responseHeader).Should().BeTrue();
        responseHeader.ToString().Should().NotBeNullOrWhiteSpace();
        context.Request.Headers["X-Correlation-Id"].ToString().Should().Be(responseHeader.ToString());
    }

    [Fact]
    public async Task Middleware_ShouldPreserveIncomingCorrelationId_WhenPresent()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "incoming-id";
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Correlation-Id"].ToString().Should().Be("incoming-id");
    }
}
