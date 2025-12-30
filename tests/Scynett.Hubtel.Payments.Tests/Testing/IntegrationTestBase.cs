using Microsoft.Extensions.DependencyInjection;

using Scynett.Hubtel.Payments.Infrastructure.Configuration;
using Scynett.Hubtel.Payments.Public.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Text;

namespace Scynett.Hubtel.Payments.Tests.Testing;


[Trait("Category", "Integration")]
internal class IntegrationTestBase : IDisposable
{
    protected IServiceProvider Services { get; }

    protected IntegrationTestBase(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();


        // ✅ Options required by Refit setup
        services.AddOptions<HubtelOptions>().Configure(o =>
        {
            o.ReceiveMoneyBaseAddress = "https://rmp.hubtel.com";
            o.MerchantAccountNumber = "11684";
        });

        services.AddOptions<DirectReceiveMoneyOptions>().Configure(o =>
        {
            o.PosSalesId = "POS-123";
        });

        // ✅ Your production registration
        services.AddHubtelPayments(options =>
        {
            options.PollInterval = TimeSpan.FromSeconds(30);          
            options.CallbackGracePeriod = TimeSpan.FromMinutes(5);
            options.BatchSize = 200;

        });

        configure?.Invoke(services);


        Services = services.BuildServiceProvider(validateScopes: true);
    }

    protected T GetRequiredService<T>() where T : notnull
         => Services.GetRequiredService<T>();

    public virtual void Dispose()
    {
        if (Services is IDisposable d) d.Dispose();
    }
}
