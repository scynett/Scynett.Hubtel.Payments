using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Abstractions;
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
        // Configuration
        services.Configure<HubtelOptions>(configuration.GetSection(HubtelOptions.SectionName));

        // Core payment services - these will internally register Refit clients
        services.AddHubtelPaymentsCore();

        // Status service - Uses HttpClient directly
        services.AddHttpClient<ITransactionStatusProcessor, HubtelStatusService>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HubtelOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        // Storage
        services.AddSingleton<IPendingTransactionsStore, InMemoryPendingTransactionsStore>();

        // Background worker
        services.AddHostedService<PendingTransactionsWorker>();

        return services;
    }
}
