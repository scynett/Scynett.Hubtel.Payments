using System.Globalization;

using FluentValidation.TestHelper;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

#pragma warning disable xUnit1000 // Test classes must be public
internal class InitiateReceiveMoneyRequestValidatorTests : UnitTestBase
#pragma warning restore xUnit1000 // Test classes must be public
{
    private readonly InitiateReceiveMoneyRequestValidator _sut = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ShouldFail_WhenMsisdnIsNullOrEmpty(string? msisdn)
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            CustomerMobileNumber = msisdn!
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CustomerMobileNumber);
    }

    [Theory]
    [InlineData("0241234567")]
    [InlineData("23324123456")]
    [InlineData("2332412345678")]
    [InlineData("133241234567")]
    [InlineData("23324A234567")]
    public void Validate_ShouldFail_WhenMsisdnIsMalformed(string msisdn)
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            CustomerMobileNumber = msisdn
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CustomerMobileNumber);
    }

    [Fact]
    public void Validate_ShouldFail_WhenMsisdnContainsUnicodeDigits()
    {
        var unicodeDigits = "\u0661\u0662\u0663\u0664\u0665\u0666\u0667\u0668\u0669";
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            CustomerMobileNumber = $"233{unicodeDigits}"
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CustomerMobileNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ShouldFail_WhenChannelIsMissing(string? channel)
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            Channel = channel!
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Channel);
    }

    [Fact]
    public void Validate_ShouldFail_WhenChannelIsInvalid()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            Channel = "orange-gh"
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Channel);
    }

    [Fact]
    public void Validate_ShouldFail_WhenAmountIsNegative()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            Amount = -0.01m
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_ShouldFail_WhenAmountIsZero()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            Amount = 0m
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_ShouldFail_WhenAmountUsesCommaDecimalSeparator()
    {
        var commaCulture = CultureInfo.GetCultureInfo("fr-FR");
        var parsedAmount = decimal.Parse("0,123", commaCulture);
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            Amount = parsedAmount
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_ShouldFail_WhenAmountContainsUnicodeSeparatorsOrSpaces()
    {
        var numberFormat = (NumberFormatInfo)CultureInfo.GetCultureInfo("fr-FR").NumberFormat.Clone();
        numberFormat.NumberGroupSeparator = "\u202F";
        var parsedAmount = decimal.Parse("1\u202F234,567", NumberStyles.Number, numberFormat);
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            Amount = parsedAmount
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ShouldFail_WhenCallbackUrlIsMissing(string? callbackValue)
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            PrimaryCallbackEndPoint = callbackValue!
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PrimaryCallbackEndPoint);
    }

    [Theory]
    [InlineData("/relative/path")]
    [InlineData("ftp://example.com/callback")]
    [InlineData("http://")]
    public void Validate_ShouldFail_WhenCallbackUrlIsInvalidOrNotAbsolute(string callbackValue)
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            PrimaryCallbackEndPoint = callbackValue
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PrimaryCallbackEndPoint);
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        var longDescription = new string('a', 501);
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            Description = longDescription
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_ShouldFail_WhenClientReferenceExceedsMaxLength()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            ClientReference = new string('a', 37)
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ClientReference);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ShouldPass_WhenEmailIsNullOrEmpty(string? email)
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            CustomerEmail = email
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.CustomerEmail);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    public void Validate_ShouldFail_WhenEmailIsPresentButInvalid(string email)
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            CustomerEmail = email
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CustomerEmail);
    }

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValidInvariantFormat()
    {
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest();

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}


