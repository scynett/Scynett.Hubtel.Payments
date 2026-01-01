using System;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using Refit;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Infrastructure.BackgroundWorkers;
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.Gateways;
using Scynett.Hubtel.Payments.Infrastructure.Http;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubtelPayments(this IServiceCollection services, Action<PendingTransactionsWorkerOptions>? configure = null)
    {
        services.TryAddSingleton<IPendingTransactionsStore, InMemoryPendingTransactionsStore>();
        services.AddTransient<HubtelAuthHandler>();

        services.AddScoped<IHubtelReceiveMoneyGateway, HubtelReceiveMoneyGateway>();
        services.AddScoped<IHubtelTransactionStatusGateway, HubtelTransactionStatusGateway>();

        // --- Validators
        services.AddScoped<IValidator<InitiateReceiveMoneyRequest>, InitiateReceiveMoneyRequestValidator>();
        services.AddScoped<IValidator<ReceiveMoneyCallbackRequest>, ReceiveMoneyCallbackRequestValidator>();
        services.AddScoped<IValidator<TransactionStatusQuery>, TransactionStatusQueryValidator>();

        // --- Processors
        services.AddScoped<InitiateReceiveMoneyProcessor>();
        services.AddScoped<ReceiveMoneyCallbackProcessor>();
        services.AddScoped<TransactionStatusProcessor>();

        // --- Public feature
        services.AddScoped<IDirectReceiveMoney, DirectReceiveMoney.DirectReceiveMoney>();

        // --- HTTP clients
        services.AddRefitClient<IHubtelDirectReceiveMoneyApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HubtelOptions>>().Value;
                client.BaseAddress = ResolveBaseAddress(
                    options.ReceiveMoneyBaseAddress,
                    nameof(HubtelOptions.ReceiveMoneyBaseAddress));
                client.Timeout = ResolveTimeout(options.TimeoutSeconds);
            })
            .AddHttpMessageHandler<HubtelAuthHandler>()
            .AddHubtelResilience();

        services.AddRefitClient<IHubtelTransactionStatusApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HubtelOptions>>().Value;
                client.BaseAddress = ResolveBaseAddress(
                    options.TransactionStatusBaseAddress,
                    nameof(HubtelOptions.TransactionStatusBaseAddress));
                client.Timeout = ResolveTimeout(options.TimeoutSeconds);
            })
            .AddHttpMessageHandler<HubtelAuthHandler>()
            .AddHubtelResilience();

        services.AddOptions<PendingTransactionsWorkerOptions>();
        services.AddOptions<HubtelResilienceOptions>();
        if (configure is not null)
            services.Configure(configure);

        services.AddHostedService<PendingTransactionsWorker>();

        return services;
    }

    private static Uri ResolveBaseAddress(string? configured, string optionName)
    {
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException(
                $"Hubtel option '{optionName}' must be a non-empty absolute URI.");
        }

        if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException(
                $"Hubtel option '{optionName}' must be a valid absolute URI.");
        }

        return uri;
    }

    private static TimeSpan ResolveTimeout(int timeoutSeconds)
    {
        var value = timeoutSeconds <= 0 ? 30 : timeoutSeconds;
        return TimeSpan.FromSeconds(value);
    }

    private static IHttpClientBuilder AddHubtelResilience(this IHttpClientBuilder builder)
    {
        builder.Services
            .AddOptions<HttpStandardResilienceOptions>(builder.Name)
            .Configure<IOptions<HubtelResilienceOptions>>((options, cfg) => HubtelHttpPolicies.Apply(options, cfg.Value));

        builder.AddStandardResilienceHandler();
        return builder;
    }
}



