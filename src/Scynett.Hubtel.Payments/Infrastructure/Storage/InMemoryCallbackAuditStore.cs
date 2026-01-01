using System.Collections.Concurrent;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

namespace Scynett.Hubtel.Payments.Infrastructure.Storage;

public sealed class InMemoryCallbackAuditStore : ICallbackAuditStore
{
    private sealed class AuditEntry
    {
        public bool IsProcessing { get; set; }
        public string PayloadHash { get; set; } = string.Empty;
        public string RawPayload { get; set; } = string.Empty;
        public DateTimeOffset ReceivedAtUtc { get; set; }
        public ReceiveMoneyCallbackResult? Result { get; set; }
        public bool? IsSuccess { get; set; }
        public string? ResponseCode { get; set; }
        public DateTimeOffset? ProcessedAtUtc { get; set; }
    }

    private readonly ConcurrentDictionary<string, AuditEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    public Task<CallbackAuditStartResult> TryStartAsync(
        string transactionId,
        string payloadHash,
        string rawPayload,
        DateTimeOffset receivedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var entry = _entries.GetOrAdd(transactionId, _ => new AuditEntry());

        lock (entry)
        {
            if (entry.Result is not null)
            {
                return Task.FromResult(new CallbackAuditStartResult(false, entry.Result));
            }

            if (entry.IsProcessing)
            {
                return Task.FromResult(new CallbackAuditStartResult(false, null));
            }

            entry.IsProcessing = true;
            entry.PayloadHash = payloadHash;
            entry.RawPayload = rawPayload;
            entry.ReceivedAtUtc = receivedAtUtc;
        }

        return Task.FromResult(new CallbackAuditStartResult(true, null));
    }

    public Task SaveResultAsync(
        string transactionId,
        ReceiveMoneyCallbackResult result,
        bool isSuccess,
        string responseCode,
        DateTimeOffset processedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (_entries.TryGetValue(transactionId, out var entry))
        {
            lock (entry)
            {
                entry.IsProcessing = false;
                entry.Result = result;
                entry.IsSuccess = isSuccess;
                entry.ResponseCode = responseCode;
                entry.ProcessedAtUtc = processedAtUtc;
            }
        }
        else
        {
            var newEntry = new AuditEntry
            {
                Result = result,
                IsSuccess = isSuccess,
                ResponseCode = responseCode,
                ProcessedAtUtc = processedAtUtc,
                IsProcessing = false
            };
            _entries[transactionId] = newEntry;
        }

        return Task.CompletedTask;
    }

    public Task MarkFailureAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        if (_entries.TryGetValue(transactionId, out var entry))
        {
            lock (entry)
            {
                entry.IsProcessing = false;
            }
        }

        return Task.CompletedTask;
    }
}
