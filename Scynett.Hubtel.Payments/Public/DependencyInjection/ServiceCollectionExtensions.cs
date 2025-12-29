using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.Public.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.Public.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubtelPayments(this IServiceCollection services)
    {
        // --- Validators
        services.AddScoped<IValidator<InitiateReceiveMoneyRequest>, InitiateReceiveMoneyRequestValidator>();
        services.AddScoped<IValidator<ReceiveMoneyCallbackRequest>, ReceiveMoneyCallbackRequestValidator>();

        // --- Processors
        services.AddScoped<InitiateReceiveMoneyProcessor>();
        services.AddScoped<ReceiveMoneyCallbackProcessor>();

        // --- Public feature
        services.AddScoped<IDirectReceiveMoney, DirectReceiveMoney.DirectReceiveMoney>();

        return services;
    }
}
