using Microsoft.Data.SqlClient;
using System.Data;

namespace ConfigR.Tests.SqlServer;

public static class SqlServerTestDatabase
{
    public static string GetConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("CONFIGR_TEST_SQL_CONN");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        // Default for local docker:
        return "Server=localhost,1433;Database=ConfigR_Test;User Id=sa;Password=Pass@123;TrustServerCertificate=True;";
    }

    public static async Task EnsureDatabaseAndTableAsync()
    {
        var masterConn =
            "Server=localhost,1433;Database=master;User Id=sa;Password=Pass@123;TrustServerCertificate=True;";

        await using (var connection = new SqlConnection(masterConn))
        {
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                IF DB_ID('ConfigR_Test') IS NULL
                BEGIN
                    CREATE DATABASE ConfigR_Test;
                END;
                ";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        var dbConn = GetConnectionString();
        await using (var connection = new SqlConnection(dbConn))
        {
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                IF OBJECT_ID('dbo.ConfigR', 'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[ConfigR] (
                        [Id]    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Key]   NVARCHAR(256) NOT NULL,
                        [Value] NVARCHAR(MAX) NOT NULL,
                        [Scope] NVARCHAR(128) NULL
                    );

                    CREATE UNIQUE INDEX IX_ConfigR_Key_Scope
                        ON [dbo].[ConfigR] ([Key], [Scope]);
                END
                ";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    public static async Task ClearTableAsync()
    {
        var dbConn = GetConnectionString();
        await using var connection = new SqlConnection(dbConn);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = @"
            DELETE FROM [dbo].[ConfigR_ConcurrencyTests];
            DELETE FROM [dbo].[ConfigR_ConfigStoreTests];
            DELETE FROM [dbo].[ConfigR_IntegrationTests];";
        
        try { await command.ExecuteNonQueryAsync().ConfigureAwait(false); }
        catch { }
    }
}
