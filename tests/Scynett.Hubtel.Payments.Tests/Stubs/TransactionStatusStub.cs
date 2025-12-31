using System;

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Scynett.Hubtel.Payments.Tests.Stubs;

internal static class TransactionStatusStub
{
    public const string DefaultClientReference = InitiateReceiveMoneyStub.DefaultClientReference;
    public const string DefaultTransactionId = InitiateReceiveMoneyStub.DefaultTransactionId;

    public static void Register(WireMockServer server, string posSalesId)
    {
        var baseResponse = Response.Create()
            .WithStatusCode(200)
            .WithBodyAsJson(BuildPayload());

        server.Given(Request.Create()
                .UsingGet()
                .WithPath($"/transactions/{posSalesId}/status")
                .WithParam("clientReference", DefaultClientReference))
            .RespondWith(baseResponse);

        server.Given(Request.Create()
                .UsingGet()
                .WithPath($"/transactions/{posSalesId}/status")
                .WithParam("hubtelTransactionId", DefaultTransactionId))
            .RespondWith(baseResponse);
    }

    private static object BuildPayload() => new
    {
        Message = "Successful",
        ResponseCode = "0000",
        Data = new
        {
            Date = DateTimeOffset.UtcNow,
            Status = "Paid",
            TransactionId = DefaultTransactionId,
            ExternalTransactionId = "ext-transaction-1",
            PaymentMethod = "mobilemoney",
            ClientReference = DefaultClientReference,
            CurrencyCode = "GHS",
            Amount = 0.8m,
            Charges = 0.02m,
            AmountAfterCharges = 0.78m,
            IsFulfilled = true
        }
    };
}
