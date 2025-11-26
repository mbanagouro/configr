namespace ConfigR.Redis;

public sealed class RedisConfigStoreOptions
{
    public required string ConnectionString { get; set; }
    public string KeyPrefix { get; set; } = "configr";
}