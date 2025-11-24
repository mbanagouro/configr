using Npgsql;

namespace ConfigR.Tests.Npgsql;

public static class NpgsqlTestDatabase
{
    public static string GetConnectionString()
    {
        var env = Environment.GetEnvironmentVariable("CONFIGR_TEST_POSTGRES_CONN");
        return string.IsNullOrWhiteSpace(env)
            ? "Host=localhost;Port=5432;Database=configr_test;Username=postgres;Password=123456;"
            : env;
    }

    public static async Task ClearTableAsync()
    {
        await using var conn = new NpgsqlConnection(GetConnectionString());
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM public.configr;";
        try { await cmd.ExecuteNonQueryAsync(); }
        catch { }
    }
}
