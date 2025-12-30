using Scynett.Hubtel.Payments.Public.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.Public;


/// <summary>
/// Root entry point for all Hubtel payment capabilities.
/// </summary>
internal interface IHubtelPayments
{
    /// <summary>
    /// Direct Mobile Money receive operations (MoMo Debit).
    /// </summary>
    IDirectReceiveMoney DirectReceiveMoney { get; }
}