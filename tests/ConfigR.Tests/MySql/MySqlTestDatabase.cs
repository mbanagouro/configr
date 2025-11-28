using MySqlConnector;
using System.Data;

namespace ConfigR.Tests.MySql;

public static class MySqlTestDatabase
{
    public static string GetConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("CONFIGR_TEST_MYSQL_CONN");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        // Default for local docker:
        return "Server=localhost;Database=ConfigR_Test;User Id=root;Password=root;";
    }

    public static async Task EnsureDatabaseAndTableAsync()
    {
        var masterConn = "Server=localhost;User Id=root;Password=root;";

        await using (var connection = new MySqlConnection(masterConn))
        {
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                CREATE DATABASE IF NOT EXISTS ConfigR_Test;
                ";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        var dbConn = GetConnectionString();
        await using (var connection = new MySqlConnection(dbConn))
        {
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ConfigR (
                    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    cfg_key VARCHAR(255) NOT NULL,
                    cfg_value TEXT NOT NULL,
                    scope VARCHAR(255) NULL,
                    UNIQUE KEY uk_config (cfg_key, scope)
                );
                ";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    public static async Task ClearTableAsync()
    {
        var dbConn = GetConnectionString();
        await using var connection = new MySqlConnection(dbConn);
        await connection.OpenAsync().ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = @"
            DELETE FROM ConfigR_ConcurrencyTests;
            DELETE FROM ConfigR_ConfigStoreTests;
            DELETE FROM ConfigR_IntegrationTests;";
        
        try { await command.ExecuteNonQueryAsync().ConfigureAwait(false); }
        catch { }
    }
}
