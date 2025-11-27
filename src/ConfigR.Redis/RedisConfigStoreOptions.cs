namespace ConfigR.Redis;

/// <summary>
/// Configuration options for the Redis configuration store.
/// </summary>
public sealed class RedisConfigStoreOptions
{
    /// <summary>
    /// Gets or sets the Redis connection string.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the prefix for Redis keys.
    /// Defaults to "configr".
    /// </summary>
    public string KeyPrefix { get; set; } = "configr";
}