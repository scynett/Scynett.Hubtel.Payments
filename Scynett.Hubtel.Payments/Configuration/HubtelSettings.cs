namespace Scynett.Hubtel.Payments.Configuration;

public class HubtelSettings
{
    public const string SectionName = "Hubtel";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string MerchantAccountNumber { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.hubtel.com";
    public int TimeoutSeconds { get; set; } = 30;
    public string PrimaryCallbackEndPoint { get; set; } = string.Empty;

    public ResilienceSettings Resilience { get; set; } = new();
}

public class ResilienceSettings
{
    /// <summary>
    /// Enable or disable retry policy.
    /// </summary>
    public bool EnableRetries { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts for transient failures.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay in seconds for exponential backoff.
    /// </summary>
    public double BaseDelaySeconds { get; set; } = 1;

    /// <summary>
    /// Circuit breaker - minimum number of requests before opening circuit.
    /// </summary>
    public int CircuitBreakerMinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Circuit breaker - failure threshold percentage (0-1).
    /// </summary>
    public double CircuitBreakerFailureThreshold { get; set; } = 0.5;

    /// <summary>
    /// Circuit breaker - sampling duration in seconds.
    /// </summary>
    public int CircuitBreakerSamplingDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Circuit breaker - duration to keep circuit open in seconds.
    /// </summary>
    public int CircuitBreakerBreakDurationSeconds { get; set; } = 30;
}
