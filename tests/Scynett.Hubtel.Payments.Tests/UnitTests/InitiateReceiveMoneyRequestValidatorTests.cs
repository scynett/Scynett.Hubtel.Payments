using FluentValidation.TestHelper;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;


namespace Scynett.Hubtel.Payments.Tests.UnitTests;


public class InitiateReceiveMoneyRequestValidatorTests
{
    private readonly InitiateReceiveMoneyRequestValidator _sut;

    public InitiateReceiveMoneyRequestValidatorTests()
    {
        _sut = new InitiateReceiveMoneyRequestValidator();
    }

    [Fact]
    public void Validate_ShouldFail_WhenAmountIsZero()
    {
        //Arrange
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest()
            with
        { Amount = 0 };

        //Act
        var result = _sut.TestValidate(request);
        
        //Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }
}
