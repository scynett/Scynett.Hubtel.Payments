using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.Infrastructure.Gateways;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class HubtelTransactionStatusGatewayTests : UnitTestBase
{
    [Fact]
    public async Task CheckStatusAsync_ShouldUsePosSalesId_WhenConfigured()
    {
        const string expectedPosId = "POS-123";

        var api = new Mock<IHubtelTransactionStatusApi>(MockBehavior.Strict);
        api.Setup(x => x.GetStatusAsync(
                expectedPosId,
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResponse());

        var gateway = CreateGateway(
            api,
            directOptions: new DirectReceiveMoneyOptions { PosSalesId = expectedPosId },
            hubtelOptions: new HubtelOptions { MerchantAccountNumber = "fallback" });

        var result = await gateway.CheckStatusAsync(
            new TransactionStatusQuery(ClientReference: "client-ref"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        api.VerifyAll();
    }

    [Fact]
    public async Task CheckStatusAsync_ShouldFallbackToMerchantAccountNumber_WhenPosSalesIdIsEmpty()
    {
        const string merchantAccount = "MERCHANT-789";

        var api = new Mock<IHubtelTransactionStatusApi>(MockBehavior.Strict);
        api.Setup(x => x.GetStatusAsync(
                merchantAccount,
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResponse());

        var gateway = CreateGateway(
            api,
            directOptions: new DirectReceiveMoneyOptions { PosSalesId = null },
            hubtelOptions: new HubtelOptions { MerchantAccountNumber = merchantAccount });

        var result = await gateway.CheckStatusAsync(
            new TransactionStatusQuery(HubtelTransactionId: "hubtel-id"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        api.VerifyAll();
    }

    [Fact]
    public async Task CheckStatusAsync_ShouldFail_WhenPosSalesIdAndMerchantAccountNumberAreBothEmpty()
    {
        var api = new Mock<IHubtelTransactionStatusApi>(MockBehavior.Strict);

        var gateway = CreateGateway(
            api,
            directOptions: new DirectReceiveMoneyOptions { PosSalesId = "" },
            hubtelOptions: new HubtelOptions { MerchantAccountNumber = "" });

        var result = await gateway.CheckStatusAsync(
            new TransactionStatusQuery(NetworkTransactionId: "network"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Config.PosSalesId");
        api.Verify(x => x.GetStatusAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static HubtelTransactionStatusGateway CreateGateway(
        Mock<IHubtelTransactionStatusApi> api,
        DirectReceiveMoneyOptions directOptions,
        HubtelOptions hubtelOptions)
        => new(
            api.Object,
            Microsoft.Extensions.Options.Options.Create(directOptions),
            Microsoft.Extensions.Options.Options.Create(hubtelOptions));

    private static TransactionStatusResponseDto CreateSuccessResponse()
        => new()
        {
            ResponseCode = "0000",
            Message = "Successful",
            Data = new TransactionStatusDataDto
            {
                Status = "Paid",
                ClientReference = "client"
            }
        };
}


