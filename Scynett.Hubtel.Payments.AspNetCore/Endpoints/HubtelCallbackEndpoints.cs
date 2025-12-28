using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Application.Abstractions;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;

using System.Text.Json;

namespace Scynett.Hubtel.Payments.AspNetCore.Endpoints;

public static class HubtelCallbackEndpoints
{
    public static IEndpointRouteBuilder MapHubtelCallbacks(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/hubtel/callback")
    {
        endpoints.MapPost(pattern, HandleCallback)
            .WithName("HubtelCallback")
            .WithTags("Hubtel")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> HandleCallback(
        CallbackRequest request,
        IReceiveMoneyProcessor processor,
        ILogger<IReceiveMoneyProcessor> logger)
    {
        Log.ReceivedCallback(logger, request.Data?.TransactionId);

        if (request.Data == null)
        {
            Log.ReceivedCallbackWithNullData(logger);
            return Results.BadRequest(new { error = "Invalid callback data" });
        }

        var command = new PaymentCallback(
            request.Data.ResponseCode ?? string.Empty,
            request.Data.Status ?? string.Empty,
            request.Data.TransactionId ?? string.Empty,
            request.Data.ClientReference ?? string.Empty,
            request.Data.Description ?? string.Empty,
            request.Data.ExternalTransactionId ?? string.Empty,
            request.Data.Amount ?? 0,
            request.Data.Charges ?? 0,
            request.Data.CustomerMsisdn ?? string.Empty);

        var result = await processor.ProcessCallbackAsync(command).ConfigureAwait(false);

        if (result.IsFailure)
        {
            Log.CallbackProcessingFailed(logger, result.Error.Message);
            return Results.BadRequest(new { error = result.Error.Message });
        }

        Log.CallbackProcessedSuccessfully(logger, request.Data.TransactionId);
        return Results.Ok(new { success = true });
    }

    private sealed record CallbackRequest(CallbackData? Data);
    private sealed record CallbackData(
        string? ResponseCode,
        string? Data,
        string? Status,
        string? TransactionId,
        string? ClientReference,
        string? Description,
        string? ExternalTransactionId,
        decimal? Amount,
        decimal? Charges,
        string? CustomerMsisdn);
}
