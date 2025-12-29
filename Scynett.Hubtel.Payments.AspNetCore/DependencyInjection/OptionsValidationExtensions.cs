using Microsoft.Extensions.DependencyInjection;

using Scynett.Hubtel.Payments.Infrastructure.Configuration;

namespace Scynett.Hubtel.Payments.AspNetCore.DependencyInjection;

internal static class OptionsValidationExtensions
{
    internal static IServiceCollection AddHubtelOptionsValidation(
        this IServiceCollection services)
    {
        services.AddOptions<HubtelOptions>()
            .Validate(o =>
                !string.IsNullOrWhiteSpace(o.ClientId) &&
                !string.IsNullOrWhiteSpace(o.ClientSecret),
                "Hubtel ClientId and ClientSecret must be provided");

        return services;
    }
}