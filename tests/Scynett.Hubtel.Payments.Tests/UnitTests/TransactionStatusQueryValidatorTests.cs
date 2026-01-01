using FluentAssertions;

using FluentValidation.TestHelper;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class TransactionStatusQueryValidatorTests : UnitTestBase
{
    private readonly TransactionStatusQueryValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenNoIdentifierIsSupplied()
    {
        var query = new TransactionStatusQuery();

        var result = _validator.TestValidate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .Contain(e => e.ErrorMessage.Contains("At least one identifier", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldPass_WhenTransactionIdIsSupplied()
    {
        var query = new TransactionStatusQuery(HubtelTransactionId: "txn-123");

        var result = _validator.TestValidate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenClientReferenceIsSupplied()
    {
        var query = new TransactionStatusQuery(ClientReference: "client-456");

        var result = _validator.TestValidate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenTransactionIdExceedsMaxLength()
    {
        var query = new TransactionStatusQuery(HubtelTransactionId: new string('a', 65));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.HubtelTransactionId);
    }

    [Fact]
    public void Validate_ShouldFail_WhenClientReferenceExceedsMaxLength()
    {
        var query = new TransactionStatusQuery(ClientReference: new string('b', 37));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.ClientReference);
    }

    [Fact]
    public void Validate_ShouldFail_WhenBothIdentifiersExceedMaxLength()
    {
        var query = new TransactionStatusQuery(
            ClientReference: new string('c', 40),
            HubtelTransactionId: new string('d', 70));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.ClientReference);
        result.ShouldHaveValidationErrorFor(q => q.HubtelTransactionId);
    }
}
