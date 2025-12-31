namespace Scynett.Hubtel.Payments.Options;


internal sealed class HubtelResilienceOptions
{
    public RetryOptions Retry { get; init; } = new();
    public CircuitBreakerOptions CircuitBreaker { get; init; } = new();
    public TimeoutOptions Timeout { get; init; } = new();

    public sealed class RetryOptions
    {
        public int MaxRetryAttempts { get; init; } = 3;
        public int DelaySeconds { get; init; } = 1;
        public bool UseJitter { get; init; } = true;
        public string BackoffType { get; init; } = "Exponential"; // "Constant" | "Linear" | "Exponential"
    }

    public sealed class CircuitBreakerOptions
    {
        public int MinimumThroughput { get; init; } = 10;
        public double FailureRatio { get; init; } = 0.5;
        public int SamplingDurationSeconds { get; init; } = 30;
        public int BreakDurationSeconds { get; init; } = 30;
    }

    public sealed class TimeoutOptions
    {
        public int TotalRequestTimeoutSeconds { get; init; } = 30;
        public int AttemptTimeoutSeconds { get; init; } = 10;
    }
}

