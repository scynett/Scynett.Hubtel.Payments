using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.DependencyInjection;
using Scynett.Hubtel.Payments.Tests.Stubs;

using WireMock.Server;
using WireMock.Settings;

namespace Scynett.Hubtel.Payments.Tests.Fixtures;

[SuppressMessage("Design", "CA1515", Justification = "xUnit fixtures must be public.")]
public sealed class IntegrationFixture : IDisposable
{
    public const string DefaultPosSalesId = "POS-123";

    public IServiceProvider Services { get; }
    public WireMockServer HubtelMock { get; }

    public IntegrationFixture()
    {
        HubtelMock = WireMockServer.Start(new WireMockServerSettings
        {
            StartAdminInterface = false,
            ReadStaticMappings = false
        });

        RegisterDefaultStubs();

        var services = new ServiceCollection();

        services.AddOptions<HubtelOptions>().Configure(o =>
        {
            o.ClientId = "test-client";
            o.ClientSecret = "test-secret";
            o.MerchantAccountNumber = DefaultPosSalesId;
            o.ReceiveMoneyBaseAddress = HubtelMock.Url!;
            o.TransactionStatusBaseAddress = HubtelMock.Url!;
            o.TimeoutSeconds = 5;
        });

        services.AddOptions<DirectReceiveMoneyOptions>().Configure(o =>
        {
            o.PosSalesId = DefaultPosSalesId;
        });

        services.AddHubtelPayments(options =>
        {
            options.BatchSize = 50;
            options.CallbackGracePeriod = TimeSpan.Zero;
            options.PollInterval = TimeSpan.FromMilliseconds(50);
        });
        services.AddHubtelPaymentsWorker();

        Services = services.BuildServiceProvider(validateScopes: true);
    }

    public Uri DefaultCallbackUrl => CallbackStub.GetCallbackUri(HubtelMock);

    /// <summary>
    /// Clears and re-registers the standard WireMock mappings. Allows tests to append
    /// custom stubs via the optional configuration callback.
    /// </summary>
    public void ResetHubtel(Action<WireMockServer>? configure = null)
    {
        HubtelMock.Reset();
        RegisterDefaultStubs();
        configure?.Invoke(HubtelMock);
    }

    private void RegisterDefaultStubs()
    {
        InitiateReceiveMoneyStub.Register(HubtelMock, DefaultPosSalesId);
        TransactionStatusStub.Register(HubtelMock, DefaultPosSalesId);
        CallbackStub.Register(HubtelMock);
    }

    public void Dispose()
    {
        (Services as IDisposable)?.Dispose();
        HubtelMock.Dispose();
    }
}

[CollectionDefinition("IntegrationTests")]
[SuppressMessage("Design", "CA1515", Justification = "xUnit collection definitions must be public.")]
public sealed class IntegrationTestsDefinition : ICollectionFixture<IntegrationFixture>
{
}


