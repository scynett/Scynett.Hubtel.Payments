using FluentAssertions;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class ReceiveMoneyCallbackMappingTests : UnitTestBase
{
    [Fact]
    public void ToResult_ShouldIncludeFinancialFields()
    {
        var request = new ReceiveMoneyCallbackRequest(
            ResponseCode: "0000",
            Message: "Success",
            new ReceiveMoneyCallbackData(
                Amount: 100m,
                Charges: 2m,
                AmountAfterCharges: 98m,
                AmountCharged: 100m,
                Description: "Payment complete",
                ClientReference: "client-1",
                TransactionId: "txn-1",
                ExternalTransactionId: "ext-1",
                OrderId: "order-1",
                PaymentDate: DateTimeOffset.UtcNow));
        var decision = new HandlingDecision(
            Code: "0000",
            Description: "Success",
            NextAction: NextAction.None,
            Category: ResponseCategory.Success,
            IsSuccess: true,
            IsFinal: true);

        var result = ReceiveMoneyCallbackMapping.ToResult(request, decision);

        result.Amount.Should().Be(100m);
        result.Charges.Should().Be(2m);
        result.AmountAfterCharges.Should().Be(98m);
        result.AmountCharged.Should().Be(100m);
        result.Description.Should().Be("Payment complete");
        result.ExternalTransactionId.Should().Be("ext-1");
        result.OrderId.Should().Be("order-1");
        result.PaymentDate.Should().BeCloseTo(request.Data.PaymentDate!.Value, TimeSpan.FromSeconds(1));
    }
}
