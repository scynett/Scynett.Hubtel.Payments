namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

public record ReceiveMobileMoneyResponse(
     string Message,
     string ResponseCode,
     ResponseData? Data);
public record ResponseData(
     string ClientReference,
     string TransactionId,
     decimal Amount,
     decimal Charges,
     decimal AmountCharged);
