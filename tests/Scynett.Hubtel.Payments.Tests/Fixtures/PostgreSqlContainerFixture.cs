using System.Diagnostics.CodeAnalysis;

using Testcontainers.PostgreSql;

using Xunit;

namespace Scynett.Hubtel.Payments.Tests.Fixtures;

[SuppressMessage("Design", "CA1515", Justification = "xUnit fixtures must be public.")]
public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public PostgreSqlContainerFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .WithDatabase("hubtel_tests")
            .WithUsername("hubtel")
            .WithPassword("hubtel")
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync() =>
        await _container.StartAsync().ConfigureAwait(false);

    public async Task DisposeAsync() =>
        await _container.DisposeAsync().ConfigureAwait(false);
}

[SuppressMessage("Design", "CA1515", Justification = "xUnit collections referenced across test classes must be public.")]
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "xUnit naming convention.")]
[CollectionDefinition(CollectionName)]
public sealed class PostgreSqlContainerCollectionDefinition : ICollectionFixture<PostgreSqlContainerFixture>
{
    public const string CollectionName = "PostgreSqlContainer";
}
