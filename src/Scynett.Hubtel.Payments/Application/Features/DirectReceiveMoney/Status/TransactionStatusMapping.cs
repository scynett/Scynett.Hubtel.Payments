using Scynett.Hubtel.Payments.Application.Abstractions.Gateways;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

internal static class TransactionStatusMapping
{
    internal static TransactionStatusResult ToResult(
        GatewayTransactionStatusResult gateway,
        HandlingDecision decision)
    {
        return new TransactionStatusResult(
            ClientReference: gateway.ClientReference,
            Status: gateway.Status,
            Amount: gateway.Amount,
            Charges: gateway.Charges,
            AmountAfterCharges: gateway.AmountAfterCharges,
            TransactionId: gateway.HubtelTransactionId, 
            ExternalTransactionId: gateway.ExternalTransactionId,
            PaymentMethod: gateway.PaymentMethod,
            CurrencyCode: gateway.CurrencyCode,
            IsFulfilled: gateway.IsFulfilled,
            Date: gateway.PaymentDate,
            RawResponseCode: decision.Code,
            RawMessage: decision.CustomerMessage);
    }
}
