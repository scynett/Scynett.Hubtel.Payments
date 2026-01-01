using System;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.DirectReceiveMoney;
using Scynett.Hubtel.Payments.Tests.Fixtures;
using Scynett.Hubtel.Payments.Tests.Stubs;

namespace Scynett.Hubtel.Payments.Tests.IntegrationTests;

[Collection("IntegrationTests")]
public sealed class ReceiveMobileMoneyTests
{
    private readonly IntegrationFixture _fixture;

    public ReceiveMobileMoneyTests(IntegrationFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Initiate_ShouldReturnPendingStatus_WhenHubtelAcceptsRequest()
    {
        using var scope = _fixture.Services.CreateScope();
        var direct = scope.ServiceProvider.GetRequiredService<IDirectReceiveMoney>();
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            ClientReference = InitiateReceiveMoneyStub.DefaultClientReference,
            PrimaryCallbackEndPoint = _fixture.DefaultCallbackUrl.ToString()
        };

        var result = await direct.InitiateAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.IsFailure ? result.Error.ToString() : string.Empty);
        result.Value!.HubtelTransactionId.Should().Be(InitiateReceiveMoneyStub.DefaultTransactionId);
        result.Value.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task CheckStatus_ShouldReturnPaid_WhenHubtelReportsPaid()
    {
        using var scope = _fixture.Services.CreateScope();
        var direct = scope.ServiceProvider.GetRequiredService<IDirectReceiveMoney>();

        var statusResult = await direct.CheckStatusAsync(
            new TransactionStatusQuery(ClientReference: TransactionStatusStub.DefaultClientReference),
            CancellationToken.None);

        statusResult.IsSuccess.Should().BeTrue(statusResult.IsFailure ? statusResult.Error.ToString() : string.Empty);
        statusResult.Value!.Status.Should().Be("Paid");
        statusResult.Value.TransactionId.Should().Be(TransactionStatusStub.DefaultTransactionId);
    }

    [Fact]
    public async Task Initiate_ShouldSendPrimaryCallbackUrl()
    {
        using var scope = _fixture.Services.CreateScope();
        var direct = scope.ServiceProvider.GetRequiredService<IDirectReceiveMoney>();
        var callback = _fixture.DefaultCallbackUrl.ToString();
        var request = InitiateReceiveMoneyRequestBuilder.ValidRequest() with
        {
            PrimaryCallbackEndPoint = callback
        };

        await direct.InitiateAsync(request, CancellationToken.None);

        var entry = _fixture.HubtelMock.LogEntries
            .Last(e => e.RequestMessage.Path.Contains("/receive/mobilemoney", StringComparison.OrdinalIgnoreCase));

        entry.RequestMessage.Body.Should().Contain($"\"PrimaryCallbackUrl\":\"{callback}\"")
            .And.NotContain("PrimaryCallbackEndpoint");
    }
}
