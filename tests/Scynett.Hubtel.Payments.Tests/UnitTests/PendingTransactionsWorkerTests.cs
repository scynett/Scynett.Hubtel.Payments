using System;
using System.Diagnostics.CodeAnalysis;
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
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Unit tests run without a synchronization context.")]
public sealed class PendingTransactionsWorkerTests : UnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_ShouldNotPoll_WhenWithinGracePeriod()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTransaction("txn-1", DateTimeOffset.UtcNow)
            });

        var direct = new Mock<IDirectReceiveMoney>(MockBehavior.Strict);

        using var worker = CreateWorker(store.Object, direct.Object, new PendingTransactionsWorkerOptions
        {
            CallbackGracePeriod = TimeSpan.FromMinutes(5)
        });

        await worker.ProcessBatchAsync(CancellationToken.None);

        direct.Verify(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPoll_WhenGracePeriodHasElapsed()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTransaction("txn-2", DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10))
            });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.Setup(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessResult("Paid"));

        using var worker = CreateWorker(store.Object, direct.Object, new PendingTransactionsWorkerOptions
        {
            CallbackGracePeriod = TimeSpan.FromMinutes(5)
        });

        await worker.ProcessBatchAsync(CancellationToken.None);

        direct.Verify(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSkipTransaction_WhenTransactionIdIsNullOrEmpty()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PendingTransaction("client", null, DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10))
            });

        var direct = new Mock<IDirectReceiveMoney>(MockBehavior.Strict);

        using var worker = CreateWorker(store.Object, direct.Object);

        await worker.ProcessBatchAsync(CancellationToken.None);

        direct.Verify(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPollAfterLongWait_WhenGracePeriodExceededByLargeMargin()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTransaction("txn-3", DateTimeOffset.UtcNow - TimeSpan.FromDays(1))
            });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.Setup(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessResult("Paid"));

        using var worker = CreateWorker(store.Object, direct.Object);

        await worker.ProcessBatchAsync(CancellationToken.None);

        direct.Verify(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Paid")]
    [InlineData("Success")]
    public async Task ExecuteAsync_ShouldRemoveTransaction_WhenStatusIsFinalSuccess(string status)
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTransaction("txn-4")
            });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.Setup(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessResult(status));

        using var worker = CreateWorker(store.Object, direct.Object);

        await worker.ProcessBatchAsync(CancellationToken.None);

        store.Verify(s => s.RemoveAsync("txn-4", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Unpaid")]
    [InlineData("Refunded")]
    public async Task ExecuteAsync_ShouldRemoveTransaction_WhenStatusIsFinalFailure(string status)
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTransaction("txn-final")
            });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.Setup(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessResult(status));

        using var worker = CreateWorker(store.Object, direct.Object);

        await worker.ProcessBatchAsync(CancellationToken.None);

        store.Verify(s => s.RemoveAsync("txn-final", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotRemoveTransaction_WhenStatusIsStillPending()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTransaction("txn-5")
            });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.Setup(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessResult("Processing"));

        using var worker = CreateWorker(store.Object, direct.Object);

        await worker.ProcessBatchAsync(CancellationToken.None);

        store.Verify(s => s.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotRemoveTransaction_WhenStatusIsUnknown()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTransaction("txn-6")
            });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.Setup(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessResult("Pending"));

        using var worker = CreateWorker(store.Object, direct.Object);

        await worker.ProcessBatchAsync(CancellationToken.None);

        store.Verify(s => s.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStopGracefully_WhenCancellationRequestedBeforeLoop()
    {
        var store = new Mock<IPendingTransactionsStore>(MockBehavior.Strict);
        var direct = new Mock<IDirectReceiveMoney>(MockBehavior.Strict);
        using var worker = CreateWorker(store.Object, direct.Object);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await worker.ProcessBatchAsync(cts.Token);

        store.VerifyNoOtherCalls();
        direct.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldStopGracefully_WhenCancellationRequestedMidLoop()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTransaction("txn-7"),
                CreateTransaction("txn-8")
            });

        using var cts = new CancellationTokenSource();

        var direct = new Mock<IDirectReceiveMoney>();
        direct.SetupSequence(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await cts.CancelAsync();
                return SuccessResult("Paid");
            })
            .ReturnsAsync(SuccessResult("Paid"));

        using var worker = CreateWorker(store.Object, direct.Object);

        await worker.ProcessBatchAsync(cts.Token);

        direct.Verify(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotRemoveRemainingTransactions_WhenCancelledMidBatch()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                CreateTransaction("txn-9"),
                CreateTransaction("txn-10")
            });

        using var cts = new CancellationTokenSource();

        var direct = new Mock<IDirectReceiveMoney>();
        direct.SetupSequence(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await cts.CancelAsync();
                return SuccessResult("Paid");
            })
            .ReturnsAsync(SuccessResult("Paid"));

        using var worker = CreateWorker(store.Object, direct.Object);

        await worker.ProcessBatchAsync(cts.Token);

        store.Verify(s => s.RemoveAsync("txn-9", It.IsAny<CancellationToken>()), Times.Once);
        store.Verify(s => s.RemoveAsync("txn-10", It.IsAny<CancellationToken>()), Times.Never);
    }

    private static PendingTransactionsWorker CreateWorker(
        IPendingTransactionsStore store,
        IDirectReceiveMoney directReceiveMoney,
        PendingTransactionsWorkerOptions? workerOptions = null,
        ILogger<PendingTransactionsWorker>? logger = null)
    {
        return new PendingTransactionsWorker(
            store,
            directReceiveMoney,
            Microsoft.Extensions.Options.Options.Create(workerOptions ?? new PendingTransactionsWorkerOptions
            {
                CallbackGracePeriod = TimeSpan.FromMinutes(1),
                PollInterval = TimeSpan.Zero
            }),
            logger ?? NullLogger<PendingTransactionsWorker>.Instance);
    }

    private static PendingTransaction CreateTransaction(string? transactionId, DateTimeOffset? createdAt = null)
        => new("client-ref", transactionId, createdAt ?? DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10));

    private static OperationResult<TransactionStatusResult> SuccessResult(string status) =>
        OperationResult<TransactionStatusResult>.Success(
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



