using System;

namespace Scynett.Hubtel.Payments.Infrastructure.Configuration;


public class HubtelOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Hubtel";

    /// <summary>
    /// Default base address for the Direct Receive Money API.
    /// </summary>
    public const string DefaultReceiveMoneyBaseAddress = "https://rmp.hubtel.com";

    /// <summary>
    /// Default base address for the Transaction Status API.
    /// </summary>
    public const string DefaultTransactionStatusBaseAddress = "https://api-txnstatus.hubtel.com";

    /// <summary>
    /// Hubtel API Client ID for authentication.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Hubtel API Client Secret for authentication.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Merchant account number (POS Sales ID) from Hubtel.
    /// </summary>
    public string MerchantAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Direct Receive Money endpoint.
    /// </summary>
    public string ReceiveMoneyBaseAddress { get; set; } = DefaultReceiveMoneyBaseAddress;

    /// <summary>
    /// Base URL for Transaction Status endpoint.
    /// </summary>
    public string TransactionStatusBaseAddress { get; set; } = DefaultTransactionStatusBaseAddress;

    /// <summary>
    /// Maintained for backward compatibility. Use <see cref="ReceiveMoneyBaseAddress"/> instead.
    /// </summary>
    [Obsolete("Use ReceiveMoneyBaseAddress instead.")]
    public string BaseAddress
    {
        get => ReceiveMoneyBaseAddress;
        set => ReceiveMoneyBaseAddress = value;
    }

    /// <summary>
    /// HTTP client timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Default callback endpoint for payment notifications.
    /// </summary>
    public string PrimaryCallbackEndPoint { get; set; } = string.Empty;

    /// <summary>
    /// Resilience settings for HTTP retries and circuit breaker.
    /// </summary>
    public ResilienceSettings Resilience { get; set; } = new();
}

/// <summary>
/// Configuration for resilience policies (retry, circuit breaker, timeout).
/// </summary>
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
