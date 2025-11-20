using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scynett.Hubtel.Payments.AspNetCore.Workers;
using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;
using Scynett.Hubtel.Payments.Features.Status;
using Scynett.Hubtel.Payments.Storage;

namespace Scynett.Hubtel.Payments.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubtelPayments(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<HubtelOptions>(configuration.GetSection(HubtelOptions.SectionName));

        services.AddHttpClient<IReceiveMoneyService, ReceiveMoneyService>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<HubtelOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        services.AddHttpClient<IHubtelStatusService, HubtelStatusService>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<HubtelOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        services.AddSingleton<IPendingTransactionsStore, InMemoryPendingTransactionsStore>();

        services.AddHostedService<PendingTransactionsWorker>();

        return services;
    }
}
