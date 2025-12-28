using System.Globalization;

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
            CustomerName: request.CustomerName ?? string.Empty,
            PosSalesId: posSalesId,
            CustomerMsisdn: request.CustomerMobileNumber,
            CustomeeEmail: string.Empty,
            Channel: request.Channel,
            Amount: request.Amount.ToString("F2", CultureInfo.InvariantCulture),
            CallbackUrl: callbackUrl,
            Description: request.Description,
            ClientReference: request.ClientReference);
    }

    internal static InitiateReceiveMoneyResult ToResult(
        InitiateReceiveMoneyRequest request,
        GatewayInitiateReceiveMoneyResult gateway,
        HandlingDecision decision)
    {
        var status = decision.Category.ToString(); // e.g. Success / Pending / CustomerError / ValidationError

        return new InitiateReceiveMoneyResult(
            ClientReference: request.ClientReference,
            HubtelTransactionId: gateway.TransactionId ?? string.Empty,
            Status: status,
            Amount: gateway.Amount ?? request.Amount,
            Charges: gateway.Charges ?? 0m,
            AmountAfterCharges: gateway.AmountAfterCharges ?? request.Amount,
            AmountCharged: gateway.AmountCharged ?? request.Amount,
            Network: request.Channel,
            RawResponseCode: gateway.ResponseCode,
            Message: gateway.Message,
            Description: gateway.Description,
            DeliveryFee: gateway.DeliveryFee);
    }
}