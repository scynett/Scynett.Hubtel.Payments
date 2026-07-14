using FluentAssertions;

using Microsoft.AspNetCore.Http;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Tests.Testing.TestBases;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

/// <summary>
/// The status code the callback endpoint returns is not cosmetic — it is an instruction to Hubtel.
/// A 4xx means "delivered, stop retrying"; a 5xx means "try again". So the only question that matters
/// for each failure is: could this have succeeded on a second delivery? If yes, it must not be a 4xx,
/// or we throw away the only notification we were ever going to get about somebody's money.
/// </summary>
public sealed class CallbackFailureStatusCodeTests : UnitTestBase
{
    /// <summary>
    /// A malformed payload is the one case redelivery cannot fix. It is the only 4xx.
    /// </summary>
    [Fact]
    public void ResolveFailureStatusCode_ShouldReturn400_OnlyForValidation()
    {
        var status = HubtelReceiveMoneyCallbackEndpoint.ResolveFailureStatusCode(
            new Error("Hubtel.Callback.Invalid", "Malformed payload.", ErrorType.Validation));

        status.Should().Be(StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// An in-flight duplicate must NOT be acknowledged. This was a 409, which is a 4xx, which told
    /// Hubtel to stop retrying a callback we had not finished processing — so if the in-flight
    /// delivery then died, that callback was gone and nobody was coming back for it. The duplicate a
    /// 503 provokes is absorbed by the same dedupe check that produced the conflict, so being wrong
    /// here costs one wasted callback rather than a lost payment.
    /// </summary>
    [Fact]
    public void ResolveFailureStatusCode_ShouldAskHubtelToRetry_WhenCallbackIsAlreadyInFlight()
    {
        var status = HubtelReceiveMoneyCallbackEndpoint.ResolveFailureStatusCode(
            new Error("Hubtel.Callback.InFlight", "Already being processed.", ErrorType.Conflict));

        status.Should().Be(StatusCodes.Status503ServiceUnavailable);
        status.Should().BeGreaterThanOrEqualTo(500, "a 4xx would tell Hubtel to stop retrying");
    }

    /// <summary>
    /// Everything else is a failure to process a callback we should have processed. Hubtel must retry.
    /// </summary>
    [Theory]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.Problem)]
    [InlineData(ErrorType.NotFound)]
    public void ResolveFailureStatusCode_ShouldReturn5xx_ForEverythingRetryable(ErrorType errorType)
    {
        var status = HubtelReceiveMoneyCallbackEndpoint.ResolveFailureStatusCode(
            new Error("Hubtel.Callback.Failed", "Could not process.", errorType));

        status.Should().BeGreaterThanOrEqualTo(500, "Hubtel must be asked to try again");
    }

    /// <summary>
    /// The 401 does NOT come from here. A caller that fails the shared-secret / source-IP check is
    /// turned away by the validator short-circuit at the top of the endpoint, before the callback is
    /// ever processed — so this resolver never sees an authorization failure. Anything that does reach
    /// it and is not recognised falls to a retryable 5xx, which is the right way to be wrong: an
    /// unrecognised failure might succeed on redelivery, and guessing 4xx would throw the callback away.
    /// </summary>
    [Fact]
    public void ResolveFailureStatusCode_ShouldFallBackToRetryable_ForAnythingUnrecognised()
    {
        var status = HubtelReceiveMoneyCallbackEndpoint.ResolveFailureStatusCode(
            new Error("Hubtel.Callback.Unexpected", "Something new.", ErrorType.Authorization));

        status.Should().BeGreaterThanOrEqualTo(500, "an unrecognised failure must not be acknowledged");
    }

    /// <summary>
    /// A null error is still a failure. It must not be acknowledged either.
    /// </summary>
    [Fact]
    public void ResolveFailureStatusCode_ShouldReturn5xx_WhenErrorIsNull()
    {
        HubtelReceiveMoneyCallbackEndpoint.ResolveFailureStatusCode(null)
            .Should().BeGreaterThanOrEqualTo(500);
    }
}
