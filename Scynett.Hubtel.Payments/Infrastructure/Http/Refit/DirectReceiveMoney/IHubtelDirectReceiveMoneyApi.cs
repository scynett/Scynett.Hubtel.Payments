using Refit;

using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;


namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;

/// <summary>
/// Refit API for Hubtel Direct Receive Money.
/// </summary>
internal interface IHubtelDirectReceiveMoneyApi
{
    /// <summary>
    /// Initiate a Mobile Money debit (Direct Receive Money).
    /// POST /merchantaccount/merchants/{POS_Sales_ID}/receive/mobilemoney
    /// </summary>
    [Post("/merchantaccount/merchants/{posSalesId}/receive/mobilemoney")]
    Task<ApiResponse<InitiateReceiveMoneyResponseDto>> InitiateAsync(
        string posSalesId,
        [Body] InitiateReceiveMoneyRequestDto request,
        CancellationToken cancellationToken);
}


