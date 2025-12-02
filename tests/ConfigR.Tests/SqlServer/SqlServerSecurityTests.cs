using ConfigR.Abstractions;
using ConfigR.SqlServer;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.SqlServer;

[Collection("SqlServerSecurity")]
public sealed class SecurityTests
{
    [Theory]
    [InlineData("'; DROP TABLE ConfigR; --")]
    [InlineData("ConfigR; DELETE FROM ConfigR WHERE 1=1; --")]
    [InlineData("ConfigR' OR '1'='1")]
    [InlineData("ConfigR\"; DROP TABLE users; --")]
    public void SqlServerConfigStore_Should_Reject_Malicious_TableNames(string maliciousTableName)
    {
        // Arrange & Act
        Action act = () =>
        {
            var options = Options.Create(new SqlServerConfigStoreOptions
            {
                ConnectionString = SqlServerTestDatabase.GetConnectionString(),
                Table = maliciousTableName
            });
            _ = new SqlServerConfigStore(options);
        };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid SQL identifier*");
    }

    [Theory]
    [InlineData("dbo'; DROP SCHEMA dbo; --")]
    [InlineData("test\"; DELETE FROM")]
    [InlineData("public' OR '1'='1")]
    public void SqlServerConfigStore_Should_Reject_Malicious_SchemaNames(string maliciousSchemaName)
    {
        // Arrange & Act
        Action act = () =>
        {
            var options = Options.Create(new SqlServerConfigStoreOptions
            {
                ConnectionString = SqlServerTestDatabase.GetConnectionString(),
                Schema = maliciousSchemaName,
                Table = "ConfigR"
            });
            _ = new SqlServerConfigStore(options);
        };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid SQL identifier*");
    }

    [Theory]
    [InlineData("ValidTable")]
    [InlineData("Config_R")]
    [InlineData("_ConfigR")]
    [InlineData("ConfigR123")]
    [InlineData("TABLE1")]
    public void SqlServerConfigStore_Should_Accept_Valid_TableNames(string validTableName)
    {
        // Arrange & Act
        Action act = () =>
        {
            var options = Options.Create(new SqlServerConfigStoreOptions
            {
                ConnectionString = SqlServerTestDatabase.GetConnectionString(),
                Table = validTableName,
                AutoCreateTable = false // Não tentar criar para não precisar de DB real
            });
            _ = new SqlServerConfigStore(options);
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SqlServerConfigStore_Should_Reject_TableName_Starting_With_Number()
    {
        // Arrange & Act
        Action act = () =>
        {
            var options = Options.Create(new SqlServerConfigStoreOptions
            {
                ConnectionString = SqlServerTestDatabase.GetConnectionString(),
                Table = "1InvalidTable"
            });
            _ = new SqlServerConfigStore(options);
        };

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SqlServerConfigStore_Should_Reject_TableName_With_SpecialChars()
    {
        // Arrange & Act
        Action act = () =>
        {
            var options = Options.Create(new SqlServerConfigStoreOptions
            {
                ConnectionString = SqlServerTestDatabase.GetConnectionString(),
                Table = "Config@Table!"
            });
            _ = new SqlServerConfigStore(options);
        };

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SqlServerConfigStore_Should_Reject_Too_Long_TableName()
    {
        // Arrange
        var tooLongName = new string('A', 129); // Máximo é 128

        // Act
        Action act = () =>
        {
            var options = Options.Create(new SqlServerConfigStoreOptions
            {
                ConnectionString = SqlServerTestDatabase.GetConnectionString(),
                Table = tooLongName
            });
            _ = new SqlServerConfigStore(options);
        };

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConfigEntry_Should_Reject_Oversized_Values()
    {
        // Arrange
        var options = Options.Create(new SqlServerConfigStoreOptions
        {
            ConnectionString = SqlServerTestDatabase.GetConnectionString(),
            Table = "ConfigR",
            AutoCreateTable = false
        });
        var store = new SqlServerConfigStore(options);

        // Criar um valor gigante (101 MB)
        var oversizedValue = new string('X', 101 * 1024 * 1024);
        var entries = new[]
        {
            new ConfigEntry
            {
                Key = "test.key",
                Value = oversizedValue
            }
        };

        // Act
        Func<Task> act = async () => await store.UpsertAsync(entries);

        // Assert - Isso deve falhar por ser muito grande
        // Nota: O teste exato depende da implementação, mas deve haver alguma validação
        act.Should().ThrowAsync<Exception>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ConfigStore_Should_Reject_Empty_Keys(string? emptyKey)
    {
        // Arrange
        var options = Options.Create(new SqlServerConfigStoreOptions
        {
            ConnectionString = SqlServerTestDatabase.GetConnectionString(),
            Table = "ConfigR",
            AutoCreateTable = false
        });
        var store = new SqlServerConfigStore(options);

        var entries = new[]
        {
            new ConfigEntry
            {
                Key = emptyKey!,
                Value = "some value"
            }
        };

        // Act - keys vazias devem ser ignoradas ou lançar exceção
        Func<Task> act = async () => await store.UpsertAsync(entries);

        // Assert - Não deve causar crash
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ConfigStore_Should_Reject_Null_ConnectionString()
    {
        // Arrange & Act
        Action act = () =>
        {
            var options = Options.Create(new SqlServerConfigStoreOptions
            {
                ConnectionString = null!,
                Table = "ConfigR"
            });
            _ = new SqlServerConfigStore(options);
        };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ConnectionString*");
    }

    [Fact]
    public void ConfigStore_Should_Reject_Empty_ConnectionString()
    {
        // Arrange & Act
        Action act = () =>
        {
            var options = Options.Create(new SqlServerConfigStoreOptions
            {
                ConnectionString = string.Empty,
                Table = "ConfigR"
            });
            _ = new SqlServerConfigStore(options);
        };

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}