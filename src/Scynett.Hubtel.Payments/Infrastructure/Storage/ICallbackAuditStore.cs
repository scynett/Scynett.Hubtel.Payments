namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

public interface ICallbackAuditStore
{
    Task<CallbackAuditStartResult> TryStartAsync(
        string transactionId,
        string payloadHash,
        string rawPayload,
        DateTimeOffset receivedAtUtc,
        CancellationToken cancellationToken = default);

    Task SaveResultAsync(
        string transactionId,
        ReceiveMoneyCallbackResult result,
        bool isSuccess,
        string responseCode,
        DateTimeOffset processedAtUtc,
        CancellationToken cancellationToken = default);

    Task MarkFailureAsync(
        string transactionId,
        CancellationToken cancellationToken = default);
}

public sealed record CallbackAuditStartResult(
    bool CanProcess,
    ReceiveMoneyCallbackResult? ExistingResult);
