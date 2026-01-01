using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Infrastructure.BackgroundWorkers;
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Tests.Testing;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class PendingTransactionsWorkerLoggingTests : UnitTestBase
{
    [Fact]
    public async Task Log_ShouldWriteCompletionWithTransactionId_WhenStatusIsFinalSuccess()
    {
        var logger = new TestLogger<PendingTransactionsWorker>();
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateTransaction("txn-success") });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.Setup(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessResult("Paid"));

        using var worker = CreateWorker(store.Object, direct.Object, logger: logger);

        await worker.ProcessBatchAsync(CancellationToken.None);

        logger.Entries.Should().Contain(entry =>
            entry.Message.Contains("Pending transaction completed", StringComparison.OrdinalIgnoreCase) &&
            entry.Message.Contains("txn-success", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Log_ShouldWriteFailureWithTransactionId_WhenStatusIsFinalFailure()
    {
        var logger = new TestLogger<PendingTransactionsWorker>();
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateTransaction("txn-failure") });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.Setup(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<TransactionStatusResult>.Failure(
                Error.Failure("Gateway.Failure", "failed")));

        using var worker = CreateWorker(store.Object, direct.Object, logger: logger);

        await worker.ProcessBatchAsync(CancellationToken.None);

        logger.Entries.Should().Contain(entry =>
            entry.Message.Contains("Status check failed", StringComparison.OrdinalIgnoreCase) &&
            entry.Message.Contains("txn-failure", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Log_ShouldWriteTooEarlyWithTransactionId_WhenWithinGracePeriod()
    {
        var logger = new TestLogger<PendingTransactionsWorker>();
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateTransaction("txn-early", DateTimeOffset.UtcNow) });

        var direct = new Mock<IDirectReceiveMoney>(MockBehavior.Strict);

        using var worker = CreateWorker(store.Object, direct.Object, new PendingTransactionsWorkerOptions
        {
            CallbackGracePeriod = TimeSpan.FromMinutes(2),
            PollInterval = TimeSpan.Zero
        }, logger);

        await worker.ProcessBatchAsync(CancellationToken.None);

        logger.Entries.Should().Contain(entry =>
            entry.Message.Contains("waiting for callback window", StringComparison.OrdinalIgnoreCase) &&
            entry.Message.Contains("txn-early", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Log_ShouldLogSkippingNullTransactionId_WhenEncountered()
    {
        var logger = new TestLogger<PendingTransactionsWorker>();
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new PendingTransaction("client", null, DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10)) });

        var direct = new Mock<IDirectReceiveMoney>(MockBehavior.Strict);

        using var worker = CreateWorker(store.Object, direct.Object, logger: logger);

        await worker.ProcessBatchAsync(CancellationToken.None);

        logger.Entries.Should().Contain(entry =>
            entry.Message.Contains("MissingTransactionId", StringComparison.OrdinalIgnoreCase));
    }

    private static PendingTransactionsWorker CreateWorker(
        IPendingTransactionsStore store,
        IDirectReceiveMoney directReceiveMoney,
        PendingTransactionsWorkerOptions? options = null,
        ILogger<PendingTransactionsWorker>? logger = null)
        => new(
            store,
            directReceiveMoney,
            Microsoft.Extensions.Options.Options.Create(options ?? new PendingTransactionsWorkerOptions
            {
                CallbackGracePeriod = TimeSpan.Zero,
                PollInterval = TimeSpan.Zero
            }),
            logger ?? NullLogger<PendingTransactionsWorker>.Instance);

    private static PendingTransaction CreateTransaction(string transactionId, DateTimeOffset? created = null)
        => new("client", transactionId, created ?? DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10));

    private static OperationResult<TransactionStatusResult> SuccessResult(string status)
        => OperationResult<TransactionStatusResult>.Success(
            new TransactionStatusResult(
                Status: status,
                ClientReference: "client",
                TransactionId: "txn",
                ExternalTransactionId: null,
                PaymentMethod: null,
                CurrencyCode: null,
                IsFulfilled: null,
                Amount: null,
                Charges: null,
                AmountAfterCharges: null,
                Date: null,
                RawResponseCode: "0000",
                RawMessage: "ok"));
}



