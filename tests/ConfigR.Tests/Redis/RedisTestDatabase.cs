using StackExchange.Redis;

namespace ConfigR.Tests.Redis;

public static class RedisTestDatabase
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("CONFIGR_TEST_REDIS_CONN")
        ?? "localhost:6379";

    public static async Task<IConnectionMultiplexer> GetConnectionAsync()
    {
        return await ConnectionMultiplexer.ConnectAsync(ConnectionString);
    }

    public static async Task FlushAsync(string keyPrefix = "configr-test")
    {
        var mux = await GetConnectionAsync();
        var server = mux.GetServer(mux.GetEndPoints().First());
        var db = mux.GetDatabase();

        var keys = server.Keys(pattern: $"{keyPrefix}:*").ToArray();

        foreach (var key in keys)
            await db.KeyDeleteAsync(key);
    }
}
