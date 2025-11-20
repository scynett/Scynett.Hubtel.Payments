namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

public sealed record ReceiveMoneyCallbackCommand(
    string ResponseCode,
    string Status,
    string TransactionId,
    string ClientReference,
    string Description,
    string ExternalTransactionId,
    decimal Amount,
    decimal Charges,
    string CustomerMobileNumber);
