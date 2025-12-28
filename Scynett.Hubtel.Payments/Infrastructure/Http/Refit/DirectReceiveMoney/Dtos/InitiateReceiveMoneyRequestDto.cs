namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

internal record InitiateReceiveMoneyRequestDto(string CustomerName,
    string CustomerMsisdn,
    string CustomerEmail,
    string Channel,
    string Amount,
    string PrimaryCallbackEndpoint,
    string Description,
    string ClientReference);
