using System.Diagnostics;
using System.Linq;
using System.Net;

using FluentAssertions;

using Microsoft.AspNetCore.Http;

using Scynett.Hubtel.Payments.Infrastructure.Http;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class HubtelCorrelationHandlerTests : UnitTestBase
{
    [Fact]
    public async Task SendAsync_ShouldPropagateHeader_FromHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = "from-http";
        var accessor = new HttpContextAccessor { HttpContext = context };
        using var handler = new HubtelCorrelationHandler(accessor)
        {
            InnerHandler = new RecordingHandler()
        };

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/") };
        await client.GetAsync(new Uri("https://example.com/test"));

        var recording = (RecordingHandler)handler.InnerHandler;
        recording.LastRequest!.Headers.GetValues("X-Correlation-Id").Single().Should().Be("from-http");
    }

    [Fact]
    public async Task SendAsync_ShouldUseActivityTraceId_WhenNoHttpContextHeader()
    {
        using var activity = new Activity("test");
        activity.Start();
        var accessor = new HttpContextAccessor();
        using var handler = new HubtelCorrelationHandler(accessor)
        {
            InnerHandler = new RecordingHandler()
        };

        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/") };
        await client.GetAsync(new Uri("https://example.com/test"));

        var recording = (RecordingHandler)handler.InnerHandler;
        recording.LastRequest!.Headers.GetValues("X-Correlation-Id").Single().Should().Be(activity.TraceId.ToString());
        activity.Stop();
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
