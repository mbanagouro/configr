namespace ConfigR.MongoDB;

/// <summary>
/// Configuration options for the MongoDB configuration store.
/// </summary>
public sealed class MongoConfigStoreOptions
{
    /// <summary>
    /// Gets or sets the MongoDB connection string.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the MongoDB database name.
    /// </summary>
    public required string Database { get; set; }

    /// <summary>
    /// Gets or sets the MongoDB collection name.
    /// Defaults to "ConfigR".
    /// </summary>
    public string Collection { get; set; } = "ConfigR";
}