using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Scynett.Hubtel.Payments.Tests.Stubs;

internal static class InitiateReceiveMoneyStub
{
    public const string DefaultTransactionId = "hubtelTxn001";
    public const string DefaultClientReference = "clientref001";

    public static void Register(WireMockServer server, string posSalesId)
    {
        server.Given(Request.Create()
                .WithPath($"/merchantaccount/merchants/{posSalesId}/receive/mobilemoney")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    ResponseCode = "0001",
                    Message = "Pending",
                    Data = new
                    {
                        TransactionId = DefaultTransactionId,
                        ClientReference = DefaultClientReference,
                        Description = "Awaiting approval",
                        Amount = 0.8m,
                        Charges = 0.02m,
                        AmountAfterCharges = 0.78m,
                        AmountCharged = 0.8m,
                        DeliveryFee = 0m,
                        ExternalTransactionId = (string?)null,
                        OrderId = (string?)null
                    }
                }));
    }
}
