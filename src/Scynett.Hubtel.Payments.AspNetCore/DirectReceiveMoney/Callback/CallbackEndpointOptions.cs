namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

public sealed class CallbackEndpointOptions
{
    public string Route { get; init; } =
        RouteConstants.ReceiveMoneyCallback;
}