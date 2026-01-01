using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

namespace Scynett.Hubtel.Payments.AspNetCore.DependencyInjection;

public static class CallbackValidationServiceCollectionExtensions
{
    public static IServiceCollection AddHubtelCallbackValidation(
        this IServiceCollection services,
        Action<CallbackValidationOptions>? configure = null)
    {
        services.AddOptions<CallbackValidationOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<ICallbackValidator, CallbackValidator>();
        return services;
    }
}
