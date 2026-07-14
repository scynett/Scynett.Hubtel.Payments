using System.Net;
using System.Net.Http;

using FluentAssertions;

using Moq;

using Refit;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Infrastructure.Gateways;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

/// <summary>
/// Refit's <see cref="ApiResponse{T}"/> does not throw on non-2xx. The gateway must notice that itself,
/// keep the HTTP status, and never pretend the payment definitively failed.
/// </summary>
public sealed class HubtelReceiveMoneyGatewayTests : UnitTestBase
{
    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, 500)]
    [InlineData(HttpStatusCode.BadGateway, 502)]
    [InlineData(HttpStatusCode.ServiceUnavailable, 503)]
    [InlineData(HttpStatusCode.Unauthorized, 401)]
    [InlineData(HttpStatusCode.BadRequest, 400)]
    public async Task InitiateAsync_ShouldSurfaceHttpStatus_WhenHubtelReturnsNonSuccess(
        HttpStatusCode status,
        int expected)
    {
        var api = new Mock<IHubtelDirectReceiveMoneyApi>();
        api.Setup(x => x.InitiateAsync(
                It.IsAny<string>(),
                It.IsAny<InitiateReceiveMoneyRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => CreateResponse(status, content: null));

        var sut = new HubtelReceiveMoneyGateway(api.Object);

        var result = await sut.InitiateAsync(CreateRequest(), CancellationToken.None);

        result.ResponseCode.Should().Be(HubtelResponseDecisionFactory.HttpErrorCode);
        result.HttpStatusCode.Should().Be(expected, "the HTTP status must never be lost");
        result.Message.Should().Contain(expected.ToString(System.Globalization.CultureInfo.InvariantCulture));
        result.TransactionId.Should().BeNull();
    }

    [Fact]
    public async Task InitiateAsync_ShouldProduceNonFinalDecision_WhenHubtelReturns500()
    {
        var api = new Mock<IHubtelDirectReceiveMoneyApi>();
        api.Setup(x => x.InitiateAsync(
                It.IsAny<string>(),
                It.IsAny<InitiateReceiveMoneyRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => CreateResponse(HttpStatusCode.InternalServerError, content: null));

        var sut = new HubtelReceiveMoneyGateway(api.Object);

        var result = await sut.InitiateAsync(CreateRequest(), CancellationToken.None);
        var decision = HubtelResponseDecisionFactory.Create(result.ResponseCode, result.Message);

        decision.IsFinal.Should().BeFalse();
        decision.ShouldRetry.Should().BeTrue();
        decision.Category.Should().Be(ResponseCategory.TransientError);
    }

    [Fact]
    public async Task InitiateAsync_ShouldNotThrow_WhenBodyIsEmptyOn200()
    {
        var api = new Mock<IHubtelDirectReceiveMoneyApi>();
        api.Setup(x => x.InitiateAsync(
                It.IsAny<string>(),
                It.IsAny<InitiateReceiveMoneyRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => CreateResponse(HttpStatusCode.OK, content: null));

        var sut = new HubtelReceiveMoneyGateway(api.Object);

        var result = await sut.InitiateAsync(CreateRequest(), CancellationToken.None);

        result.ResponseCode.Should().Be(HubtelResponseDecisionFactory.HttpErrorCode);
        result.HttpStatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InitiateAsync_ShouldMapTransportFailure_ToHttpErrorWithoutStatus()
    {
        var api = new Mock<IHubtelDirectReceiveMoneyApi>();
        api.Setup(x => x.InitiateAsync(
                It.IsAny<string>(),
                It.IsAny<InitiateReceiveMoneyRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("connection reset"));

        var sut = new HubtelReceiveMoneyGateway(api.Object);

        var result = await sut.InitiateAsync(CreateRequest(), CancellationToken.None);

        result.ResponseCode.Should().Be(HubtelResponseDecisionFactory.HttpErrorCode);
        result.HttpStatusCode.Should().BeNull("no response was ever received");
        HubtelResponseDecisionFactory.Create(result.ResponseCode).IsFinal.Should().BeFalse();
    }

    [Fact]
    public async Task InitiateAsync_ShouldReturnHubtelPayload_OnSuccess()
    {
        var payload = new InitiateReceiveMoneyResponseDto(
            ResponseCode: "0001",
            Message: "Request accepted",
            Data: new HubtelReceiveMoneyData(
                TransactionId: "txn-1",
                ClientReference: "ref-1",
                Description: "desc",
                Amount: 1m,
                Charges: 0m,
                AmountAfterCharges: 1m,
                AmountCharged: 1m,
                DeliveryFee: 0m,
                ExternalTransactionId: "ext-1",
                OrderId: "order-1"));

        var api = new Mock<IHubtelDirectReceiveMoneyApi>();
        api.Setup(x => x.InitiateAsync(
                It.IsAny<string>(),
                It.IsAny<InitiateReceiveMoneyRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => CreateResponse(HttpStatusCode.OK, payload));

        var sut = new HubtelReceiveMoneyGateway(api.Object);

        var result = await sut.InitiateAsync(CreateRequest(), CancellationToken.None);

        result.ResponseCode.Should().Be("0001");
        result.TransactionId.Should().Be("txn-1");
        result.HttpStatusCode.Should().BeNull("a parsed 2xx response carries no error status");
    }

    // Ownership of the HttpResponseMessage is transferred to the ApiResponse<T> we return: ApiResponse<T>
    // is IDisposable and disposes the underlying response, and the gateway under test consumes it with
    // `using var response = await api.InitiateAsync(...)`. CA2000 cannot see across that transfer.
#pragma warning disable CA2000 // Dispose objects before losing scope
    private static ApiResponse<InitiateReceiveMoneyResponseDto> CreateResponse(
        HttpStatusCode status,
        InitiateReceiveMoneyResponseDto? content)
    {
        var httpResponse = new HttpResponseMessage(status)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://hubtel.test/receive")
        };

        return new ApiResponse<InitiateReceiveMoneyResponseDto>(
            httpResponse,
            content,
            new RefitSettings());
    }
#pragma warning restore CA2000

    private static GatewayInitiateReceiveMoneyRequest CreateRequest() =>
        new(
            CustomerName: "Joe Doe",
            PosSalesId: "POS-TEST",
            CustomerMsisdn: "233200010000",
            CustomerEmail: "username@example.com",
            Channel: "mtn-gh",
            Amount: "1.00",
            CallbackUrl: "https://example.test/callback",
            Description: "Test",
            ClientReference: "ref1");
}
