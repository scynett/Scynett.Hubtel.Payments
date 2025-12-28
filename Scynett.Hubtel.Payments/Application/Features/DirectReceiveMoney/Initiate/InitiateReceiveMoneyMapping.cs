using Scynett.Hubtel.Payments.Application.Abstractions.Gateways.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

internal static class InitiateReceiveMoneyMapping
{
    internal static GatewayInitiateReceiveMoneyRequest ToGatewayRequest(
        InitiateReceiveMoneyRequest request,
        string posSalesId,
        string callbackUrl)
    {
        return new GatewayInitiateReceiveMoneyRequest(
            PosSalesId: posSalesId,
            CustomerMsisdn: request.CustomerMobileNumber,
            Channel: request.Channel,
            Amount: request.Amount,
            CallbackUrl: callbackUrl,
            Description: request.Description,
            ClientReference: request.ClientReference);
    }

    internal static InitiateReceiveMoneyResult ToResult(
        InitiateReceiveMoneyRequest request,
        GatewayInitiateReceiveMoneyResult gateway,
        HandlingDecision decision)
    {
        // status should come from decision/category, not hard-coded.
        // Keep it explicit and predictable for SDK users.
        var status = decision.Category.ToString(); // e.g. Success / Pending / CustomerError / ValidationError

        return new InitiateReceiveMoneyResult(
            ClientReference: request.ClientReference,
            HubtelTransactionId: gateway.TransactionId ?? string.Empty,
            Status: status,
            Amount: request.Amount,
            Network: request.Channel,
            RawResponseCode: gateway.ResponseCode);
    }
}