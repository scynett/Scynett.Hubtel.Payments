using FluentAssertions;

using FluentValidation;

using Microsoft.Extensions.Logging;

using Moq;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class ReceiveMoneyCallbackProcessorTests : UnitTestBase
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnExistingResult_WhenDuplicate()
    {
        var pendingStore = new Mock<IPendingTransactionsStore>();
        var auditStore = new Mock<ICallbackAuditStore>();
        var validator = new Mock<IValidator<ReceiveMoneyCallbackRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ReceiveMoneyCallbackRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        var existingResult = CreateResult();
        auditStore.Setup(x => x.TryStartAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallbackAuditStartResult(false, existingResult));

        var processor = new ReceiveMoneyCallbackProcessor(
            pendingStore.Object,
            auditStore.Object,
            validator.Object,
            Mock.Of<ILogger<ReceiveMoneyCallbackProcessor>>());

        var result = await processor.ExecuteAsync(CreateRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingResult);
        pendingStore.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistResultAndRemovePending_OnFinalDecision()
    {
        var pendingStore = new Mock<IPendingTransactionsStore>();
        var auditStore = new Mock<ICallbackAuditStore>();
        var validator = new Mock<IValidator<ReceiveMoneyCallbackRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ReceiveMoneyCallbackRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        auditStore.Setup(x => x.TryStartAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallbackAuditStartResult(true, null));

        var processor = new ReceiveMoneyCallbackProcessor(
            pendingStore.Object,
            auditStore.Object,
            validator.Object,
            Mock.Of<ILogger<ReceiveMoneyCallbackProcessor>>());

        var result = await processor.ExecuteAsync(CreateRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pendingStore.Verify(x => x.RemoveAsync("txn-100", It.IsAny<CancellationToken>()), Times.Once);
        auditStore.Verify(x => x.SaveResultAsync("txn-100", It.IsAny<ReceiveMoneyCallbackResult>(), true, "0000", It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenInFlightAndNoResult()
    {
        var pendingStore = new Mock<IPendingTransactionsStore>();
        var auditStore = new Mock<ICallbackAuditStore>();
        var validator = new Mock<IValidator<ReceiveMoneyCallbackRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ReceiveMoneyCallbackRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        auditStore.Setup(x => x.TryStartAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallbackAuditStartResult(false, null));

        var processor = new ReceiveMoneyCallbackProcessor(
            pendingStore.Object,
            auditStore.Object,
            validator.Object,
            Mock.Of<ILogger<ReceiveMoneyCallbackProcessor>>());

        var result = await processor.ExecuteAsync(CreateRequest(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hubtel.Callback.InFlight");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMarkFailure_WhenExceptionOccurs()
    {
        var pendingStore = new Mock<IPendingTransactionsStore>();
        pendingStore.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));
        var auditStore = new Mock<ICallbackAuditStore>();
        var validator = new Mock<IValidator<ReceiveMoneyCallbackRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ReceiveMoneyCallbackRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        auditStore.Setup(x => x.TryStartAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallbackAuditStartResult(true, null));

        var processor = new ReceiveMoneyCallbackProcessor(
            pendingStore.Object,
            auditStore.Object,
            validator.Object,
            Mock.Of<ILogger<ReceiveMoneyCallbackProcessor>>());

        var result = await processor.ExecuteAsync(CreateRequest(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hubtel.Callback.Exception");
        auditStore.Verify(x => x.MarkFailureAsync("txn-100", It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ReceiveMoneyCallbackRequest CreateRequest() =>
        new(
            ResponseCode: "0000",
            Message: "Success",
            new ReceiveMoneyCallbackData(
                Amount: 100m,
                Charges: 2m,
                AmountAfterCharges: 98m,
                AmountCharged: 100m,
                Description: "Payment complete",
                ClientReference: "client-100",
                TransactionId: "txn-100",
                ExternalTransactionId: "ext-1",
                OrderId: "order-1",
                PaymentDate: DateTimeOffset.UtcNow));

    private static ReceiveMoneyCallbackResult CreateResult() =>
        new(
            ClientReference: "client-100",
            TransactionId: "txn-100",
            ResponseCode: "0000",
            Category: ResponseCategory.Success,
            NextAction: NextAction.None,
            IsFinal: true,
            IsSuccess: true,
            CustomerMessage: "Done",
            RawMessage: "Success",
            Amount: 100m,
            Charges: 2m,
            AmountAfterCharges: 98m,
            AmountCharged: 100m,
            Description: "Payment complete",
            ExternalTransactionId: "ext-1",
            OrderId: "order-1",
            PaymentDate: DateTimeOffset.UtcNow);
}
