using Microsoft.Extensions.DependencyInjection;

using System;
using System.Diagnostics.CodeAnalysis;

namespace Scynett.Hubtel.Payments.Tests.Testing;

internal sealed class IntegrationFixture : IDisposable
{
    public IServiceProvider Services { get; }

    public IntegrationFixture()
    {
        var services = new ServiceCollection();

        // services.AddHubtelPayments(...);

        Services = services.BuildServiceProvider(validateScopes: true);
    }

    public void Dispose()
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

[CollectionDefinition("IntegrationTests")]
[SuppressMessage("Design", "CA1515", Justification = "xUnit collection definitions must be public.")]
public sealed class IntegrationTestsDefinition : ICollectionFixture<IntegrationFixture>
{
}
