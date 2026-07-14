using FluentAssertions;

using FluentValidation.TestHelper;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class InitiateReceiveMoneyCallbackRequestValidatorTests : UnitTestBase
{
    private readonly ReceiveMoneyCallbackRequestValidator _sut = new();

    [Fact]
    public void Validate_ShouldFail_WhenTransactionIdIsMissing()
    {
        var request = CreateRequest(CreateValidData() with { TransactionId = string.Empty });

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Data!.TransactionId);
    }

    [Fact]
    public void Validate_ShouldFail_WhenClientReferenceIsMissing()
    {
        var request = CreateRequest(CreateValidData() with { ClientReference = string.Empty });

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Data!.ClientReference);
    }

    // Hubtel sends Amount: 0 on FAILURE callbacks. Rejecting those callbacks left the pending
    // transaction unresolved and told Hubtel (via a 4xx) not to retry. Zero must validate.
    [Fact]
    public void Validate_ShouldPass_WhenAmountIsZero_OnFailureCallback()
    {
        var request = new ReceiveMoneyCallbackRequest(
            "2001",
            "Transaction Failed",
            CreateValidData() with { Amount = 0m, Charges = 0m, AmountAfterCharges = 0m, AmountCharged = 0m });

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Data!.Amount);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenAmountIsNegative()
    {
        var request = CreateRequest(CreateValidData() with { Amount = -5m });

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Data!.Amount);
    }

    [Fact]
    public void Validate_ShouldPass_WhenPayloadIsValid()
    {
        var result = _sut.TestValidate(CreateRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPayloadIsNull()
    {
        var request = new ReceiveMoneyCallbackRequest("0000", "Success", null!);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Data);
    }

    [Fact]
    public void Validate_ShouldReturnAllErrors_WhenMultipleFieldsAreInvalid()
    {
        var data = CreateValidData() with
        {
            ClientReference = string.Empty,
            TransactionId = string.Empty,
            Amount = -1m
        };
        var result = _sut.TestValidate(CreateRequest(data));

        result.ShouldHaveValidationErrorFor(x => x.Data!.ClientReference);
        result.ShouldHaveValidationErrorFor(x => x.Data!.TransactionId);
        result.ShouldHaveValidationErrorFor(x => x.Data!.Amount);
        result.Errors.Should().HaveCount(3);
    }

    private static ReceiveMoneyCallbackRequest CreateRequest(ReceiveMoneyCallbackData? data = null) =>
        new("0000", "Success", data ?? CreateValidData());

    private static ReceiveMoneyCallbackData CreateValidData() =>
        new(
            Amount: 10m,
            Charges: 0.5m,
            AmountAfterCharges: 9.5m,
            AmountCharged: 10m,
            Description: "Payment for order 123",
            ClientReference: "ABC123XYZ",
            TransactionId: "TXN123456",
            ExternalTransactionId: "EXT987654",
            OrderId: "ORDER-111",
            PaymentDate: DateTimeOffset.UtcNow);
}
