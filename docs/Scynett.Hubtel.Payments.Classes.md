# Scynett.Hubtel.Payments Classes

Generated on 2026-01-01T08:58:50Z

## src\Scynett.Hubtel.Payments\Application\Abstractions\Gateways\DirectReceiveMoney\GatewayInitiateReceiveMoneyRequest.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;

/// <summary>
/// Normalized request for initiating a Direct Receive Money transaction
/// through the Hubtel gateway.
/// </summary>
public sealed record GatewayInitiateReceiveMoneyRequest(
    string CustomerName,
    string PosSalesId,
    string CustomerMsisdn,
    string CustomerEmail,
    string Channel,
    string Amount,
#pragma warning disable CA1054 // URI-like parameters should not be strings
#pragma warning disable CA1056 // URI-like properties should not be strings
    string CallbackUrl,
#pragma warning restore CA1056 // URI-like properties should not be strings
#pragma warning restore CA1054 // URI-like parameters should not be strings
    string Description,
    string ClientReference
);

 ``` 

## src\Scynett.Hubtel.Payments\Application\Abstractions\Gateways\DirectReceiveMoney\GatewayInitiateReceiveMoneyResult.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;

/// <summary>
/// Normalized result returned by the Hubtel gateway
/// after initiating a Direct Receive Money transaction.
/// </summary>
public sealed record GatewayInitiateReceiveMoneyResult(
    string ResponseCode,
    string? Message,
    string? TransactionId,
    string? ExternalReference = null,
    string? ExternalTransactionId = null,
    string? OrderId = null,
    string? Description = null,
    decimal? Amount = null,
    decimal? Charges = null,
    decimal? AmountAfterCharges = null,
    decimal? AmountCharged = null,
    decimal? DeliveryFee = null
);

 ``` 

## src\Scynett.Hubtel.Payments\Application\Abstractions\Gateways\DirectReceiveMoney\IHubtelReceiveMoneyGateway.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;


/// <summary>
/// Application boundary for Hubtel Direct Receive Money.
/// </summary>
internal interface IHubtelReceiveMoneyGateway
{
    Task<GatewayInitiateReceiveMoneyResult> InitiateAsync(
        GatewayInitiateReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Abstractions\Gateways\DirectReceiveMoney\IHubtelTransactionStatusGateway.cs

 ```csharp 
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;

internal interface IHubtelTransactionStatusGateway
{
    Task<OperationResult<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusQuery query,
        CancellationToken ct = default);
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Abstractions\Gateways\GatewayTransactionStatusResult.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Abstractions.Gateways;

public sealed record GatewayTransactionStatusResult(
    string ResponseCode,
    string Message,
    string ClientReference,
    string Status,
    decimal Amount,
    decimal Charges,
    decimal AmountAfterCharges,
    string? HubtelTransactionId,
    string? ExternalTransactionId,
    string? PaymentMethod,
    string? CurrencyCode,
    bool? IsFulfilled,
    DateTimeOffset? PaymentDate);

 ``` 

## src\Scynett.Hubtel.Payments\Application\Abstractions\IHubtelReceiveMoneyClient.cs

 ```csharp 
using Refit;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

namespace Scynett.Hubtel.Payments.Application.Abstractions;

/// <summary>
/// Refit client for Hubtel Receive Money API.
/// </summary>
public interface IHubtelReceiveMoneyClient
{
    /// <summary>
    /// Initiates a receive money transaction.
    /// </summary>
    /// <param name="request">The receive money request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response from Hubtel API.</returns>
    [Post("/receive/mobilemoney")]
    Task<InitiateReceiveMoneyResult> ReceiveMobileMoneyAsync(
        [Body] InitiateReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Common\Error.cs

 ```csharp 
using System.Collections.Generic;

namespace Scynett.Hubtel.Payments.Application.Common;

#pragma warning disable CA1716 // Identifiers should not match keywords
public record Error(string Code, string Description, ErrorType Type)
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    public static readonly Error NullValue =
        Validation("General.Null", "Null value was provided");

    public string? ProviderCode { get; init; }
    public string? ProviderMessage { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    public static Error Failure(string code, string description) =>
        new(NormalizeCode(code, "General.Failure"), NormalizeDescription(description), ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(NormalizeCode(code, "General.NotFound"), NormalizeDescription(description), ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(NormalizeCode(code, "General.Problem"), NormalizeDescription(description), ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(NormalizeCode(code, "General.Conflict"), NormalizeDescription(description), ErrorType.Conflict);

    public static Error Validation(string code, string description) =>
        new(NormalizeCode(code, "General.Validation"), NormalizeDescription(description), ErrorType.Validation);

    public static Error From<TEnum>(TEnum code, string description, ErrorType type = ErrorType.Failure)
        where TEnum : Enum
        => new($"{typeof(TEnum).Name}.{code}", NormalizeDescription(description), type);

    public Error WithProvider(string? providerCode, string? providerMessage)
        => this with
        {
            ProviderCode = providerCode,
            ProviderMessage = providerMessage
        };

    public Error WithMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return this;

        var updated = Metadata is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(Metadata, StringComparer.OrdinalIgnoreCase);

        updated[key] = value;
        return this with { Metadata = updated };
    }

    public override string ToString() => $"{Code}: {Description}";

    private static string NormalizeCode(string code, string fallback)
        => string.IsNullOrWhiteSpace(code) ? fallback : code.Trim();

    private static string NormalizeDescription(string description)
        => string.IsNullOrWhiteSpace(description) ? "An error occurred." : description.Trim();
}

 ``` 

## src\Scynett.Hubtel.Payments\Application\Common\ErrorType.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Common;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4,
    Authorization = 5
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Common\HubtelEventIds.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Common;

/// <summary>
/// Event IDs for Hubtel Payments logging.
/// </summary>
public static class HubtelEventIds
{
    // Payment Events (100-199)
    public const int PaymentInitiating = 100;
    public const int PaymentInitResponse = 101;
    public const int PaymentInitError = 102;
    public const int TransactionPending = 103;
    public const int TransactionCompleted = 104;

    // DirectReceiveMoney - Initiate Events (110-129)
    public const int DirectReceiveMoneyInitiating = 110;
    public const int DirectReceiveMoneyValidationFailed = 111;
    public const int DirectReceiveMoneyDecisionComputed = 112;
    public const int DirectReceiveMoneyPendingStored = 113;
    public const int DirectReceiveMoneyPendingButMissingTransactionId = 114;
    public const int DirectReceiveMoneyGatewayFailed = 115;
    public const int DirectReceiveMoneyUnhandledException = 116;

    // DirectReceiveMoney - Callback Events (130-149)
    public const int DirectReceiveMoneyCallbackReceived = 130;
    public const int DirectReceiveMoneyCallbackDecision = 131;
    public const int DirectReceiveMoneyCallbackPendingRemoved = 132;
    public const int DirectReceiveMoneyCallbackValidationFailed = 133;
    public const int DirectReceiveMoneyCallbackProcessingFailed = 134;

    // Callback Events (200-299) - Legacy/Generic
    public const int CallbackReceived = 200;
    public const int CallbackProcessing = 201;
    public const int CallbackProcessed = 202;
    public const int CallbackError = 203;
    public const int CallbackInvalidData = 204;

    // Status Check Events (300-399)
    public const int StatusCheckStarted = 300;
    public const int StatusCheckCompleted = 301;
    public const int StatusCheckFailed = 302;
    public const int StatusCheckError = 303;

    // Worker Events (400-499)
    public const int WorkerStarted = 400;
    public const int WorkerStopped = 401;
    public const int WorkerCheckingTransactions = 402;
    public const int WorkerNoPendingTransactions = 403;
    public const int WorkerTransactionCheckFailed = 404;
    public const int WorkerError = 405;
    public const int WorkerTransactionError = 406;
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Common\LogMessages.cs

 ```csharp 
using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

using System.Net;

namespace Scynett.Hubtel.Payments;

internal static partial class LogMessages
{
    [LoggerMessage(
        EventId = HubtelEventIds.StatusCheckStarted,
        Level = LogLevel.Information,
        Message = "Checking transaction status for {identifier}")]
    internal static partial void CheckingStatus(
        ILogger logger,
        string identifier);

    [LoggerMessage(
        EventId = HubtelEventIds.StatusCheckFailed,
        Level = LogLevel.Warning,
        Message = "Failed to check status. HTTP {statusCode}: {error}")]
    internal static partial void FailedToCheckStatus(
        ILogger logger,
        HttpStatusCode statusCode,
        string error);

    [LoggerMessage(
        EventId = HubtelEventIds.StatusCheckError,
        Level = LogLevel.Error,
        Message = "Error checking transaction status for {identifier}")]
    internal static partial void ErrorCheckingStatus(
        ILogger logger,
        Exception exception,
        string identifier);

    [LoggerMessage(
        EventId = HubtelEventIds.PaymentInitiating,
        Level = LogLevel.Information,
        Message = "Initiating payment for {customerName} - Amount: {amount}, Channel: {channel}")]
    internal static partial void InitiatingPayment(
        ILogger logger,
        string customerName,
        decimal amount,
        string channel);

    [LoggerMessage(
        EventId = HubtelEventIds.PaymentInitResponse,
        Level = LogLevel.Information,
        Message = "Payment init response - Code: {code}, Category: {category}, Message: {message}")]
    internal static partial void PaymentInitResponse(
        ILogger logger,
        string code,
        ResponseCategory category,
        string message);

    [LoggerMessage(
        EventId = HubtelEventIds.TransactionPending,
        Level = LogLevel.Information,
        Message = "Transaction {transactionId} added to pending store")]
    internal static partial void TransactionAddedToPendingStore(
        ILogger logger,
        string transactionId);

    [LoggerMessage(
        EventId = HubtelEventIds.PaymentInitError,
        Level = LogLevel.Error,
        Message = "Error initiating payment for {customerName}")]
    internal static partial void ErrorInitiatingPayment(
        ILogger logger,
        Exception exception,
        string customerName);

    [LoggerMessage(
        EventId = HubtelEventIds.CallbackProcessing,
        Level = LogLevel.Information,
        Message = "Processing callback for transaction {transactionId} - Status: {status}")]
    internal static partial void ProcessingCallback(
        ILogger logger,
        string transactionId,
        string status);

    [LoggerMessage(
        EventId = HubtelEventIds.CallbackProcessed,
        Level = LogLevel.Information,
        Message = "Callback decision - Code: {code}, Category: {category}, IsFinal: {isFinal}")]
    internal static partial void CallbackDecision(
        ILogger logger,
        string code,
        ResponseCategory category,
        bool isFinal);

    [LoggerMessage(
        EventId = HubtelEventIds.TransactionCompleted,
        Level = LogLevel.Information,
        Message = "Transaction {transactionId} removed from pending store")]
    internal static partial void TransactionRemovedFromPendingStore(
        ILogger logger,
        string transactionId);

    [LoggerMessage(
        EventId = HubtelEventIds.CallbackError,
        Level = LogLevel.Error,
        Message = "Error processing callback for transaction {transactionId}")]
    internal static partial void ErrorProcessingCallback(
        ILogger logger,
        Exception exception,
        string transactionId);
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Common\OperationResult.cs

 ```csharp 
using System.Diagnostics.CodeAnalysis;

using CSharpFunctionalExtensions;

namespace Scynett.Hubtel.Payments.Application.Common;

#pragma warning disable CA1000 // Do not declare static members on generic types
public sealed class OperationResult<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private readonly T? _value;
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No value for a failed result.");

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("No error for a successful result.");

    private readonly Error? _error;

    private OperationResult(bool isSuccess, T? value, Error? error)
    {
        if (isSuccess && error is not null)
            throw new ArgumentException("Successful result cannot contain an error.", nameof(error));

        if (!isSuccess && error is null)
            throw new ArgumentException("Failed result must contain an error.", nameof(error));

        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static OperationResult<T> Success(T value) => new(true, value, null);

    public static OperationResult<T> Failure(Error error) => new(false, default, error);

    public static OperationResult<T> From(Result<T, Error> result)
        => result.IsSuccess ? Success(result.Value) : Failure(result.Error);

    public bool TryGetValue([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out Error? error)
    {
        if (IsSuccess)
        {
            value = _value!;
            error = null;
            return true;
        }

        value = default;
        error = _error!;
        return false;
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(_value!) : onFailure(_error!);
    }
}
#pragma warning restore CA1000 // Do not declare static members on generic types

public class OperationResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private readonly Error? _error;
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("No error for a successful result.");

    protected OperationResult(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new ArgumentException("Successful result cannot contain an error.", nameof(error));

        if (!isSuccess && error is null)
            throw new ArgumentException("Failed result must contain an error.", nameof(error));

        IsSuccess = isSuccess;
        _error = error;
    }

    public static OperationResult Success() => new(true, null);

    public static OperationResult Failure(Error error) => new(false, error);

    public static OperationResult From(Result result, Error error)
        => result.IsSuccess ? Success() : Failure(error);
}

 ``` 

## src\Scynett.Hubtel.Payments\Application\Common\PendingTransactionsWorkerLogMessages.cs

 ```csharp 
using Microsoft.Extensions.Logging;

namespace Scynett.Hubtel.Payments.Application.Common;

internal static partial class PendingTransactionsWorkerLogMessages
{
    [LoggerMessage(EventId = 7701, Level = LogLevel.Information,
        Message = "PendingTransactionsWorker started. PollInterval={PollInterval}")]
    public static partial void Started(ILogger logger, TimeSpan pollInterval);

    [LoggerMessage(EventId = 7702, Level = LogLevel.Debug,
        Message = "Polling pending transactions. Count={Count}")]
    public static partial void Polling(ILogger logger, int count);

    [LoggerMessage(EventId = 7703, Level = LogLevel.Information,
        Message = "Pending transaction completed. TransactionId={TransactionId} Status={Status}. Removed.")]
    public static partial void Completed(ILogger logger, string transactionId, string status);

    [LoggerMessage(EventId = 7704, Level = LogLevel.Debug,
        Message = "Pending transaction still pending. TransactionId={TransactionId} Status={Status}")]
    public static partial void StillPending(ILogger logger, string transactionId, string status);

    [LoggerMessage(EventId = 7708, Level = LogLevel.Debug,
        Message = "Skipping transaction {TransactionId} - waiting for callback window to elapse.")]
    public static partial void TooEarly(ILogger logger, string transactionId);

    [LoggerMessage(EventId = 7705, Level = LogLevel.Warning,
        Message = "Status check failed. TransactionId={TransactionId} Code={Code} Message={Message}")]
    public static partial void StatusFailed(ILogger logger, string transactionId, string? code, string? message);

    [LoggerMessage(EventId = 7706, Level = LogLevel.Error,
        Message = "Error processing pending transaction. TransactionId={TransactionId}")]
    public static partial void ProcessingError(ILogger logger, Exception ex, string transactionId);

    [LoggerMessage(EventId = 7707, Level = LogLevel.Error,
        Message = "PendingTransactionsWorker loop error.")]
    public static partial void LoopError(ILogger logger, Exception ex);
}

 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Callback\ReceiveMoneyCallbackLogMessages.cs

 ```csharp 
using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Common;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

internal static partial class ReceiveMoneyCallbackLogMessages
{
    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackReceived,
        Level = LogLevel.Information,
        Message = "Hubtel callback received. ClientReference={ClientReference}, TransactionId={TransactionId}, ResponseCode={ResponseCode}")]
    public static partial void CallbackReceived(
        ILogger logger,
        string clientReference,
        string transactionId,
        string responseCode);

    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackDecision,
        Level = LogLevel.Information,
        Message = "Hubtel callback decision. Code={Code}, Category={Category}, IsFinal={IsFinal}, NextAction={NextAction}")]
    public static partial void CallbackDecision(
        ILogger logger,
        string code,
        string category,
        bool isFinal,
        string nextAction);

    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackPendingRemoved,
        Level = LogLevel.Information,
        Message = "Pending transaction removed. TransactionId={TransactionId}, ClientReference={ClientReference}")]
    public static partial void PendingRemoved(
        ILogger logger,
        string transactionId,
        string clientReference);

    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackValidationFailed,
        Level = LogLevel.Warning,
        Message = "Hubtel callback validation failed. TransactionId={TransactionId}, ClientReference={ClientReference}")]
    public static partial void ValidationFailed(
        ILogger logger,
        string transactionId,
        string clientReference);

    [LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackProcessingFailed,
        Level = LogLevel.Error,
        Message = "Hubtel callback processing failed. TransactionId={TransactionId}, ClientReference={ClientReference}")]
    public static partial void ProcessingFailed(
        ILogger logger,
        Exception exception,
        string transactionId,
        string clientReference);
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Callback\ReceiveMoneyCallbackMapping.cs

 ```csharp 
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

internal static class ReceiveMoneyCallbackMapping
{
    internal static ReceiveMoneyCallbackResult ToResult(
        ReceiveMoneyCallbackRequest callback,
        HandlingDecision decision)
    {
        return new ReceiveMoneyCallbackResult(
            ClientReference: callback.Data.ClientReference,
            TransactionId: callback.Data.TransactionId,
            ResponseCode: callback.ResponseCode,
            Category: decision.Category,
            NextAction: decision.NextAction,
            IsFinal: decision.IsFinal,
            IsSuccess: decision.IsSuccess,
            CustomerMessage: decision.CustomerMessage,
            RawMessage: callback.Message);
    }

    internal static string BuildDecisionMessage(ReceiveMoneyCallbackRequest callback)
    {
        // Hubtel often puts the most useful “variant text” in Data.Description.
        // Use Message + Description to refine 2001 variants.
        var msg = callback.Message?.Trim();
        var desc = callback.Data.Description?.Trim();

        return string.IsNullOrWhiteSpace(desc)
            ? (msg ?? string.Empty)
            : string.IsNullOrWhiteSpace(msg)
                ? desc
                : $"{msg}. {desc}";
    }
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Callback\ReceiveMoneyCallbackProcessor.cs

 ```csharp 
using FluentValidation;

using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Infrastructure.Storage;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public sealed class ReceiveMoneyCallbackProcessor(
    IPendingTransactionsStore pendingStore,
    IValidator<ReceiveMoneyCallbackRequest> validator,
    ILogger<ReceiveMoneyCallbackProcessor> logger)
{
    public async Task<OperationResult<ReceiveMoneyCallbackResult>> ExecuteAsync(
        ReceiveMoneyCallbackRequest callback,
        CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(callback, ct).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            ReceiveMoneyCallbackLogMessages.ValidationFailed(
                logger,
                callback.Data?.TransactionId ?? string.Empty,
                callback.Data?.ClientReference ?? string.Empty);

            return OperationResult<ReceiveMoneyCallbackResult>.Failure(
                Error.Validation("Hubtel.Callback.Validation", validation.ToString()));
        }

        try
        {
            ReceiveMoneyCallbackLogMessages.CallbackReceived(
                logger,
                callback.Data.ClientReference,
                callback.Data.TransactionId,
                callback.ResponseCode);

            var messageForDecision = ReceiveMoneyCallbackMapping.BuildDecisionMessage(callback);

            var decision = HubtelResponseDecisionFactory.Create(
                callback.ResponseCode,
                messageForDecision);

            ReceiveMoneyCallbackLogMessages.CallbackDecision(
                logger,
                decision.Code,
                decision.Category.ToString(),
                decision.IsFinal,
                decision.NextAction.ToString());

            // For callbacks, response is final for 0000 and 2001; but we still follow decision.IsFinal.
            if (decision.IsFinal)
            {
                // Remove pending by TransactionId (Hubtel callback always contains TransactionId).
                await pendingStore.RemoveAsync(callback.Data.TransactionId, ct).ConfigureAwait(false);

                ReceiveMoneyCallbackLogMessages.PendingRemoved(
                    logger,
                    callback.Data.TransactionId,
                    callback.Data.ClientReference);
            }

            var result = ReceiveMoneyCallbackMapping.ToResult(callback, decision);

            // Decide whether to treat non-success callbacks as failures:
            // - If you want the endpoint to return 200 always (recommended), still return Success here,
            //   but include IsSuccess=false in the result. However, OperationResult should reflect
            //   whether *processing* succeeded, not payment success.
            return OperationResult<ReceiveMoneyCallbackResult>.Success(result);
        }
        catch (Exception ex)
        {
            ReceiveMoneyCallbackLogMessages.ProcessingFailed(
                logger,
                ex,
                callback.Data.TransactionId,
                callback.Data.ClientReference);

            return OperationResult<ReceiveMoneyCallbackResult>.Failure(
                Error.Problem(
                        "Hubtel.Callback.Exception",
                        "An error occurred while processing the Hubtel callback.")
                    .WithMetadata("exception", ex.GetType().Name));
        }
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Callback\ReceiveMoneyCallbackRequest.cs

 ```csharp 
using System.Text.Json.Serialization;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public sealed record ReceiveMoneyCallbackRequest(
    [property: JsonPropertyName("ResponseCode")] string ResponseCode,
    [property: JsonPropertyName("Message")] string? Message,
    [property: JsonPropertyName("Data")] ReceiveMoneyCallbackData Data);

public sealed record ReceiveMoneyCallbackData(
    [property: JsonPropertyName("Amount")] decimal Amount,
    [property: JsonPropertyName("Charges")] decimal? Charges,
    [property: JsonPropertyName("AmountAfterCharges")] decimal? AmountAfterCharges,
    [property: JsonPropertyName("AmountCharged")] decimal? AmountCharged,
    [property: JsonPropertyName("Description")] string? Description,

    [property: JsonPropertyName("ClientReference")] string ClientReference,
    [property: JsonPropertyName("TransactionId")] string TransactionId,
    [property: JsonPropertyName("ExternalTransactionId")] string? ExternalTransactionId,
    [property: JsonPropertyName("OrderId")] string? OrderId,
    [property: JsonPropertyName("PaymentDate")] DateTimeOffset? PaymentDate
);
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Callback\ReceiveMoneyCallbackRequestValidator.cs

 ```csharp 
using FluentValidation;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public sealed class ReceiveMoneyCallbackRequestValidator : AbstractValidator<ReceiveMoneyCallbackRequest>
{
    public ReceiveMoneyCallbackRequestValidator()
    {
        RuleFor(x => x.ResponseCode)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Data)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(x => x.Data!.ClientReference)
                    .NotEmpty()
                    .MaximumLength(36);

                RuleFor(x => x.Data!.TransactionId)
                    .NotEmpty();

                RuleFor(x => x.Data!.Amount)
                    .GreaterThan(0);

                RuleFor(x => x.Data!.PaymentDate)
                    .Must(_ => true);
            });
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Callback\ReceiveMoneyCallbackResult.cs

 ```csharp 
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public sealed record ReceiveMoneyCallbackResult(
    string ClientReference,
    string TransactionId,
    string ResponseCode,
    ResponseCategory Category,
    NextAction NextAction,
    bool IsFinal,
    bool IsSuccess,
    string? CustomerMessage,
    string? RawMessage
);
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Decisions\HandlingDecision.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

public sealed record HandlingDecision(
       string Code,
       string Description,
       NextAction NextAction,
       ResponseCategory Category,
       bool IsSuccess = false,
       bool IsFinal = true,
       bool ShouldRetry = false,
       string? CustomerMessage = null,
       string? DeveloperHint = null);
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Decisions\HubtelResponseDecisionFactory.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

public static class HubtelResponseDecisionFactory
{
    // Core code mapping (independent of message variants)
    private static readonly Dictionary<string, HandlingDecision> ByCode =
        new Dictionary<string, HandlingDecision>(StringComparer.OrdinalIgnoreCase)
        {
            ["0000"] = new HandlingDecision(
                Code: "0000",
                Description: "The transaction has been processed successfully.",
                NextAction: NextAction.None,
                Category: ResponseCategory.Success,
                IsSuccess: true,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "Payment successful."),

            ["0001"] = new HandlingDecision(
                Code: "0001",
                Description: "Request has been accepted. A callback will be sent on final state.",
                NextAction: NextAction.WaitForCallback,
                Category: ResponseCategory.Pending,
                IsSuccess: false,
                IsFinal: false,
                ShouldRetry: false,
                CustomerMessage: "Payment request received. Please confirm on your phone.",
                DeveloperHint: "Do not mark as failed. Persist as Pending and await callback/webhook."
            ),

            // 2001 has many message variants, handled below with message-based enrichment.
            ["2001"] = new HandlingDecision(
                Code: "2001",
                Description: "Mobile money customer/rail failure (PIN, funds, limits, timeout, invalid transaction, etc.).",
                NextAction: NextAction.AskCustomerToRetry,
                Category: ResponseCategory.CustomerError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "Payment could not be completed. Please try again.",
                DeveloperHint: "Use message text to refine: insufficient funds vs wrong PIN vs timeout vs invalid transaction."
            ),

            ["4000"] = new HandlingDecision(
                Code: "4000",
                Description: "Validation errors. Something is not quite right with this request.",
                NextAction: NextAction.FixRequest,
                Category: ResponseCategory.ValidationError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "We couldn’t start the payment due to invalid details.",
                DeveloperHint: "Inspect request payload; log validation details returned by Hubtel."
            ),

            ["4070"] = new HandlingDecision(
                Code: "4070",
                Description: "Unable to complete payment at the moment. Fees not set for given conditions.",
                NextAction: NextAction.ContactHubtelOrRSE,
                Category: ResponseCategory.ConfigurationError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: true,
                CustomerMessage: "Payment is temporarily unavailable. Please try again later.",
                DeveloperHint: "Ensure minimum amount is passed; otherwise contact Hubtel relationship manager to setup fees."
            ),

            ["4101"] = new HandlingDecision(
                Code: "4101",
                Description: "Business not fully set up / auth scopes mismatch / keys mismatch / POS sales number missing.",
                NextAction: NextAction.CheckAuthAndKeys,
                Category: ResponseCategory.ConfigurationError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "Payment service is not available for this merchant at the moment.",
                DeveloperHint: "Check Basic Auth, correct API keys, required scopes (e.g., mobilemoney-receive-direct), and POS Sales number."
            ),

            ["4103"] = new HandlingDecision(
                Code: "4103",
                Description: "Permission denied. Account not allowed to transact on this channel.",
                NextAction: NextAction.NotAllowed,
                Category: ResponseCategory.PermissionError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "This payment method is not available right now.",
                DeveloperHint: "Contact Hubtel Retail Systems Engineer to enable channel permissions."
            ),
        };

    /// <summary>
    /// Main factory method. Provide the Hubtel response code and (optionally) the raw message/description.
    /// The message is used to refine 2001 cases into clearer customer guidance.
    /// </summary>
    public static HandlingDecision Create(string? code, string? message = null)
    {
        code = (code ?? string.Empty).Trim();
        message = Normalize(message);

        if (!ByCode.TryGetValue(code, out var baseDecision))
        {
            return new HandlingDecision(
                Code: string.IsNullOrWhiteSpace(code) ? "UNKNOWN" : code,
                Description: "Unknown response code from Hubtel.",
                NextAction: NextAction.RetryLater,
                Category: ResponseCategory.Unknown,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: true,
                CustomerMessage: "We couldn’t complete the payment. Please try again.",
                DeveloperHint: $"Unhandled Hubtel response code '{code}'. Capture and map it. Raw message: '{message ?? ""}'."
            );
        }

        // Refine 2001 variants using message contents.
        if (code.Equals("2001", StringComparison.OrdinalIgnoreCase))
            return Refine2001(baseDecision, message);

        return baseDecision;
    }

    private static HandlingDecision Refine2001(HandlingDecision baseDecision, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return baseDecision;

        // Funds / limits
        if (ContainsAny(message, "insufficient funds", "avail. balance", "balance limits", "counter", "limit"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToCheckFunds,
                CustomerMessage = "Insufficient funds or account limits reached. Please top up and try again.",
                DeveloperHint = "Customer funds/limits issue (not a system error)."
            };
        }

        // Wrong PIN / invalid PIN
        if (ContainsAny(message, "wrong pin", "invalid pin", "entered the wrong pin", "no or invalid pin", "missing permissions"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToRetry,
                CustomerMessage = "Incorrect PIN. Please try again.",
                DeveloperHint = "Customer entered wrong/invalid PIN."
            };
        }

        // USSD timeout / session timeout
        if (ContainsAny(message, "ussd session timeout", "session timeout", "timeout"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToRetry,
                CustomerMessage = "The confirmation timed out. Please try again and confirm promptly.",
                DeveloperHint = "USSD session timed out."
            };
        }

        // Invalid transaction id
        if (ContainsAny(message, "transaction id is invalid", "invalid transaction"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToRetry,
                CustomerMessage = "Payment could not be verified. Please try again.",
                DeveloperHint = "Invalid/unknown transaction id returned by rail/provider."
            };
        }

        // Network parsing / strange characters
        if (ContainsAny(message, "not able to parse", "strange characters", "(&*!%@)", "parse your request"))
        {
            return baseDecision with
            {
                NextAction = NextAction.FixRequest,
                Category = ResponseCategory.ValidationError,
                CustomerMessage = "We couldn’t start the payment due to invalid details. Please try again.",
                DeveloperHint = "Sanitize/validate description and request fields; avoid special characters in descriptions."
            };
        }

        // Channel mismatch / wrong number
        if (ContainsAny(message, "matches the channel", "number provided matches the channel", "ensure that the number provided matches"))
        {
            return baseDecision with
            {
                NextAction = NextAction.FixRequest,
                Category = ResponseCategory.ValidationError,
                CustomerMessage = "The phone number doesn’t match the selected network. Please correct it and try again.",
                DeveloperHint = "Ensure MSISDN matches channel/network (MTN/Vodafone/AirtelTigo)."
            };
        }

        // FRI not found / account holder issues
        if (ContainsAny(message, "fri not found", "account holder"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToRetry,
                CustomerMessage = "We couldn’t validate this account. Please confirm your number and try again.",
                DeveloperHint = "Provider account holder lookup failed (FRI not found)."
            };
        }

        // Vodafone cash generic failure
        if (ContainsAny(message, "vodafone cash failed", "vodafone failed"))
        {
            return baseDecision with
            {
                NextAction = NextAction.RetryLater,
                Category = ResponseCategory.TransientError,
                ShouldRetry = true,
                CustomerMessage = "Vodafone Cash is currently unavailable. Please try again later.",
                DeveloperHint = "Likely provider-side transient issue."
            };
        }

        // Default fallback: keep base
        return baseDecision with
        {
            CustomerMessage = "Payment could not be completed. Please try again.",
            DeveloperHint = $"2001 variant not specifically mapped. Raw message: '{message}'."
        };
    }

    private static bool ContainsAny(string haystack, params string[] needles)
    {
        foreach (var n in needles)
        {
            if (haystack.Contains(n, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string? Normalize(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Decisions\NextAction.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

public enum NextAction
{
    None,                   // success
    WaitForCallback,        // accepted/pending
    AskCustomerToRetry,     // wrong pin / ussd timeout / temporary user-side issue
    AskCustomerToCheckFunds,// insufficient funds / limits
    FixRequest,             // validation / bad payload
    RetryLater,             // transient / fees not set / service unavailable
    ContactHubtelOrRSE,     // configuration/scopes/fees/channel not enabled
    CheckAuthAndKeys,       // auth mismatch / wrong basic auth / keys mismatch
    NotAllowed              // permission denied for channel
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Decisions\ResponseCategory.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

public enum ResponseCategory
{
    Success,
    Pending,
    CustomerError,
    ValidationError,
    ConfigurationError,
    PermissionError,
    TransientError,
    Unknown
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Initiate\InitiateReceiveMoneyLogMessages.cs

 ```csharp 
using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Common;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

internal static partial class InitiateReceiveMoneyLogMessages
{
    [LoggerMessage(
        EventId = HubtelEventIds.DirectReceiveMoneyValidationFailed,
        Level = LogLevel.Warning,
        Message = "DirectReceiveMoney validation failed. ClientReference={ClientReference}. Error={Error}")]
    public static partial void ValidationFailed(
        ILogger logger,
        string clientReference,
        string error);

    [LoggerMessage(
        EventId = HubtelEventIds.DirectReceiveMoneyInitiating,
        Level = LogLevel.Information,
        Message = "Initiating DirectReceiveMoney. ClientReference={ClientReference}, Amount={Amount}, Network={Network}, Msisdn={Msisdn}")]
    public static partial void Initiating(
        ILogger logger,
        string clientReference,
        decimal amount,
        string network,
        string msisdn);

    [LoggerMessage(
        EventId = HubtelEventIds.DirectReceiveMoneyGatewayFailed,
        Level = LogLevel.Error,
        Message = "DirectReceiveMoney gateway call failed. ClientReference={ClientReference}. Error={ErrorCode} {ErrorDescription}")]
    public static partial void GatewayFailed(
        ILogger logger,
        string clientReference,
        string errorCode,
        string errorDescription);

    [LoggerMessage(
        EventId = HubtelEventIds.DirectReceiveMoneyDecisionComputed,
        Level = LogLevel.Information,
        Message = "DirectReceiveMoney decision computed. ClientReference={ClientReference}, Code={Code}, Category={Category}, NextAction={NextAction}, IsFinal={IsFinal}")]
    public static partial void DecisionComputed(
        ILogger logger,
        string clientReference,
        string code,
        string category,
        string nextAction,
        bool isFinal);

    [LoggerMessage(
        EventId = HubtelEventIds.DirectReceiveMoneyPendingStored,
        Level = LogLevel.Information,
        Message = "Stored pending DirectReceiveMoney transaction. ClientReference={ClientReference}, TransactionId={TransactionId}")]
    public static partial void PendingStored(
        ILogger logger,
        string clientReference,
        string transactionId);

    [LoggerMessage(
        EventId = HubtelEventIds.DirectReceiveMoneyPendingButMissingTransactionId,
        Level = LogLevel.Warning,
        Message = "DirectReceiveMoney pending decision returned but TransactionId is missing. ClientReference={ClientReference}, Code={Code}")]
    public static partial void PendingButMissingTransactionId(
        ILogger logger,
        string clientReference,
        string code);

    [LoggerMessage(
        EventId = HubtelEventIds.DirectReceiveMoneyUnhandledException,
        Level = LogLevel.Error,
        Message = "Unhandled exception while initiating DirectReceiveMoney. ClientReference={ClientReference}")]
    public static partial void UnhandledException(
        ILogger logger,
        Exception exception,
        string clientReference);
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Initiate\InitiateReceiveMoneyMapping.cs

 ```csharp 
using System.Globalization;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

internal static class InitiateReceiveMoneyMapping
{
    internal static GatewayInitiateReceiveMoneyRequest ToGatewayRequest(
        InitiateReceiveMoneyRequest request,
        string posSalesId,
        string callbackUrl)
    {
        return new GatewayInitiateReceiveMoneyRequest(
            CustomerName: request.CustomerName ?? string.Empty,
            PosSalesId: posSalesId,
            CustomerMsisdn: request.CustomerMobileNumber,
            CustomerEmail: request.CustomerEmail ?? string.Empty,
            Channel: request.Channel,
            Amount: request.Amount.ToString("F2", CultureInfo.InvariantCulture),
            CallbackUrl: callbackUrl,
            Description: request.Description,
            ClientReference: request.ClientReference);
    }

    internal static InitiateReceiveMoneyResult ToResult(
        InitiateReceiveMoneyRequest request,
        GatewayInitiateReceiveMoneyResult gateway,
        HandlingDecision decision)
    {
        var status = decision.Category.ToString(); // e.g. Success / Pending / CustomerError / ValidationError

        return new InitiateReceiveMoneyResult(
            ClientReference: request.ClientReference,
            HubtelTransactionId: gateway.TransactionId ?? string.Empty,
            ExternalTransactionId: gateway.ExternalTransactionId,
            OrderId: gateway.OrderId,
            Status: status,
            Amount: gateway.Amount ?? request.Amount,
            Charges: gateway.Charges ?? 0m,
            AmountAfterCharges: gateway.AmountAfterCharges ?? request.Amount,
            AmountCharged: gateway.AmountCharged ?? request.Amount,
            Network: request.Channel,
            RawResponseCode: gateway.ResponseCode,
            Message: gateway.Message,
            Description: gateway.Description,
            DeliveryFee: gateway.DeliveryFee);
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Initiate\InitiateReceiveMoneyProcessor.cs

 ```csharp 
using System.Globalization;

using FluentValidation;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.Storage;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

internal sealed class InitiateReceiveMoneyProcessor(
    IHubtelReceiveMoneyGateway gateway,
    IPendingTransactionsStore pendingStore,
    IOptions<HubtelOptions> hubtelOptions,
    IOptions<DirectReceiveMoneyOptions> directReceiveMoneyOptions,
    IValidator<InitiateReceiveMoneyRequest> validator,
    ILogger<InitiateReceiveMoneyProcessor> logger)
{
    public async Task<OperationResult<InitiateReceiveMoneyResult>> ExecuteAsync(
        InitiateReceiveMoneyRequest request,
        CancellationToken ct = default)
    {
        // 1) Validate
        var validation = await validator.ValidateAsync(request, ct).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            var message = validation.Errors.Count > 0
                ? validation.Errors[0].ErrorMessage
                : "Validation failed.";

            InitiateReceiveMoneyLogMessages.ValidationFailed(
                logger,
                request.ClientReference,
                message);

            return OperationResult<InitiateReceiveMoneyResult>.Failure(
                Error.Validation("DirectReceiveMoney.ValidationFailed", message));
        }

        try
        {
            InitiateReceiveMoneyLogMessages.Initiating(
                logger,
                request.ClientReference,
                request.Amount,
                request.Channel,
                MaskMsisdn(request.CustomerMobileNumber));

            // Determine which POS Sales ID to use
            var posSalesId = !string.IsNullOrWhiteSpace(directReceiveMoneyOptions.Value.PosSalesId)
                ? directReceiveMoneyOptions.Value.PosSalesId
                : hubtelOptions.Value.MerchantAccountNumber;

            if (string.IsNullOrWhiteSpace(posSalesId))
            {
                return OperationResult<InitiateReceiveMoneyResult>.Failure(
                    Error.Validation(
                        "DirectReceiveMoney.MissingPosSalesId",
                        "POS Sales ID is not configured. Please set either HubtelOptions.MerchantAccountNumber or DirectReceiveMoneyOptions.PosSalesIdOverride."));
            }

            // 2) Map request -> gateway request
            var gatewayRequest = new GatewayInitiateReceiveMoneyRequest(
                CustomerName: request.CustomerName ?? string.Empty,
                PosSalesId: posSalesId,
                CustomerMsisdn: request.CustomerMobileNumber,
                CustomerEmail: request.CustomerEmail ?? string.Empty,
                Channel: request.Channel,
                Amount: request.Amount.ToString("F2", CultureInfo.InvariantCulture),
                CallbackUrl: request.PrimaryCallbackEndPoint,
                Description: request.Description,
                ClientReference: request.ClientReference);

            // 3) Call gateway - returns GatewayInitiateReceiveMoneyResult directly
            var gatewayResponse = await gateway.InitiateAsync(gatewayRequest, ct)
                .ConfigureAwait(false);

            // 4) Decision (your factory)
            var decision = HubtelResponseDecisionFactory.Create(
                gatewayResponse.ResponseCode,
                gatewayResponse.Message);

            InitiateReceiveMoneyLogMessages.DecisionComputed(
                logger,
                request.ClientReference,
                decision.Code,
                decision.Category.ToString(),
                decision.NextAction.ToString(),
                decision.IsFinal);

            // 5) Persist pending when waiting for callback
            if (decision.NextAction == NextAction.WaitForCallback)
            {
                if (!string.IsNullOrWhiteSpace(gatewayResponse.TransactionId))
                {
                    await pendingStore.AddAsync(
                        gatewayResponse.TransactionId,
                        DateTimeOffset.UtcNow,
                        ct).ConfigureAwait(false);

                    InitiateReceiveMoneyLogMessages.PendingStored(
                        logger,
                        request.ClientReference,
                        gatewayResponse.TransactionId);
                }
                else
                {
                    InitiateReceiveMoneyLogMessages.PendingButMissingTransactionId(
                        logger,
                        request.ClientReference,
                        decision.Code);
                    // Just logginthis .What do I have to do????
                }
            }

            // 6) Build result using mapper
            var result = InitiateReceiveMoneyMapping.ToResult(
                request,
                gatewayResponse,
                decision);

            // 7) Check if this is a failure scenario that should return failure
            if (!decision.IsSuccess && decision.IsFinal)
            {
                InitiateReceiveMoneyLogMessages.GatewayFailed(
                    logger,
                    request.ClientReference,
                    gatewayResponse.ResponseCode,
                    gatewayResponse.Message ?? "No message provided");

                return OperationResult<InitiateReceiveMoneyResult>.Failure(
                    Error.Failure(
                            $"DirectReceiveMoney.{decision.Category}",
                            decision.CustomerMessage ?? gatewayResponse.Message ?? "Payment initialization failed")
                        .WithProvider(gatewayResponse.ResponseCode, gatewayResponse.Message));
            }

            // SDK-friendly: Success if we got a Hubtel response; consumer interprets decision via result
            return OperationResult<InitiateReceiveMoneyResult>.Success(result);
        }
#pragma warning disable CA1031 // Do not catch general exception types - Required for SDK error handling
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            InitiateReceiveMoneyLogMessages.UnhandledException(
                logger,
                ex,
                request.ClientReference);

            return OperationResult<InitiateReceiveMoneyResult>.Failure(
                Error.Problem(
                        "DirectReceiveMoney.UnhandledException",
                        "An unexpected error occurred while initiating the payment.")
                    .WithMetadata("exception", ex.GetType().Name));
        }
    }

    private static string DetermineStatus(HandlingDecision decision)
    {
        if (decision.IsSuccess)
            return "Success";

        if (decision.IsFinal)
            return "Failed";

        return "Pending";
    }

    private static string MaskMsisdn(string msisdn)
    {
        if (string.IsNullOrWhiteSpace(msisdn) || msisdn.Length < 6)
            return "****";

        // 0241234567 -> 024***567
        var prefix = msisdn[..3];
        var suffix = msisdn[^3..];
        return $"{prefix}***{suffix}";
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Initiate\InitiateReceiveMoneyRequest.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

// <summary>
/// Request to initiate a receive money transaction.
// </summary>

public sealed record InitiateReceiveMoneyRequest(
    string? CustomerName,
    string? CustomerEmail,
    string CustomerMobileNumber,
    string Channel,
    decimal Amount,
    string Description,
    string ClientReference,
    string PrimaryCallbackEndPoint);

 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Initiate\InitiateReceiveMoneyRequestValidator.cs

 ```csharp 
using FluentValidation;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;


/// <summary>
/// Validator for ReceiveMoneyRequest based on Hubtel API specifications.
/// </summary>
public class InitiateReceiveMoneyRequestValidator : AbstractValidator<InitiateReceiveMoneyRequest>
{
    private static readonly string[] ValidChannels = ["mtn-gh", "vodafone-gh", "tigo-gh"];

    public InitiateReceiveMoneyRequestValidator()
    {
        RuleFor(x => x.CustomerName)
        .MaximumLength(100)
        .WithMessage("Customer name must not exceed 100 characters")
        .When(x => !string.IsNullOrWhiteSpace(x.CustomerName));

        RuleFor(x => x.CustomerEmail)
            .MaximumLength(256)
            .WithMessage("Customer email must not exceed 256 characters")
            .EmailAddress()
            .WithMessage("Customer email must be a valid email address")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            .WithMessage("Customer email must be a valid email address")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail));

        RuleFor(x => x.CustomerMobileNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Customer mobile number is required (Mandatory)")
            .Matches(@"^[0-9]{12}$")
            .WithMessage("Mobile number must be 12 digits in international format (e.g., 233241234567)")
            .Must(number => number.StartsWith("233", StringComparison.Ordinal))
            .WithMessage("Mobile number must start with Ghana country code 233");

        RuleFor(x => x.Channel)
            .NotEmpty()
            .WithMessage("Payment channel is required (Mandatory)")
            .Must(channel => ValidChannels.Any(vc => vc.Equals(channel, StringComparison.OrdinalIgnoreCase)))
            .WithMessage($"Channel must be one of: {string.Join(", ", ValidChannels)}");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0 (Mandatory)")
            .PrecisionScale(10, 2, ignoreTrailingZeros: true)
            .WithMessage("Amount must have at most 2 decimal places (e.g., 0.50)");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required (Mandatory)")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.ClientReference)
            .NotEmpty()
            .WithMessage("Client reference is required (Mandatory) and must be unique for every transaction")
            .MaximumLength(36)
            .WithMessage("Client reference must not exceed 36 characters")
            .Matches(@"^[a-zA-Z0-9]+$")
            .WithMessage("Client reference should preferably be alphanumeric characters");

        RuleFor(x => x.PrimaryCallbackEndPoint)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Primary callback URL is required (Mandatory)")
            .Must(BeAValidUrl)
            .WithMessage("Callback endpoint must be a valid HTTP or HTTPS URL");
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Initiate\InitiateReceiveMoneyResult.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

/// <summary>
/// Result of initiating a Direct Receive Money transaction.
/// </summary>
public sealed record InitiateReceiveMoneyResult(
    string ClientReference,
    string HubtelTransactionId,
    string? ExternalTransactionId,
    string? OrderId,
    string Status,              // Pending / Success / Failed
    decimal Amount,
    decimal Charges,
    decimal AmountAfterCharges,
    decimal AmountCharged,
    string Network,
    string RawResponseCode,
    string? Message = null,
    string? Description = null,
    decimal? DeliveryFee = null);
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Status\TransactionStatusLogMessages.cs

 ```csharp 
using Microsoft.Extensions.Logging;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

internal static partial class TransactionStatusLogMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Invalid transaction status request: {ClientReference}")]
    public static partial void InvalidRequest(this ILogger logger, string clientReference);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Checking Hubtel transaction status for ClientReference={ClientReference}")]
    public static partial void CheckingStatus(this ILogger logger, string clientReference);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Error while checking transaction status for {ClientReference}")]
    public static partial void StatusCheckError(this ILogger logger, Exception exception, string clientReference);
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Status\TransactionStatusMapping.cs

 ```csharp 
using Scynett.Hubtel.Payments.Application.Abstractions.Gateways;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

internal static class TransactionStatusMapping
{
    internal static TransactionStatusResult ToResult(
        GatewayTransactionStatusResult gateway,
        HandlingDecision decision)
    {
        return new TransactionStatusResult(
            ClientReference: gateway.ClientReference,
            Status: gateway.Status,
            Amount: gateway.Amount,
            Charges: gateway.Charges,
            AmountAfterCharges: gateway.AmountAfterCharges,
            TransactionId: gateway.HubtelTransactionId, 
            ExternalTransactionId: gateway.ExternalTransactionId,
            PaymentMethod: gateway.PaymentMethod,
            CurrencyCode: gateway.CurrencyCode,
            IsFulfilled: gateway.IsFulfilled,
            Date: gateway.PaymentDate,
            RawResponseCode: decision.Code,
            RawMessage: decision.CustomerMessage);
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Status\TransactionStatusProcessor.cs

 ```csharp 
using FluentValidation;

using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

internal sealed class TransactionStatusProcessor(
    IHubtelTransactionStatusGateway gateway, 
    IValidator<TransactionStatusQuery> validator,
    ILogger<TransactionStatusProcessor> logger)
{
    public async Task<OperationResult<TransactionStatusResult>> CheckAsync(
        TransactionStatusQuery query,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(query, ct).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            logger.InvalidRequest(GetLogKey(query));
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Validation("TransactionStatus.InvalidQuery", validation.ToString()));
        }

        try
        {
            var key = GetLogKey(query);
            logger.CheckingStatus(key);

            var gatewayResult = await gateway.CheckStatusAsync(query, ct).ConfigureAwait(false);

            if (gatewayResult.IsFailure)
                return OperationResult<TransactionStatusResult>.Failure(gatewayResult.Error!);

            // If your status endpoint returns "responseCode" always 0000 for successful call
            // you can still apply decisions if you want, but typically it’s not needed.
            // Keeping it here is fine if you already have code mapping logic.
            var decision = HubtelResponseDecisionFactory.Create(
                gatewayResult.Value!.RawResponseCode,
                gatewayResult.Value!.RawMessage);

            if (!decision.IsSuccess && decision.IsFinal)
            {
                return OperationResult<TransactionStatusResult>.Failure(
                    Error.Failure(
                            $"TransactionStatus.{decision.Category}",
                            decision.CustomerMessage ?? "Transaction status check failed")
                        .WithProvider(gatewayResult.Value!.RawResponseCode, gatewayResult.Value!.RawMessage));
            }

            return OperationResult<TransactionStatusResult>.Success(gatewayResult.Value!);
        }
        catch (Exception ex)
        {
            logger.StatusCheckError(ex, GetLogKey(query));
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem("TransactionStatus.Exception",
                        "An error occurred while checking transaction status")
                    .WithMetadata("exception", ex.GetType().Name));
        }
    }

    private static string GetLogKey(TransactionStatusQuery q)
        => q.ClientReference
        ?? q.HubtelTransactionId
        ?? q.NetworkTransactionId
        ?? "unknown";
}

 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Status\TransactionStatusQuery.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

public sealed record TransactionStatusQuery(
    string? ClientReference = null,
    string? HubtelTransactionId = null,
    string? NetworkTransactionId = null)
{
    public bool HasAnyIdentifier =>
        !string.IsNullOrWhiteSpace(ClientReference) ||
        !string.IsNullOrWhiteSpace(HubtelTransactionId) ||
        !string.IsNullOrWhiteSpace(NetworkTransactionId);
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Status\TransactionStatusQueryValidator.cs

 ```csharp 
using FluentValidation;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

internal sealed class TransactionStatusQueryValidator : AbstractValidator<TransactionStatusQuery>
{
    public TransactionStatusQueryValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.ClientReference) ||
                !string.IsNullOrWhiteSpace(x.HubtelTransactionId) ||
                !string.IsNullOrWhiteSpace(x.NetworkTransactionId))
            .WithMessage("At least one identifier is required: clientReference, hubtelTransactionId, or networkTransactionId.");

        RuleFor(x => x.ClientReference)
            .MaximumLength(36)
            .When(x => !string.IsNullOrWhiteSpace(x.ClientReference));

        // Optional: keep these sane (Hubtel IDs are usually 32 hex, but don’t over-restrict)
        RuleFor(x => x.HubtelTransactionId)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.HubtelTransactionId));

        RuleFor(x => x.NetworkTransactionId)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.NetworkTransactionId));
    }
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Status\TransactionStatusRequest.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

public sealed record TransactionStatusRequest(string ClientReference);
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Status\TransactionStatusRequestValidator.cs

 ```csharp 
using FluentValidation;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;


internal sealed class TransactionStatusRequestValidator
    : AbstractValidator<TransactionStatusRequest>
{
    public TransactionStatusRequestValidator()
    {
        RuleFor(x => x.ClientReference)
            .NotEmpty()
            .MaximumLength(36);
    }
}
 ``` 

## src\Scynett.Hubtel.Payments\Application\Features\DirectReceiveMoney\Status\TransactionStatusResult.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

public sealed record TransactionStatusResult(
    string Status,
    string? ClientReference,
    string? TransactionId,
    string? ExternalTransactionId,
    string? PaymentMethod,
    string? CurrencyCode,
    bool? IsFulfilled,
    decimal? Amount,
    decimal? Charges,
    decimal? AmountAfterCharges,
    DateTimeOffset? Date,
    string? RawResponseCode,
    string? RawMessage);

 ``` 

## src\Scynett.Hubtel.Payments\AssemblyInfo.cs

 ```csharp 
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Scynett.Hubtel.Payments.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

 ``` 

## src\Scynett.Hubtel.Payments\GlobalSuppressions.cs

 ```csharp 
using System.Diagnostics.CodeAnalysis;

// CA1716: "Error" is a well-established type name in the Result pattern
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is a domain-specific type name following the Result pattern", Scope = "type", Target = "~T:Scynett.Hubtel.Payments.Application.Common.Error")]

// CA1716: namespace names align with SDK features (DependencyInjection, DirectReceiveMoney)
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Namespace names describe SDK surface area", Scope = "namespace", Target = "~N:Scynett.Hubtel.Payments.DependencyInjection")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Namespace names describe SDK surface area", Scope = "namespace", Target = "~N:Scynett.Hubtel.Payments.DirectReceiveMoney")]

// CA1056: Base addresses are stored as strings for configuration flexibility
[assembly: SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "Base addresses are stored as strings for configuration flexibility", Scope = "member", Target = "~P:Scynett.Hubtel.Payments.Options.HubtelOptions.ReceiveMoneyBaseAddress")]
[assembly: SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "Base addresses are stored as strings for configuration flexibility", Scope = "member", Target = "~P:Scynett.Hubtel.Payments.Options.HubtelOptions.TransactionStatusBaseAddress")]

// CA1031: Catching general exceptions at service boundaries is acceptable for logging and graceful failure
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Service boundary methods catch all exceptions for logging and graceful failure", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Application.Features")]

// CA1062: IOptions and command parameters are validated by the DI container and framework
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by the DI container and framework", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Application.Features")]

// CA1062: Extension method parameters are validated by the compiler and caller
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Extension method parameters are guaranteed non-null by the compiler", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Validation")]

// CA1812: Internal classes are instantiated by DI container
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Internal classes are instantiated by dependency injection", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Application.Features")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Internal classes are instantiated by dependency injection", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Infrastructure")]

// CA1852: Internal types that don't need inheritance should be sealed
[assembly: SuppressMessage("Performance", "CA1852:Seal internal types", Justification = "Internal types may be extended in future versions", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments")]

// CA1000: Static members on generic types are acceptable for factory methods
[assembly: SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are appropriate for Result pattern", Scope = "member", Target = "~M:Scynett.Hubtel.Payments.Application.Common.OperationResult`1.FromTValue(`0)~Scynett.Hubtel.Payments.Application.Common.OperationResult{`0}")]



 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\BackgroundWorkers\PendingTransactionsWorker.cs

 ```csharp 
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.Infrastructure.BackgroundWorkers;

internal sealed class PendingTransactionsWorker(
    IPendingTransactionsStore store,
    IDirectReceiveMoney directReceiveMoney,
    IOptions<PendingTransactionsWorkerOptions> options,
    ILogger<PendingTransactionsWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollInterval = options.Value.PollInterval;

        PendingTransactionsWorkerLogMessages.Started(logger, pollInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);

            try
            {
                await Task.Delay(pollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Background worker must swallow exceptions and continue polling.")]
    internal async Task ProcessBatchAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        var callbackWait = options.Value.CallbackGracePeriod;

        try
        {
            var pending = await store.GetAllAsync(stoppingToken).ConfigureAwait(false);
            PendingTransactionsWorkerLogMessages.Polling(logger, pending.Count);

            foreach (var transaction in pending)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    if (DateTimeOffset.UtcNow - transaction.CreatedAtUtc < callbackWait)
                    {
                        PendingTransactionsWorkerLogMessages.TooEarly(logger, transaction.HubtelTransactionId ?? "unknown");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(transaction.HubtelTransactionId))
                    {
                        PendingTransactionsWorkerLogMessages.StatusFailed(
                            logger,
                            "unknown",
                            "MissingTransactionId",
                            "Pending transaction is missing HubtelTransactionId.");
                        continue;
                    }

                    var query = new TransactionStatusQuery(HubtelTransactionId: transaction.HubtelTransactionId);

                    var result = await directReceiveMoney
                        .CheckStatusAsync(query, stoppingToken)
                        .ConfigureAwait(false);

                    if (result.IsFailure)
                    {
                        PendingTransactionsWorkerLogMessages.StatusFailed(
                            logger,
                            transaction.HubtelTransactionId,
                            result.Error?.Code,
                            result.Error?.Description);

                        continue;
                    }

                    var status = result.Value?.Status ?? "Unknown";

                    if (IsFinal(status))
                    {
                        await store.RemoveAsync(transaction.HubtelTransactionId, stoppingToken).ConfigureAwait(false);
                        PendingTransactionsWorkerLogMessages.Completed(logger, transaction.HubtelTransactionId, status);
                    }
                    else
                    {
                        PendingTransactionsWorkerLogMessages.StillPending(logger, transaction.HubtelTransactionId, status);
                    }
                }
                catch (Exception ex)
                {
                    PendingTransactionsWorkerLogMessages.ProcessingError(logger, ex, transaction.HubtelTransactionId ?? "unknown");
                }
            }
        }
        catch (Exception ex)
        {
            PendingTransactionsWorkerLogMessages.LoopError(logger, ex);
        }
    }

    private static bool IsFinal(string status)
        => status.Equals("success", StringComparison.OrdinalIgnoreCase)
        || status.Equals("failed", StringComparison.OrdinalIgnoreCase)
        || status.Equals("paid", StringComparison.OrdinalIgnoreCase)
        || status.Equals("unpaid", StringComparison.OrdinalIgnoreCase)
        || status.Equals("refunded", StringComparison.OrdinalIgnoreCase);
}


 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Configuration\DirectReceiveMoneyOptions.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Options;

public sealed class DirectReceiveMoneyOptions
{
    /// <summary>
    /// Default callback URL used when the request does not explicitly specify one.
    /// </summary>
    public string DefaultCallbackAddress { get; set; } = string.Empty;

    /// <summary>
    /// Optional override for the POS Sales ID for Direct Receive Money.
    /// If empty, the global HubtelOptions.PosSalesId is used.
    /// </summary>
    public string? PosSalesId { get; set; }
}
 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Configuration\HubtelOptions.cs

 ```csharp 
using System;

namespace Scynett.Hubtel.Payments.Options;


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

 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Configuration\HubtelResilienceOptions.cs

 ```csharp 
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

 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Configuration\PendingTransactionsWorkerOptions.cs

 ```csharp 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scynett.Hubtel.Payments.Options;

public sealed class PendingTransactionsWorkerOptions
{
    /// <summary>
    /// Polling interval for checking pending transactions. Default: 1 minute.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Max number of pending items to process per run (prevents long loops).
    /// </summary>
    public int BatchSize { get; set; } = 200;

    /// <summary>
    /// How long to wait for the primary callback before polling status.
    /// </summary>
    public TimeSpan CallbackGracePeriod { get; set; } = TimeSpan.FromMinutes(5);
}
 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Gateways\HubtelReceiveMoneyGateway.cs

 ```csharp 
using Refit;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways;
using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

using System.Text.Json;

namespace Scynett.Hubtel.Payments.Infrastructure.Gateways;

/// <summary>
/// Infrastructure implementation of IHubtelReceiveMoneyGateway.
/// Responsible for HTTP, DTO mapping, and transport error handling.
/// </summary>
internal sealed class HubtelReceiveMoneyGateway(
    IHubtelDirectReceiveMoneyApi api)
    : IHubtelReceiveMoneyGateway
{
    public async Task<GatewayInitiateReceiveMoneyResult> InitiateAsync(
        GatewayInitiateReceiveMoneyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = new InitiateReceiveMoneyRequestDto(
                CustomerName: request.CustomerName,
                CustomerMsisdn: request.CustomerMsisdn,
                Channel: request.Channel,
                CustomerEmail: request.CustomerEmail,
                Amount: request.Amount,
                PrimaryCallbackEndpoint: request.CallbackUrl,
                Description: request.Description,
                ClientReference: request.ClientReference);

            var response = await api.InitiateAsync(
                request.PosSalesId,
                dto,
                cancellationToken).ConfigureAwait(false);

            var content = response.Content
                ?? throw new InvalidOperationException("Hubtel returned empty response body.");

            return new GatewayInitiateReceiveMoneyResult(
                ResponseCode: content.ResponseCode,
                Message: content.Message,
                TransactionId: content.Data?.TransactionId,
                ExternalReference: content.Data?.ClientReference,
                ExternalTransactionId: content.Data?.ExternalTransactionId,
                OrderId: content.Data?.OrderId,
                Description: content.Data?.Description,
                Amount: content.Data?.Amount,
                Charges: content.Data?.Charges,
                AmountAfterCharges: content.Data?.AmountAfterCharges,
                AmountCharged: content.Data?.AmountCharged,
                DeliveryFee: content.Data?.DeliveryFee);
        }
        catch (ApiException ex)
        {
            var parsed = TryParseError(ex);

            return new GatewayInitiateReceiveMoneyResult(
                ResponseCode: "HTTP_ERROR",
                Message: parsed?.Message ?? ex.Message,
                TransactionId: null,
                ExternalReference: null);
        }
    }

    private static HubtelApiErrorDto? TryParseError(ApiException ex)
    {
        if (string.IsNullOrWhiteSpace(ex.Content))
            return null;

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            return JsonSerializer.Deserialize<HubtelApiErrorDto>(ex.Content);
        }
        catch
        {
            return null;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Gateways\HubtelTransactionStatusGateway.cs

 ```csharp 
using System.Globalization;

using Microsoft.Extensions.Options;

using Refit;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.Infrastructure.Gateways;

internal sealed class HubtelTransactionStatusGateway(
    IHubtelTransactionStatusApi api,
    IOptions<DirectReceiveMoneyOptions> directReceiveMoneyOptions,
    IOptions<HubtelOptions> hubtelOptions)
    : IHubtelTransactionStatusGateway
{
    public async Task<OperationResult<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusQuery query,
        CancellationToken ct = default)
    {
        var posSalesId = !string.IsNullOrWhiteSpace(directReceiveMoneyOptions.Value.PosSalesId)
            ? directReceiveMoneyOptions.Value.PosSalesId
            : hubtelOptions.Value.MerchantAccountNumber;

        if (string.IsNullOrWhiteSpace(posSalesId))
        {
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem("Config.PosSalesId", "POS Sales ID is not configured."));
        }

        try
        {
            var response = await api.GetStatusAsync(
                posSalesId,
                clientReference: query.ClientReference,
                hubtelTransactionId: query.HubtelTransactionId,
                networkTransactionId: query.NetworkTransactionId,
                ct).ConfigureAwait(false);

            if (!string.Equals(response.ResponseCode, "0000", StringComparison.OrdinalIgnoreCase))
            {
                return OperationResult<TransactionStatusResult>.Failure(
                    Error.Failure(
                            "Hubtel.StatusCheckFailed",
                            response.Message ?? "Status check failed")
                        .WithProvider(response.ResponseCode, response.Message));
            }

            var data = response.Data;

            return OperationResult<TransactionStatusResult>.Success(
                new TransactionStatusResult(
                    Status: data?.Status ?? "Unknown",
                    ClientReference: data?.ClientReference,
                    TransactionId: data?.TransactionId,
                    ExternalTransactionId: data?.ExternalTransactionId,
                    PaymentMethod: data?.PaymentMethod,
                    CurrencyCode: data?.CurrencyCode,
                    IsFulfilled: data?.IsFulfilled,
                    Amount: data?.Amount,
                    Charges: data?.Charges,
                    AmountAfterCharges: data?.AmountAfterCharges,
                    Date: data?.Date,
                    RawResponseCode: response.ResponseCode,
                    RawMessage: response.Message));
        }
        catch (ApiException apiEx)
        {
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem("Hubtel.Http.Status",
                        "Failed to contact Hubtel status endpoint.")
                    .WithMetadata("statusCode", ((int)apiEx.StatusCode).ToString(CultureInfo.InvariantCulture))
                    .WithMetadata("reason", apiEx.ReasonPhrase ?? "unknown"));
        }
#pragma warning disable CA1031 // We intentionally convert all exceptions into OperationResult failures for transport resiliency
        catch (Exception ex)
        {
            return OperationResult<TransactionStatusResult>.Failure(
                Error.Problem("Hubtel.Http.Status",
                        "Failed to contact Hubtel status endpoint.")
                    .WithMetadata("exception", ex.GetType().Name));
        }
#pragma warning restore CA1031
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Http\HubtelAuthHandler.cs

 ```csharp 
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Options;

using System.Net.Http.Headers;
using System.Text;

namespace Scynett.Hubtel.Payments.Infrastructure.Http;

internal sealed class HubtelAuthHandler(IOptions<HubtelOptions> options)
    : DelegatingHandler
{
    private readonly string _authValue =
        Convert.ToBase64String(
            Encoding.ASCII.GetBytes(
                $"{options.Value.ClientId}:{options.Value.ClientSecret}"));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", _authValue);

        return base.SendAsync(request, cancellationToken);
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Http\HubtelHttpPolicies.cs

 ```csharp 
using Microsoft.Extensions.Http.Resilience;

using Polly;

using Scynett.Hubtel.Payments.Options;

using System.Net;

namespace Scynett.Hubtel.Payments.Infrastructure.Http;

internal static class HubtelHttpPolicies
{
    public static void Apply(HttpStandardResilienceOptions options, HubtelResilienceOptions cfg)
    {
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
    }

    private static DelayBackoffType ParseBackoff(string? value)
        => value?.Trim().ToUpperInvariant() switch
        {
            "CONSTANT" => DelayBackoffType.Constant,
            "LINEAR" => DelayBackoffType.Linear,
            _ => DelayBackoffType.Exponential
        };
}

 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Http\Refit\DirectReceiveMoney\Dtos\HubtelApiErrorDto.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;


/// <summary>
/// Represents an error payload returned by Hubtel when the HTTP request fails
/// (e.g. authentication, authorization, or server errors).
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes - Instantiated by Refit for error responses
internal sealed record HubtelApiErrorDto
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
{
    public int? Status { get; init; }
    public string? Error { get; init; }
    public string? Message { get; init; }
    public string? Path { get; init; }
    public string? Timestamp { get; init; }
}
 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Http\Refit\DirectReceiveMoney\Dtos\InitiateReceiveMoneyRequestDto.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

internal record InitiateReceiveMoneyRequestDto(string CustomerName,
    string CustomerMsisdn,
    string CustomerEmail,
    string Channel,
    string Amount,
    string PrimaryCallbackEndpoint,
    string Description,
    string ClientReference);
 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Http\Refit\DirectReceiveMoney\Dtos\InitiateReceiveMoneyResponseDto.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

/// <summary>
/// Response model from Hubtel Receive Money API.
/// </summary>
public sealed record InitiateReceiveMoneyResponseDto(
    string ResponseCode,
    string Message,
    HubtelReceiveMoneyData? Data);

/// <summary>
/// Transaction data from Hubtel Receive Money API response.
/// </summary>
public sealed record HubtelReceiveMoneyData(
    string? TransactionId,
    string? ClientReference,
    string? Description,
    decimal? Amount,
    decimal? Charges,
    decimal? AmountAfterCharges,
    decimal? AmountCharged,
    decimal? DeliveryFee,
    string? ExternalTransactionId,
    string? OrderId);

 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Http\Refit\DirectReceiveMoney\Dtos\TransactionStatusResponseDto.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

/// <summary>
/// Raw response from Hubtel Transaction Status API.
/// </summary>
internal sealed class TransactionStatusResponseDto
{
    public string? Message { get; init; }        // "Successful"
    public string? ResponseCode { get; init; }   // "0000"
    public TransactionStatusDataDto? Data { get; init; }
}

/// <summary>
/// Data payload returned by Hubtel for a transaction status.
/// </summary>
internal sealed class TransactionStatusDataDto
{
    public DateTimeOffset? Date { get; init; }

    /// <summary>
    /// Paid | Unpaid | Refunded
    /// </summary>
    public string? Status { get; init; }

    public string? TransactionId { get; init; }
    public string? ExternalTransactionId { get; init; }

    public string? PaymentMethod { get; init; }      // "mobilemoney"
    public string? ClientReference { get; init; }

    public string? CurrencyCode { get; init; }

    public decimal? Amount { get; init; }
    public decimal? Charges { get; init; }
    public decimal? AmountAfterCharges { get; init; }

    public bool? IsFulfilled { get; init; }
}
 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Http\Refit\DirectReceiveMoney\IHubtelDirectReceiveMoneyApi.cs

 ```csharp 
using Refit;

using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;


namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;

/// <summary>
/// Refit API for Hubtel Direct Receive Money.
/// </summary>
internal interface IHubtelDirectReceiveMoneyApi
{
    /// <summary>
    /// Initiate a Mobile Money debit (Direct Receive Money).
    /// POST /merchantaccount/merchants/{POS_Sales_ID}/receive/mobilemoney
    /// </summary>
    [Post("/merchantaccount/merchants/{posSalesId}/receive/mobilemoney")]
    Task<ApiResponse<InitiateReceiveMoneyResponseDto>> InitiateAsync(
        string posSalesId,
        [Body] InitiateReceiveMoneyRequestDto request,
        CancellationToken cancellationToken);
}


 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Http\Refit\DirectReceiveMoney\IHubtelTransactionStatusApi.cs

 ```csharp 
using Refit;

using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;

internal interface IHubtelTransactionStatusApi
{
    [Get("/transactions/{posSalesId}/status")]
    Task<TransactionStatusResponseDto> GetStatusAsync(
        [AliasAs("posSalesId")] string posSalesId,
        [Query] string? clientReference = null,
        [Query] string? hubtelTransactionId = null,
        [Query] string? networkTransactionId = null,
        CancellationToken ct = default);
}
 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Storage\InMemoryPendingTransactionsStore.cs

 ```csharp 
using System.Collections.Concurrent;

namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public sealed class InMemoryPendingTransactionsStore : IPendingTransactionsStore
{
    private sealed record Entry(DateTimeOffset CreatedAtUtc);

    private readonly ConcurrentDictionary<string, Entry> _transactions = new(StringComparer.OrdinalIgnoreCase);

    public Task AddAsync(string transactionId, DateTimeOffset createdAtUtc, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(transactionId))
            _transactions.TryAdd(transactionId, new Entry(createdAtUtc));

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(transactionId))
            _transactions.TryRemove(transactionId, out _);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PendingTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = _transactions.Select(pair =>
            new PendingTransaction(
                ClientReference: pair.Key,
                HubtelTransactionId: pair.Key,
                CreatedAtUtc: pair.Value.CreatedAtUtc)).ToList();

        return Task.FromResult<IReadOnlyList<PendingTransaction>>(results);
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Storage\IPendingTransactionsStore.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public interface IPendingTransactionsStore
{
    Task AddAsync(string transactionId, DateTimeOffset createdAtUtc, CancellationToken cancellationToken = default);
    Task RemoveAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all pending transaction IDs to poll.
    /// </summary>
    Task<IReadOnlyList<PendingTransaction>> GetAllAsync(CancellationToken cancellationToken = default);
}

 ``` 

## src\Scynett.Hubtel.Payments\Infrastructure\Storage\PendingTransaction.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public sealed record PendingTransaction(
    string ClientReference,
    string? HubtelTransactionId,
    DateTimeOffset CreatedAtUtc);
 ``` 

## src\Scynett.Hubtel.Payments\obj\Debug\net9.0\.NETCoreApp,Version=v9.0.AssemblyAttributes.cs

 ```csharp 
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
 ``` 

## src\Scynett.Hubtel.Payments\obj\Debug\net9.0\Scynett.Hubtel.Payments.AssemblyInfo.cs

 ```csharp 
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("Scynett")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyCopyrightAttribute("Copyright (c) 2026 Scynett")]
[assembly: System.Reflection.AssemblyDescriptionAttribute(("A modern .NET SDK for Hubtel Mobile Money payment integration with built-in resil" +
    "ience, observability, and production-ready patterns."))]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+6a0d9d24e4d84ea2ffafde31a2836573a703e743")]
[assembly: System.Reflection.AssemblyProductAttribute("Scynett.Hubtel.Payments")]
[assembly: System.Reflection.AssemblyTitleAttribute("Scynett.Hubtel.Payments")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyMetadataAttribute("RepositoryUrl", "https://github.com/scynett/Scynett.Hubtel.Payments")]

// Generated by the MSBuild WriteCodeFragment class.

 ``` 

## src\Scynett.Hubtel.Payments\obj\Debug\net9.0\Scynett.Hubtel.Payments.GlobalUsings.g.cs

 ```csharp 
// <auto-generated/>
global using System;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Threading;
global using System.Threading.Tasks;
 ``` 

## src\Scynett.Hubtel.Payments\Public\DependencyInjection\ServiceCollectionExtensions.cs

 ```csharp 
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



 ``` 

## src\Scynett.Hubtel.Payments\Public\DirectReceiveMoney\DirectReceiveMoney.cs

 ```csharp 


using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.DirectReceiveMoney;

internal sealed class DirectReceiveMoney(
    InitiateReceiveMoneyProcessor initiateProcessor,
    ReceiveMoneyCallbackProcessor callbackProcessor,
    TransactionStatusProcessor statusProcessor) : IDirectReceiveMoney
{
    public Task<OperationResult<InitiateReceiveMoneyResult>> InitiateAsync(
        InitiateReceiveMoneyRequest request,
        CancellationToken cancellationToken = default)
        => initiateProcessor.ExecuteAsync(request, cancellationToken);

    public Task<OperationResult<ReceiveMoneyCallbackResult>> HandleCallbackAsync(
        ReceiveMoneyCallbackRequest callback,
        CancellationToken cancellationToken = default)
        => callbackProcessor.ExecuteAsync(callback, cancellationToken);

    public Task<OperationResult<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusQuery query,
        CancellationToken cancellationToken = default)
        => statusProcessor.CheckAsync(query, cancellationToken);
}
 ``` 

## src\Scynett.Hubtel.Payments\Public\DirectReceiveMoney\IDirectReceiveMoney.cs

 ```csharp 
using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

namespace Scynett.Hubtel.Payments.DirectReceiveMoney;


/// <summary>
/// Direct Mobile Money receive operations (MoMo Debit).
/// </summary>
public interface IDirectReceiveMoney
{
    Task<OperationResult<InitiateReceiveMoneyResult>> InitiateAsync(
          InitiateReceiveMoneyRequest request,
          CancellationToken cancellationToken = default);

    Task<OperationResult<ReceiveMoneyCallbackResult>> HandleCallbackAsync(
        ReceiveMoneyCallbackRequest callback,
        CancellationToken cancellationToken = default);

    Task<OperationResult<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusQuery query,
        CancellationToken cancellationToken = default);
}
 ``` 

## src\Scynett.Hubtel.Payments\Public\HubtelPaymentsException.cs

 ```csharp 
using Scynett.Hubtel.Payments.Application.Common;

namespace Scynett.Hubtel.Payments;

/// <summary>
/// Exception thrown when a Hubtel operation result represents a failure and the consumer opts into exception-based flows.
/// </summary>
public sealed class HubtelPaymentsException : Exception
{
    public HubtelPaymentsException()
        : this(Error.Problem("Hubtel.Exception", "Hubtel operation failed."))
    {
    }

    public HubtelPaymentsException(string message)
        : this(Error.Problem("Hubtel.Exception", message))
    {
    }

    public HubtelPaymentsException(string message, Exception innerException)
        : this(Error.Problem("Hubtel.Exception", message), innerException)
    {
    }

    public HubtelPaymentsException(Error error, Exception? innerException = null)
        : base(FormatMessage(error), innerException)
    {
        Error = error;
    }

    public Error Error { get; }

    private static string FormatMessage(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return $"{error.Code}: {error.Description}";
    }
}

 ``` 

## src\Scynett.Hubtel.Payments\Public\IHubtelPayments.cs

 ```csharp 
using Scynett.Hubtel.Payments.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments;

/// <summary>
/// Root entry point for all Hubtel payment capabilities.
/// </summary>
internal interface IHubtelPayments
{
    /// <summary>
    /// Direct Mobile Money receive operations (MoMo Debit).
    /// </summary>
    IDirectReceiveMoney DirectReceiveMoney { get; }
}
 ``` 

## src\Scynett.Hubtel.Payments\Public\OperationResultExtensions.cs

 ```csharp 
using Scynett.Hubtel.Payments.Application.Common;

namespace Scynett.Hubtel.Payments;

public static class OperationResultExtensions
{
    public static T OrThrow<T>(this OperationResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? result.Value : throw new HubtelPaymentsException(result.Error);
    }
}

 ``` 

