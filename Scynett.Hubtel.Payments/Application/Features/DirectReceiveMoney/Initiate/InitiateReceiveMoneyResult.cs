namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

public sealed record InitiateReceiveMoneyResult(
    string ClientReference,
    string HubtelTransactionId,
    string Status,              // Pending / Success / Failed
    decimal Amount,
    string Network,
    string RawResponseCode
   
);
