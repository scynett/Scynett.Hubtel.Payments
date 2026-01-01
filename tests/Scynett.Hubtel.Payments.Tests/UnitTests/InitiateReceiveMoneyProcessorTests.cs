using FluentAssertions;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.Storage;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class InitiateReceiveMoneyProcessorTests : UnitTestBase
{
    [Fact]
    public async Task ProcessAsync_ShouldUsePosSalesId_WhenConfigured()
    {
        const string configuredPos = "POS-1001";
        var gateway = new Mock<IHubtelReceiveMoneyGateway>();
        gateway.Setup(x => x.InitiateAsync(
                It.Is<GatewayInitiateReceiveMoneyRequest>(r => r.PosSalesId == configuredPos),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateGatewayResult());

        var processor = CreateProcessor(
            gateway,
            directOptions: new DirectReceiveMoneyOptions { PosSalesId = configuredPos },
            hubtelOptions: new HubtelOptions { MerchantAccountNumber = "fallback" });

        var result = await processor.ExecuteAsync(
            InitiateReceiveMoneyRequestBuilder.ValidRequest(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        gateway.VerifyAll();
    }

    [Fact]
    public async Task ProcessAsync_ShouldFallbackToMerchantAccountNumber_WhenPosSalesIdIsEmpty()
    {
        const string merchantAccount = "MERCHANT-2002";
        var gateway = new Mock<IHubtelReceiveMoneyGateway>();
        gateway.Setup(x => x.InitiateAsync(
                It.Is<GatewayInitiateReceiveMoneyRequest>(r => r.PosSalesId == merchantAccount),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateGatewayResult());

        var processor = CreateProcessor(
            gateway,
            directOptions: new DirectReceiveMoneyOptions { PosSalesId = string.Empty },
            hubtelOptions: new HubtelOptions { MerchantAccountNumber = merchantAccount });

        var result = await processor.ExecuteAsync(
            InitiateReceiveMoneyRequestBuilder.ValidRequest(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        gateway.VerifyAll();
    }

    [Fact]
    public async Task ProcessAsync_ShouldFail_WhenPosSalesIdAndMerchantAccountNumberAreBothEmpty()
    {
        var gateway = new Mock<IHubtelReceiveMoneyGateway>(MockBehavior.Strict);

        var processor = CreateProcessor(
            gateway,
            directOptions: new DirectReceiveMoneyOptions { PosSalesId = null },
            hubtelOptions: new HubtelOptions { MerchantAccountNumber = string.Empty });

        var result = await processor.ExecuteAsync(
            InitiateReceiveMoneyRequestBuilder.ValidRequest(),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DirectReceiveMoney.MissingPosSalesId");
        gateway.Verify(
            x => x.InitiateAsync(It.IsAny<GatewayInitiateReceiveMoneyRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ShouldStorePending_WhenDecisionWaitsForCallback()
    {
        var gateway = new Mock<IHubtelReceiveMoneyGateway>();
        var pendingStore = new Mock<IPendingTransactionsStore>();
        gateway.Setup(x => x.InitiateAsync(It.IsAny<GatewayInitiateReceiveMoneyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GatewayInitiateReceiveMoneyResult(
                ResponseCode: "0001",
                Message: "Pending",
                TransactionId: "txn-200",
                ExternalReference: "client-200"));

        var processor = CreateProcessor(
            gateway,
            new DirectReceiveMoneyOptions { PosSalesId = "POS-200" },
            new HubtelOptions { MerchantAccountNumber = "merchant" },
            pendingStore);

        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest();
        var result = await processor.ExecuteAsync(
            request,
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pendingStore.Verify(
            x => x.AddAsync("txn-200", request.ClientReference, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnFailure_OnGatewayApiException()
    {
        var gateway = new Mock<IHubtelReceiveMoneyGateway>();
        gateway.Setup(x => x.InitiateAsync(It.IsAny<GatewayInitiateReceiveMoneyRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var processor = CreateProcessor(
            gateway,
            new DirectReceiveMoneyOptions { PosSalesId = "POS-300" },
            new HubtelOptions { MerchantAccountNumber = "merchant" });

        var result = await processor.ExecuteAsync(
            InitiateReceiveMoneyRequestBuilder.ValidRequest(),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DirectReceiveMoney.UnhandledException");
    }

    [Fact]
    public async Task ProcessAsync_ShouldSurfaceGatewayFailure_WhenDecisionIsFinalFailure()
    {
        var gateway = new Mock<IHubtelReceiveMoneyGateway>();
        var pendingStore = new Mock<IPendingTransactionsStore>();
        gateway.Setup(x => x.InitiateAsync(It.IsAny<GatewayInitiateReceiveMoneyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GatewayInitiateReceiveMoneyResult(
                ResponseCode: "4000",
                Message: "Validation error",
                TransactionId: null));

        var processor = CreateProcessor(
            gateway,
            new DirectReceiveMoneyOptions { PosSalesId = "POS-400" },
            new HubtelOptions { MerchantAccountNumber = "merchant" },
            pendingStore);

        var result = await processor.ExecuteAsync(
            InitiateReceiveMoneyRequestBuilder.ValidRequest(),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DirectReceiveMoney.ValidationError");
        pendingStore.Verify(
            x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static InitiateReceiveMoneyProcessor CreateProcessor(
        Mock<IHubtelReceiveMoneyGateway> gateway,
        DirectReceiveMoneyOptions directOptions,
        HubtelOptions hubtelOptions,
        Mock<IPendingTransactionsStore>? pendingStore = null,
        IValidator<InitiateReceiveMoneyRequest>? validator = null,
        ILogger<InitiateReceiveMoneyProcessor>? logger = null)
    {
        pendingStore ??= new Mock<IPendingTransactionsStore>();

        if (validator is null)
        {
            var validatorMock = new Mock<IValidator<InitiateReceiveMoneyRequest>>();
            validatorMock.Setup(v => v.ValidateAsync(
                    It.IsAny<InitiateReceiveMoneyRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            validator = validatorMock.Object;
        }

        logger ??= new Mock<ILogger<InitiateReceiveMoneyProcessor>>().Object;

        return new InitiateReceiveMoneyProcessor(
            gateway.Object,
            pendingStore.Object,
            Microsoft.Extensions.Options.Options.Create(hubtelOptions),
            Microsoft.Extensions.Options.Options.Create(directOptions),
            validator,
            logger);
    }

    private static GatewayInitiateReceiveMoneyResult CreateGatewayResult() =>
        new(
            ResponseCode: "0000",
            Message: "Successful",
            TransactionId: "txn-123");
}


