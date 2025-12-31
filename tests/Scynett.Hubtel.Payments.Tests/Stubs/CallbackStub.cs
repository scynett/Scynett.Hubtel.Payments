using System;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Scynett.Hubtel.Payments.Tests.Stubs;

internal static class CallbackStub
{
    private const string CallbackPath = "/callbacks/direct-receive-money";

    public static void Register(WireMockServer server)
    {
        server.Given(Request.Create()
                .WithPath(CallbackPath)
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("ok"));
    }

    public static Uri GetCallbackUri(WireMockServer server) 
        => new($"{server.Url}{CallbackPath}", UriKind.Absolute);
}
