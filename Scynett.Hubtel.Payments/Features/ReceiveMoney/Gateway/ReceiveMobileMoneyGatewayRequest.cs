namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

public sealed record ReceiveMobileMoneyGatewayRequest(
    string CustomerName,
    string CustomerMsisdn,
    string CustomerEmail,
    string Channel,
    string Amount,
    string PrimaryCallbackEndpoint,
    string Description,
    string ClientReference);