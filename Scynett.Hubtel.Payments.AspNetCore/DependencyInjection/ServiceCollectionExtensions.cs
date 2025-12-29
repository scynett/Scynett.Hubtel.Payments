using Microsoft.Extensions.DependencyInjection;

using Scynett.Hubtel.Payments.Public.DependencyInjection;

namespace Scynett.Hubtel.Payments.AspNetCore.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubtelPaymentsAspNetCore(
        this IServiceCollection services)
    {
        // Register core SDK (Application + Infrastructure)
        services.AddHubtelPayments();

        return services;
    }
}