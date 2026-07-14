using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Scynett.Hubtel.Payments.Application.Common;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

/// <summary>
/// Maps the inbound Hubtel Direct Receive Money callback endpoint.
/// </summary>
/// <remarks>
/// <para>
/// <b>Behaviour change (SDK hardening):</b> this endpoint used to answer <c>400 Bad Request</c> for every
/// unsuccessful outcome, including transient processing failures and the
/// <c>Hubtel.Callback.InFlight</c> conflict. A 4xx tells Hubtel "delivered, do not retry", so a database
/// blip or a concurrent delivery permanently dropped the callback. Status codes are now:
/// </para>
/// <list type="bullet">
///   <item><description><c>200 OK</c> — callback processed (this says nothing about whether the payment succeeded).</description></item>
///   <item><description><c>400 Bad Request</c> — malformed payload / failed validation. Genuinely not retryable.</description></item>
///   <item><description><c>401 Unauthorized</c> — shared-secret or source-IP check failed.</description></item>
///   <item><description><c>409 Conflict</c> — the same callback is already in flight; Hubtel may safely retry.</description></item>
///   <item><description><c>500 Internal Server Error</c> — transient processing failure. Hubtel should retry.</description></item>
/// </list>
/// <para>
/// The response body shape (<c>{ error, message }</c>) is unchanged.
/// </para>
/// </remarks>
internal static class HubtelReceiveMoneyCallbackEndpoint
{
    internal static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            RouteConstants.ReceiveMoneyCallback,
            async (
                HttpContext context,
                ReceiveMoneyCallbackRequest payload,
                IDirectReceiveMoney directReceiveMoney,
                CancellationToken ct) =>
            {
                var callbackValidator = context.RequestServices.GetService<ICallbackValidator>();
                var validationResult = callbackValidator is null
                    ? CallbackValidationResult.Success
                    : await callbackValidator.ValidateAsync(context, ct).ConfigureAwait(false);

                if (!validationResult.IsValid)
                {
                    // Rejected at the door: the caller failed the shared-secret / source-IP check.
                    // This is not something Hubtel should retry - it is a configuration or spoofing issue.
                    var validationCode = validationResult.ErrorCode ?? "Hubtel.Callback.Validation";

                    return Results.Json(
                        new
                        {
                            error = validationCode,
                            message = validationResult.ErrorMessage ?? "Callback validation failed."
                        },
                        statusCode: StatusCodes.Status401Unauthorized);
                }

                var result =
                    await directReceiveMoney
                        .HandleCallbackAsync(payload, ct)
                        .ConfigureAwait(false);

                // 200 OK means "callback processed", not "payment succeeded".
                if (result.IsSuccess)
                {
                    var callbackResult = result.Value;
                    var handlers = context.RequestServices.GetServices<IReceiveMoneyCallbackHandler>();
                    foreach (var handler in handlers)
                    {
                        await handler
                            .OnCompletedAsync(callbackResult, ct)
                            .ConfigureAwait(false);
                    }

                    return Results.Ok();
                }

                // No dependency on Scynett.Common.Domain here:
                var code = result.Error?.Code ?? "Hubtel.Callback.Error";
                var message = result.Error?.Description ?? "Callback processing failed.";

                return Results.Json(
                    new { error = code, message },
                    statusCode: ResolveFailureStatusCode(result.Error));
            })
            .AllowAnonymous()
            .WithName("HubtelDirectReceiveMoneyCallback")
            .WithTags("Hubtel", "DirectReceiveMoney");
    }

    /// <summary>
    /// Chooses the HTTP status for a failed callback. The rule that matters: anything that might succeed
    /// on a second delivery must NOT be a 4xx, because Hubtel stops retrying once it sees one.
    /// </summary>
    private static int ResolveFailureStatusCode(Error? error) => error?.Type switch
    {
        // Malformed / invalid payload. Retrying will not help.
        ErrorType.Validation => StatusCodes.Status400BadRequest,

        // Same callback already being processed (Hubtel.Callback.InFlight). Ask Hubtel to come back.
        ErrorType.Conflict => StatusCodes.Status409Conflict,

        // Everything else (Problem / Failure / NotFound / unknown) is treated as transient: we failed to
        // process a callback we should have processed, so Hubtel must retry.
        _ => StatusCodes.Status500InternalServerError,
    };
}
