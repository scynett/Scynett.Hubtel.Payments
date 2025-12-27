using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using Polly;

using Refit;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;
using Scynett.Hubtel.Payments.Features.Status;

using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Scynett.Hubtel.Payments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubtelPaymentsCore(this IServiceCollection services)
    {
        // Validators
        services.AddScoped<IValidator<ReceiveMoneyRequest>, ReceiveMoneyRequestValidator>();
        services.AddScoped<IValidator<PaymentCallback>, PaymentCallbackValidator>();
        services.AddScoped<IValidator<StatusRequest>, StatusRequestValidator>();

        // Gateway layer - Refit client for ReceiveMoney with resilience
        services.AddRefitClient<IHubtelReceiveMoneyClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HubtelOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

                var authValue = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{options.ClientId}:{options.ClientSecret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            })
            .AddStandardResilienceHandler(resilienceOptions =>
            {
                // Retry configuration with sensible defaults
                // Users can override via HubtelOptions.Resilience in appsettings.json
                resilienceOptions.Retry.MaxRetryAttempts = 3;
                resilienceOptions.Retry.BackoffType = DelayBackoffType.Exponential;
                resilienceOptions.Retry.Delay = TimeSpan.FromSeconds(1);
                resilienceOptions.Retry.UseJitter = true;
                resilienceOptions.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response =>
                        response.StatusCode == HttpStatusCode.RequestTimeout ||
                        response.StatusCode == HttpStatusCode.TooManyRequests ||
                        (int)response.StatusCode >= 500)
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>();

                // Circuit breaker configuration
                resilienceOptions.CircuitBreaker.MinimumThroughput = 10;
                resilienceOptions.CircuitBreaker.FailureRatio = 0.5;
                resilienceOptions.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                resilienceOptions.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
                resilienceOptions.CircuitBreaker.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response => (int)response.StatusCode >= 500)
                    .Handle<HttpRequestException>();

                // Timeout configuration
                resilienceOptions.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                resilienceOptions.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });

        // Public API layer - Processors
        services.AddScoped<IReceiveMoneyProcessor, ReceiveMoneyProcessor>();
        services.AddScoped<ITransactionStatusProcessor, HubtelStatusService>();

        return services;
    }
}
