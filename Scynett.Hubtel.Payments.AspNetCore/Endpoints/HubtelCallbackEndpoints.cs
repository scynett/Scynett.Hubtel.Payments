using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;

namespace Scynett.Hubtel.Payments.AspNetCore.Endpoints;

public static class HubtelCallbackEndpoints
{
    public static IEndpointRouteBuilder MapHubtelCallbacks(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/hubtel/callback")
    {
        endpoints.MapPost(pattern, HandleCallback)
            .WithName("HubtelPaymentCallback");

        return endpoints;
    }

    private static async Task<IResult> HandleCallback(
        CallbackRequest request,
        IReceiveMoneyService receiveMoneyService,
        ILogger<IReceiveMoneyService> logger)
    {
        logger.LogInformation(
            "Received Hubtel callback for transaction {TransactionId}",
            request.Data?.TransactionId);

        if (request.Data == null)
        {
            logger.LogWarning("Received callback with null data");
            return Results.BadRequest(new { error = "Invalid callback data" });
        }

        var command = new PaymentCallback(
            request.ResponseCode ?? string.Empty,
            request.Data.Status ?? string.Empty,
            request.Data.TransactionId ?? string.Empty,
            request.Data.ClientReference ?? string.Empty,
            request.Data.Description ?? string.Empty,
            request.Data.ExternalTransactionId ?? string.Empty,
            request.Data.Amount ?? 0,
            request.Data.Charges ?? 0,
            request.Data.CustomerMobileNumber ?? string.Empty);

        var result = await receiveMoneyService.ProcessCallbackAsync(command).ConfigureAwait(false);

        if (result.IsFailure)
        {
            logger.LogError("Failed to process callback: {Error}", result.Error.Message);
            return Results.Ok(new { status = "received", message = "Callback logged but processing failed" });
        }

        return Results.Ok(new { status = "success", message = "Callback processed successfully" });
    }

    private sealed record CallbackRequest(string? ResponseCode, CallbackData? Data);
    private sealed record CallbackData(
        string? Status,
        string? TransactionId,
        string? ClientReference,
        string? Description,
        string? ExternalTransactionId,
        decimal? Amount,
        decimal? Charges,
        string? CustomerMobileNumber);
}
