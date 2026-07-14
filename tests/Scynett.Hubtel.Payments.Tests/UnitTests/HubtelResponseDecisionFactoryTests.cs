using FluentAssertions;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

/// <summary>
/// The "customer paid, app says failed" guard rail: a response we cannot interpret must never be
/// reported as a final failure.
/// </summary>
public sealed class HubtelResponseDecisionFactoryTests : UnitTestBase
{
    [Theory]
    [InlineData("9999")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_ShouldNotBeFinal_WhenResponseCodeIsUnknown(string? code)
    {
        var decision = HubtelResponseDecisionFactory.Create(code, "something we have never seen");

        decision.Category.Should().Be(ResponseCategory.Unknown);
        decision.IsSuccess.Should().BeFalse();
        decision.IsFinal.Should().BeFalse("an unrecognised code means 'we do not know yet', not 'failed'");
        decision.ShouldRetry.Should().BeTrue();
        decision.NextAction.Should().Be(NextAction.RetryLater);
    }

    [Fact]
    public void Create_ShouldBeTransientAndNotFinal_WhenCodeIsHttpError()
    {
        var decision = HubtelResponseDecisionFactory.Create(
            HubtelResponseDecisionFactory.HttpErrorCode,
            "HTTP 503: upstream unavailable");

        decision.Category.Should().Be(ResponseCategory.TransientError);
        decision.IsSuccess.Should().BeFalse();
        decision.IsFinal.Should().BeFalse("a transport failure says nothing about whether the customer was debited");
        decision.ShouldRetry.Should().BeTrue();
        decision.NextAction.Should().Be(NextAction.RetryLater);
    }

    [Fact]
    public void Create_ShouldNeverBeFinalAndRetryableAtTheSameTime_ForUnknownOrTransport()
    {
        foreach (var code in new[] { "9999", HubtelResponseDecisionFactory.HttpErrorCode })
        {
            var decision = HubtelResponseDecisionFactory.Create(code);

            (decision.IsFinal && decision.ShouldRetry)
                .Should().BeFalse("'final' and 'retry' are contradictory for code {0}", code);
        }
    }

    [Fact]
    public void Create_ShouldStillMapSuccess()
    {
        var decision = HubtelResponseDecisionFactory.Create("0000", "The transaction has been processed successfully.");

        decision.IsSuccess.Should().BeTrue();
        decision.IsFinal.Should().BeTrue();
        decision.Category.Should().Be(ResponseCategory.Success);
    }

    [Fact]
    public void Create_ShouldStillMapPending()
    {
        var decision = HubtelResponseDecisionFactory.Create("0001", "Request accepted");

        decision.IsFinal.Should().BeFalse();
        decision.Category.Should().Be(ResponseCategory.Pending);
        decision.NextAction.Should().Be(NextAction.WaitForCallback);
    }

    [Fact]
    public void Create_ShouldStillMapFinalCustomerFailure()
    {
        var decision = HubtelResponseDecisionFactory.Create("2001", "Insufficient funds");

        decision.IsFinal.Should().BeTrue();
        decision.IsSuccess.Should().BeFalse();
        decision.Category.Should().Be(ResponseCategory.CustomerError);
    }
}
