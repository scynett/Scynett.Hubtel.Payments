namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

public sealed record InitReceiveMoneyCommand(
    string CustomerName,
    string CustomerMobileNumber,
    string Channel,
    decimal Amount,
    string Description,
    string? ClientReference = null,
    string? PrimaryCallbackUrl = null);
