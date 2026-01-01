using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Storage.PostgreSql;

namespace Scynett.Hubtel.Payments.Tests.UnitTests;

public sealed class PostgreSqlStorageOptionsTests
{
    [Fact]
    public void DefaultOptions_ShouldHaveCorrectDefaults()
    {
        var options = new PostgreSqlStorageOptions();

        options.SchemaName.Should().Be("hubtel");
        options.TableName.Should().Be("pending_transactions");
        options.AutoCreateSchema.Should().BeTrue();
        options.CommandTimeoutSeconds.Should().Be(30);
        options.ConnectionString.Should().BeEmpty();
    }

    [Fact]
    public void FullTableName_ShouldCombineSchemaAndTable()
    {
        var options = new PostgreSqlStorageOptions
        {
            SchemaName = "custom_schema",
            TableName = "custom_table"
        };

        options.FullTableName.Should().Be("\"custom_schema\".\"custom_table\"");
    }

    [Fact]
    public void SectionName_ShouldBeCorrect()
    {
        PostgreSqlStorageOptions.SectionName.Should().Be("Hubtel:Storage:PostgreSql");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenConnectionStringIsEmpty()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new PostgreSqlStorageOptions
        {
            ConnectionString = string.Empty
        });

        var act = () => new PostgreSqlPendingTransactionsStore(options, NullLogger<PostgreSqlPendingTransactionsStore>.Instance);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*connection string*");
    }
}
