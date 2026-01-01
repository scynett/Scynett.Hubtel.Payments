using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.BackgroundWorkers;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;
using Scynett.Hubtel.Payments.DependencyInjection;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class HubtelServiceRegistrationTests : UnitTestBase
{
    [Fact]
    public void ServiceRegistration_ShouldConfigureRefitClients_WithBaseUrlFromHubtelOptions()
    {
        const string receiveUrl = "https://api.example.com/receive";
        const string statusUrl = "https://api.example.com/status";

        using var provider = BuildServiceProvider(options =>
        {
            options.ReceiveMoneyBaseAddress = receiveUrl;
            options.TransactionStatusBaseAddress = statusUrl;
        });

        var receiveClient = GetHttpClient(provider.GetRequiredService<IHubtelDirectReceiveMoneyApi>());
        receiveClient.BaseAddress.Should().Be(new Uri(receiveUrl));

        var statusClient = GetHttpClient(provider.GetRequiredService<IHubtelTransactionStatusApi>());
        statusClient.BaseAddress.Should().Be(new Uri(statusUrl));
    }

    [Fact]
    public void ServiceRegistration_ShouldThrow_WhenBaseUrlIsMissing()
    {
        using var provider = BuildServiceProvider(options =>
        {
            options.ReceiveMoneyBaseAddress = string.Empty;
        });

        var act = () => provider.GetRequiredService<IHubtelDirectReceiveMoneyApi>();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*ReceiveMoneyBaseAddress*");
    }

    [Fact]
    public void ServiceRegistration_ShouldThrow_WhenBaseUrlIsInvalid()
    {
        using var provider = BuildServiceProvider(options =>
        {
            options.ReceiveMoneyBaseAddress = "not-a-url";
        });

        var act = () => provider.GetRequiredService<IHubtelDirectReceiveMoneyApi>();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*ReceiveMoneyBaseAddress*");
    }

    [Fact]
    public void ServiceRegistration_ShouldConfigureAllHubtelClients_WithConsistentBaseUrl()
    {
        const string baseUrl = "https://shared.example.com";

        using var provider = BuildServiceProvider(options =>
        {
            options.ReceiveMoneyBaseAddress = baseUrl;
            options.TransactionStatusBaseAddress = baseUrl;
        });

        var receiveClient = GetHttpClient(provider.GetRequiredService<IHubtelDirectReceiveMoneyApi>());
        var statusClient = GetHttpClient(provider.GetRequiredService<IHubtelTransactionStatusApi>());

        receiveClient.BaseAddress.Should().Be(new Uri(baseUrl));
        statusClient.BaseAddress.Should().Be(new Uri(baseUrl));
    }

    [Fact]
    public void ServiceRegistration_ShouldNotRegisterWorker_ByDefault()
    {
        using var provider = BuildServiceProvider(null);

        provider.GetServices<IHostedService>()
            .Should().NotContain(s => s is PendingTransactionsWorker);
    }

    [Fact]
    public void ServiceRegistration_ShouldRegisterWorker_WhenOptedIn()
    {
        using var provider = BuildServiceProvider(null, registerWorker: true);

        provider.GetServices<IHostedService>()
            .Should().ContainSingle(s => s is PendingTransactionsWorker);
    }

    private static ServiceProvider BuildServiceProvider(Action<HubtelOptions>? configure, bool registerWorker = false)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.Configure<HubtelOptions>(options =>
        {
            options.ClientId = "client-id";
            options.ClientSecret = "client-secret";
            configure?.Invoke(options);
        });

        services.AddHubtelPayments();
        if (registerWorker)
        {
            services.AddHubtelPaymentsWorker();
        }

        return services.BuildServiceProvider();
    }

    private static HttpClient GetHttpClient(object refitClient)
    {
        var httpClientField = refitClient
            .GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => typeof(HttpClient).IsAssignableFrom(f.FieldType));

        httpClientField.Should().NotBeNull("the Refit client should store its HttpClient instance");

        return (HttpClient)httpClientField!.GetValue(refitClient)!;
    }
}


