using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

using Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class CallbackValidatorTests : UnitTestBase
{
    private static readonly string[] AllowedRange = ["10.0.0.0/24"];

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenSecretMismatch()
    {
        var options = OptionsFactory.Create(new CallbackValidationOptions
        {
            EnableValidation = true,
            SharedSecret = "expected"
        });
        var validator = new CallbackValidator(options, Mock.Of<ILogger<CallbackValidator>>());
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Hubtel-Callback-Secret"] = "wrong";

        var result = await validator.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("Hubtel.Callback.InvalidSignature");
    }

    // The compare is now constant-time (CryptographicOperations.FixedTimeEquals over UTF-8 bytes).
    // Behaviour must be unchanged: right secret in, wrong secret out.
    [Fact]
    public async Task ValidateAsync_ShouldPass_WhenSecretMatchesExactly()
    {
        const string secret = "not-a-real-secret-0123456789";
        var options = OptionsFactory.Create(new CallbackValidationOptions
        {
            EnableValidation = true,
            SharedSecret = secret
        });
        var validator = new CallbackValidator(options, Mock.Of<ILogger<CallbackValidator>>());
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Hubtel-Callback-Secret"] = secret;

        var result = await validator.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]                                // missing
    [InlineData("not-a-real-secret-012345678")]     // one char short
    [InlineData("not-a-real-secret-0123456789x")]   // one char long
    [InlineData("Not-A-Real-Secret-0123456789")]    // different case
    [InlineData("xot-a-real-secret-0123456789")]    // differs on first byte
    [InlineData("not-a-real-secret-012345678x")]    // differs on last byte
    public async Task ValidateAsync_ShouldFail_WhenSecretIsWrong(string presented)
    {
        var options = OptionsFactory.Create(new CallbackValidationOptions
        {
            EnableValidation = true,
            SharedSecret = "not-a-real-secret-0123456789"
        });
        var validator = new CallbackValidator(options, Mock.Of<ILogger<CallbackValidator>>());
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Hubtel-Callback-Secret"] = presented;

        var result = await validator.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("Hubtel.Callback.InvalidSignature");
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenSecretHeaderIsAbsent()
    {
        var options = OptionsFactory.Create(new CallbackValidationOptions
        {
            EnableValidation = true,
            SharedSecret = "not-a-real-secret-0123456789"
        });
        var validator = new CallbackValidator(options, Mock.Of<ILogger<CallbackValidator>>());

        var result = await validator.ValidateAsync(new DefaultHttpContext(), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("Hubtel.Callback.InvalidSignature");
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenIpNotAllowed()
    {
        var options = OptionsFactory.Create(new CallbackValidationOptions
        {
            EnableValidation = true,
            AllowedCidrs = AllowedRange
        });
        var validator = new CallbackValidator(options, Mock.Of<ILogger<CallbackValidator>>());
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.5");

        var result = await validator.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("Hubtel.Callback.InvalidSource");
    }

    [Fact]
    public async Task ValidateAsync_ShouldPass_WhenDisabled()
    {
        var options = OptionsFactory.Create(new CallbackValidationOptions
        {
            EnableValidation = false
        });
        var validator = new CallbackValidator(options, Mock.Of<ILogger<CallbackValidator>>());
        var context = new DefaultHttpContext();

        var result = await validator.ValidateAsync(context, CancellationToken.None);

        result.IsValid.Should().BeTrue();
    }
}
