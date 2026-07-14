using System;

using FluentValidation;

using Microsoft.AspNetCore.Http;
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
    /// <summary>
    /// Registers the Hubtel payments SDK.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Behaviour change (SDK hardening):</b> this now validates <see cref="HubtelOptions"/> eagerly
    /// (<c>ValidateOnStart</c>). An app with a missing Hubtel ClientId/ClientSecret used to start happily and
    /// fail with a 401 from Hubtel on the first real payment attempt; it now fails fast at host startup with
    /// "Hubtel ClientId and ClientSecret must be provided".
    /// </para>
    /// </remarks>
    public static IServiceCollection AddHubtelPayments(this IServiceCollection services, Action<PendingTransactionsWorkerOptions>? configure = null)
    {
        // Fail at startup, not at the till: missing credentials are a deployment error.
        AddHubtelOptionsValidation(services);

        services.TryAddSingleton<IPendingTransactionsStore, InMemoryPendingTransactionsStore>();
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddSingleton<ICallbackAuditStore, InMemoryCallbackAuditStore>();
        services.AddTransient<HubtelAuthHandler>();
        services.AddTransient<HubtelCorrelationHandler>();

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
            .AddHttpMessageHandler<HubtelCorrelationHandler>()
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
            .AddHttpMessageHandler<HubtelCorrelationHandler>()
            .AddHttpMessageHandler<HubtelAuthHandler>()
            .AddHubtelResilience();

        services.AddOptions<PendingTransactionsWorkerOptions>();
        services.AddOptions<PendingTransactionsCleanupOptions>();
        services.AddOptions<HubtelResilienceOptions>();
        if (configure is not null)
            services.Configure(configure);

        services.AddHostedService<PendingTransactionsCleanupService>();

        return services;
    }

    public static IServiceCollection AddHubtelPaymentsWorker(
        this IServiceCollection services,
        Action<PendingTransactionsWorkerOptions>? configure = null)
    {
        services.AddOptions<PendingTransactionsWorkerOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddHostedService<PendingTransactionsWorker>();
        return services;
    }

    /// <summary>
    /// Eager validation of the Hubtel credentials. Private (not an extension method) on purpose: the public
    /// entry point lives in ScynettPayments.AspNetCore
    /// (<c>Scynett.Hubtel.Payments.AspNetCore.DependencyInjection.OptionsValidationExtensions</c>) and adding a
    /// second extension method of the same name here would make the two ambiguous for consumers importing both
    /// namespaces.
    /// </summary>
    private static void AddHubtelOptionsValidation(IServiceCollection services)
    {
        services.AddOptions<HubtelOptions>()
            .Validate(o =>
                !string.IsNullOrWhiteSpace(o.ClientId) &&
                !string.IsNullOrWhiteSpace(o.ClientSecret),
                "Hubtel ClientId and ClientSecret must be provided")
            .ValidateOnStart();
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



