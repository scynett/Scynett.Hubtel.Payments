using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Refit;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

using System.Net.Http.Headers;
using System.Text;

namespace Scynett.Hubtel.Payments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubtelPaymentsCore(this IServiceCollection services)
    {
        // Gateway layer - Refit API client for ReceiveMoney
        services.AddRefitClient<IReceiveMobileMoneyApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HubtelSettings>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                
                var authValue = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{options.ClientId}:{options.ClientSecret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            });

        // Public API layer - Orchestration service
        services.AddScoped<IReceiveMoneyService, ReceiveMoneyService>();

        return services;
    }
}
