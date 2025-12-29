using Scynett.Hubtel.Payments.Application.Abstractions.Gateways;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

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
            HubtelTransactionId: gateway.HubtelTransactionId,
            ExternalTransactionId: gateway.ExternalTransactionId,
            PaymentMethod: gateway.PaymentMethod,
            PaymentDate: gateway.PaymentDate);
    }
}