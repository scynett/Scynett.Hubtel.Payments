using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using Polly;

using Scynett.Hubtel.Payments.Options;

using System.Net;


namespace Scynett.Hubtel.Payments.Infrastructure.Http;

internal static class HubtelHttpPolicies
{
    public static Action<HttpStandardResilienceOptions> ConfigureFrom(
        IOptions<HubtelResilienceOptions> configured)
        => options =>
        {
            var cfg = configured.Value;

            // Retry
            options.Retry.MaxRetryAttempts = cfg.Retry.MaxRetryAttempts;
            options.Retry.Delay = TimeSpan.FromSeconds(cfg.Retry.DelaySeconds);
            options.Retry.UseJitter = cfg.Retry.UseJitter;
            options.Retry.BackoffType = ParseBackoff(cfg.Retry.BackoffType);

            options.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .HandleResult(r =>
                    r.StatusCode == HttpStatusCode.RequestTimeout ||
                    r.StatusCode == HttpStatusCode.TooManyRequests ||
                    (int)r.StatusCode >= 500)
                .Handle<HttpRequestException>()
                .Handle<TimeoutException>();

            // Circuit breaker
            options.CircuitBreaker.MinimumThroughput = cfg.CircuitBreaker.MinimumThroughput;
            options.CircuitBreaker.FailureRatio = cfg.CircuitBreaker.FailureRatio;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(cfg.CircuitBreaker.SamplingDurationSeconds);
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(cfg.CircuitBreaker.BreakDurationSeconds);

            options.CircuitBreaker.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .HandleResult(r => (int)r.StatusCode >= 500)
                .Handle<HttpRequestException>()
                .Handle<TimeoutException>();

            // Timeouts
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(cfg.Timeout.TotalRequestTimeoutSeconds);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(cfg.Timeout.AttemptTimeoutSeconds);
        };

    private static DelayBackoffType ParseBackoff(string? value)
        => value?.Trim().ToUpperInvariant() switch
        {
            "CONSTANT" => DelayBackoffType.Constant,
            "LINEAR" => DelayBackoffType.Linear,
            _ => DelayBackoffType.Exponential
        };
}
