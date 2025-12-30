using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.Public.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

internal static class HubtelReceiveMoneyCallbackEndpoint
{
    internal static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            RouteConstants.ReceiveMoneyCallback,
            async (
                ReceiveMoneyCallbackRequest payload,
                IDirectReceiveMoney directReceiveMoney,
                CancellationToken ct) =>
            {
                var result =
                    await directReceiveMoney
                        .HandleCallbackAsync(payload, ct)
                        .ConfigureAwait(false);

                // 200 OK means "callback processed", not "payment succeeded".
                if (result.IsSuccess)
                    return Results.Ok();

                // No dependency on Scynett.Common.Domain here:
                var code = result.Error?.Code ?? "Hubtel.Callback.Error";
                var message = result.Error?.Description ?? "Callback processing failed.";

                return Results.BadRequest(new { error = code, message });
            })
            .AllowAnonymous()
            .WithName("HubtelDirectReceiveMoneyCallback")
            .WithTags("Hubtel", "DirectReceiveMoney");
    }
}