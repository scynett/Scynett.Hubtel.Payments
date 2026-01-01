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
