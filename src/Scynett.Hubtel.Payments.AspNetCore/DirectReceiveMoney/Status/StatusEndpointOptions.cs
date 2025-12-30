namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Status;

public sealed class StatusEndpointOptions
{
    public string Route { get; init; } =
        RouteConstants.TransactionStatus;
}