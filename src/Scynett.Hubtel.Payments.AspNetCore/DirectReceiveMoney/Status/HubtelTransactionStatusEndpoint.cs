using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Status;

internal static class HubtelTransactionStatusEndpoint
{
    internal static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            RouteConstants.TransactionStatus,
            async (
                string clientReference,
                IDirectReceiveMoney directReceiveMoney,
                CancellationToken ct) =>
            {
                var result =
                    await directReceiveMoney
                        .CheckStatusAsync(new TransactionStatusQuery(clientReference), ct)
                        .ConfigureAwait(false);

                // 200 OK means "status retrieved"
                if (result.IsSuccess)
                    return Results.Ok(result.Value);

                var code = result.Error?.Code ?? "Hubtel.Status.Error";
                var message = result.Error?.Description ?? "Unable to retrieve transaction status.";

                return Results.BadRequest(new { error = code, message });
            })
            .AllowAnonymous()
            .WithName("HubtelDirectReceiveMoneyStatus")
            .WithTags("Hubtel", "DirectReceiveMoney");
    }
}
