using System.Text.Json;

using FluentAssertions;

using Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class InitiateReceiveMoneyRequestDtoTests
{
    [Fact]
    public void Serialize_ShouldEmitPrimaryCallbackUrl()
    {
        var dto = new InitiateReceiveMoneyRequestDto(
            CustomerName: "Test User",
            CustomerMsisdn: "233000000000",
            CustomerEmail: "test@example.com",
            Channel: "mtn-gh",
            Amount: "10.00",
            PrimaryCallbackUrl: "https://callback.example.com",
            Description: "desc",
            ClientReference: "ref-123");

        var json = JsonSerializer.Serialize(dto);

        json.Should().Contain("\"PrimaryCallbackUrl\":\"https://callback.example.com\"")
            .And.NotContain("PrimaryCallbackEndpoint");
    }
}
