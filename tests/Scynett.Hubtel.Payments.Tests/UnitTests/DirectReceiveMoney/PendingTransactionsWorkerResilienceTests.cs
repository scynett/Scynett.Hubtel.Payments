using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
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

namespace Scynett.Hubtel.Payments.Tests.UnitTests.DirectReceiveMoney;

[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Unit tests run without a synchronization context.")]
public sealed class PendingTransactionsWorkerResilienceTests : UnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_ShouldContinue_WhenCheckStatusReturnsFailureResult()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PendingTransaction("client", "txn-1", DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10)),
                new PendingTransaction("client", "txn-2", DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10))
            });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.SetupSequence(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<TransactionStatusResult>.Failure(Error.Failure("code", "failure")))
            .ReturnsAsync(OperationResult<TransactionStatusResult>.Success(new TransactionStatusResult(
                Status: "Paid",
                ClientReference: "client",
                TransactionId: "txn-2",
                ExternalTransactionId: null,
                PaymentMethod: null,
                CurrencyCode: null,
                IsFulfilled: null,
                Amount: null,
                Charges: null,
                AmountAfterCharges: null,
                Date: null,
                RawResponseCode: "0000",
                RawMessage: "ok")));

        using var worker = PendingTransactionsWorkerTestsHelper.CreateWorker(store.Object, direct.Object);

        await worker.ProcessBatchAsync(CancellationToken.None);

        store.Verify(s => s.RemoveAsync("txn-2", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldContinue_WhenCheckStatusThrowsException()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PendingTransaction("client", "txn-3", DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10)),
                new PendingTransaction("client", "txn-4", DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10))
            });

        var direct = new Mock<IDirectReceiveMoney>();
        direct.SetupSequence(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"))
            .ReturnsAsync(PendingTransactionsWorkerTestsHelper.SuccessResult("Paid"));

        var logger = new TestLogger<PendingTransactionsWorker>();
        using var worker = PendingTransactionsWorkerTestsHelper.CreateWorker(store.Object, direct.Object, logger: logger);

        await worker.ProcessBatchAsync(CancellationToken.None);

        store.Verify(s => s.RemoveAsync("txn-4", It.IsAny<CancellationToken>()), Times.Once);
        logger.Entries.Should().Contain(entry =>
            entry.LogLevel == LogLevel.Error &&
            entry.Message.Contains("Error processing pending transaction", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotCrash_WhenStoreThrowsDuringGetAll()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("store failure"));

        var direct = new Mock<IDirectReceiveMoney>(MockBehavior.Strict);
        var logger = new TestLogger<PendingTransactionsWorker>();

        using var worker = PendingTransactionsWorkerTestsHelper.CreateWorker(store.Object, direct.Object, logger: logger);

        await worker.ProcessBatchAsync(CancellationToken.None);

        logger.Entries.Should().Contain(entry =>
            entry.Message.Contains("PendingTransactionsWorker loop error", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotCrash_WhenStoreThrowsDuringRemove()
    {
        var store = new Mock<IPendingTransactionsStore>();
        store.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PendingTransaction("client", "txn-5", DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10))
            });
        store.Setup(s => s.RemoveAsync("txn-5", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("remove failure"));

        var direct = new Mock<IDirectReceiveMoney>();
        direct.Setup(x => x.CheckStatusAsync(It.IsAny<TransactionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PendingTransactionsWorkerTestsHelper.SuccessResult("Paid"));

        var logger = new TestLogger<PendingTransactionsWorker>();
        using var worker = PendingTransactionsWorkerTestsHelper.CreateWorker(store.Object, direct.Object, logger: logger);

        await worker.ProcessBatchAsync(CancellationToken.None);

        logger.Entries.Should().Contain(entry =>
            entry.Message.Contains("Error processing pending transaction", StringComparison.OrdinalIgnoreCase));
    }

    private static class PendingTransactionsWorkerTestsHelper
    {
        public static PendingTransactionsWorker CreateWorker(
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
                logger ?? new TestLogger<PendingTransactionsWorker>());

        public static OperationResult<TransactionStatusResult> SuccessResult(string status) =>
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
}



