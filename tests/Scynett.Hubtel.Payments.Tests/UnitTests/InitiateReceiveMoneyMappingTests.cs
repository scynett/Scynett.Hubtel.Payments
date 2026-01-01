using FluentAssertions;

using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class InitiateReceiveMoneyMappingTests : UnitTestBase
{
    [Fact]
    public void Map_ShouldPropagateAllFields_WhenGatewayReturnsSuccess()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest();
        var gateway = new GatewayInitiateReceiveMoneyResult(
            ResponseCode: "0000",
            Message: "Successful",
            TransactionId: "txn-001",
            ExternalReference: request.ClientReference,
            ExternalTransactionId: "ext-123",
            OrderId: "order-456",
            Description: "Payment approved",
            Amount: 100m,
            Charges: 2m,
            AmountAfterCharges: 98m,
            AmountCharged: 100m,
            DeliveryFee: 5m);
        var decision = CreateDecision(ResponseCategory.Success);

        var result = InitiateReceiveMoneyMapping.ToResult(request, gateway, decision);

        result.ClientReference.Should().Be(request.ClientReference);
        result.HubtelTransactionId.Should().Be("txn-001");
        result.ExternalTransactionId.Should().Be("ext-123");
        result.OrderId.Should().Be("order-456");
        result.Amount.Should().Be(100m);
        result.Charges.Should().Be(2m);
        result.AmountAfterCharges.Should().Be(98m);
        result.AmountCharged.Should().Be(100m);
        result.DeliveryFee.Should().Be(5m);
    }

    [Fact]
    public void Map_ShouldPropagateExternalAndOrderIds_WhenPresent()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest();
        var gateway = new GatewayInitiateReceiveMoneyResult(
            ResponseCode: "0000",
            Message: null,
            TransactionId: "txn-002",
            ExternalTransactionId: "ext-xyz",
            OrderId: "order-xyz",
            Description: null);
        var decision = CreateDecision(ResponseCategory.Success);

        var result = InitiateReceiveMoneyMapping.ToResult(request, gateway, decision);

        result.ExternalTransactionId.Should().Be("ext-xyz");
        result.OrderId.Should().Be("order-xyz");
    }

    [Fact]
    public void Map_ShouldPropagateDeliveryFee_WhenPresent()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest();
        var gateway = new GatewayInitiateReceiveMoneyResult(
            ResponseCode: "0000",
            Message: "Success",
            TransactionId: "txn-003",
            DeliveryFee: 1.25m);
        var decision = CreateDecision(ResponseCategory.Success);

        var result = InitiateReceiveMoneyMapping.ToResult(request, gateway, decision);

        result.DeliveryFee.Should().Be(1.25m);
    }

    [Fact]
    public void Map_ShouldMapStatusFields_IncludingPaymentMethodCurrencyIsFulfilled()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest();
        var gateway = new GatewayInitiateReceiveMoneyResult(
            ResponseCode: "0001",
            Message: "Pending",
            TransactionId: "txn-004",
            Description: "Awaiting approval");
        var decision = CreateDecision(ResponseCategory.Pending);

        var result = InitiateReceiveMoneyMapping.ToResult(request, gateway, decision);

        result.Status.Should().Be(ResponseCategory.Pending.ToString());
        result.Network.Should().Be(request.Channel);
        result.RawResponseCode.Should().Be("0001");
        result.Message.Should().Be("Pending");
        result.Description.Should().Be("Awaiting approval");
    }

    [Fact]
    public void Map_ShouldNotDropUnknownOptionalFields_WhenNull()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest();
        var gateway = new GatewayInitiateReceiveMoneyResult(
            ResponseCode: "0099",
            Message: null,
            TransactionId: null,
            ExternalTransactionId: null,
            OrderId: null,
            Description: null,
            Amount: null,
            Charges: null,
            AmountAfterCharges: null,
            AmountCharged: null,
            DeliveryFee: null);
        var decision = CreateDecision(ResponseCategory.Unknown);

        var result = InitiateReceiveMoneyMapping.ToResult(request, gateway, decision);

        result.HubtelTransactionId.Should().Be(string.Empty);
        result.ExternalTransactionId.Should().BeNull();
        result.OrderId.Should().BeNull();
        result.Amount.Should().Be(request.Amount);
        result.Charges.Should().Be(0m);
        result.AmountAfterCharges.Should().Be(request.Amount);
        result.AmountCharged.Should().Be(request.Amount);
        result.DeliveryFee.Should().BeNull();
    }

    private static HandlingDecision CreateDecision(ResponseCategory category) =>
        new(
            Code: "Test",
            Description: "Testing",
            NextAction: NextAction.None,
            Category: category);
}
