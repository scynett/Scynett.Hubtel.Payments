using Refit;

using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney;

internal interface IHubtelTransactionStatusApi
{
    [Get("/transactions/{posSalesId}/status")]
    Task<TransactionStatusResponseDto> GetStatusAsync(
        [AliasAs("posSalesId")] string posSalesId,
        [Query] string? clientReference = null,
        [Query] string? hubtelTransactionId = null,
        [Query] string? networkTransactionId = null,
        CancellationToken ct = default);
}