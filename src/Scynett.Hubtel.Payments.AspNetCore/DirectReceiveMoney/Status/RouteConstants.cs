namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Status;

/// <summary>
/// Contains constant values for API routes used within the application.
/// </summary>
internal static class RouteConstants
{
    /// <summary>
    /// The endpoint for receiving transaction status updates from Hubtel's 
    /// Direct Receive Money  service.
    /// </summary>
    public const string TransactionStatus = "/hubtel/direct-receive-money/status";
}