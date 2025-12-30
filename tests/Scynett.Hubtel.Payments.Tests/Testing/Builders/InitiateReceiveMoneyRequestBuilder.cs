using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;

namespace Scynett.Hubtel.Payments.Tests.Testing.Builders;

internal static class InitiateReceiveMoneyRequestBuilder
{
    public static InitiateReceiveMoneyRequest ValidRequest() =>
        new(
            CustomerName: "Joe Doe",
            CustomerEmail: "username@example.com",
            CustomerMobileNumber: "233200010000",
            Channel: "vodafone-gh",
            Amount: 0.8m,
            Description: "Union Dues",
            ClientReference: "3jL2KlUy3vt21",
            PrimaryCallbackEndPoint: "https://webhook.site/b503d1a9-e726-f315254a6ede"
        );
}
