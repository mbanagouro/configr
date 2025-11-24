using System.Collections.Concurrent;
using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ConfigR.MongoDb;

/// <summary>
/// MongoDB implementation of the configuration store.
/// </summary>
public sealed class MongoConfigStore : IConfigStore
{
    private readonly IMongoCollection<BsonDocument> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoConfigStore"/> class.
    /// </summary>
    /// <param name="options">The MongoDB configuration store options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="options"/>.Value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when ConnectionString or Database is null, empty, or whitespace.</exception>
    public MongoConfigStore(IOptions<MongoConfigStoreOptions> options)
    {
        var value = options.Value ?? throw new ArgumentNullException(nameof(options));
        
        if (string.IsNullOrWhiteSpace(value.ConnectionString))
            throw new ArgumentException("ConnectionString must be provided.", nameof(options));
        
        if (string.IsNullOrWhiteSpace(value.Database))
            throw new ArgumentException("Database must be provided.", nameof(options));
        
        if (string.IsNullOrWhiteSpace(value.Collection))
            value.Collection = "Configuracoes";

        var client = new MongoClient(value.ConnectionString);
        var database = client.GetDatabase(value.Database);
        _collection = database.GetCollection<BsonDocument>(value.Collection);

        EnsureIndexes();
    }

    /// <summary>
    /// Ensures that the required indexes are created on the MongoDB collection.
    /// </summary>
    private void EnsureIndexes()
    {
        var indexKeys = Builders<BsonDocument>.IndexKeys
            .Ascending("Key")
            .Ascending("Scope");

        var options = new CreateIndexOptions
        {
            Unique = true,
            Name = "IX_ConfigR_Key_Scope"
        };

        var model = new CreateIndexModel<BsonDocument>(indexKeys, options);
        _collection.Indexes.CreateOne(model);
    }

    /// <summary>
    /// Gets a configuration entry by key and optional scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The configuration entry if found; otherwise, null.</returns>
    public async Task<ConfigEntry?> GetAsync(string key, string? scope = null)
    {
        var filter = BuildFilter(key, scope);

        var doc = await _collection
            .Find(filter)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (doc is null)
            return null;

        return new ConfigEntry
        {
            Key = doc.TryGetValue("Key", out BsonValue? keyValue) ? keyValue.AsString : null,
            Value = doc.TryGetValue("Value", out BsonValue? value) ? value.AsString : null,
            Scope = doc.TryGetValue("Scope", out BsonValue? scopeValue) ? value.AsString : null
        };
    }

    /// <summary>
    /// Gets all configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The optional scope.</param>
    /// <returns>A read-only dictionary of configuration entries.</returns>
    public async Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null)
    {
        var filter = scope is null
            ? Builders<BsonDocument>.Filter.Empty
            : Builders<BsonDocument>.Filter.Eq("Scope", scope);

        using var cursor = await _collection
            .Find(filter)
            .ToCursorAsync()
            .ConfigureAwait(false);

        var dict = new Dictionary<string, ConfigEntry>(StringComparer.OrdinalIgnoreCase);

        await foreach (var doc in cursor.ToAsyncEnumerable().ConfigureAwait(false))
        {
            BsonValue? keyValue = null;
            BsonValue? valValue = null;

            if (!doc.TryGetValue("Key", out keyValue) ||
                !doc.TryGetValue("Value", out valValue))
            {
                continue;
            }

            BsonValue? scopeValue = null;
            doc.TryGetValue("Scope", out scopeValue);

            dict[keyValue.AsString] = new ConfigEntry
            {
                Key = keyValue.AsString,
                Value = valValue.AsString,
                Scope = !scopeValue.IsBsonNull ? scopeValue.AsString : null
            };
        }

        return dict;
    }

    /// <summary>
    /// Inserts or updates configuration entries.
    /// </summary>
    /// <param name="entries">The configuration entries to upsert.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>A task representing the asynchronous upsert operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entries"/> is null.</exception>
    public async Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
    {
        ArgumentNullException.ThrowIfNull(entries);

        foreach (var entry in entries)
        {
            if (entry is null)
            {
                continue;
            }

            var filter = BuildFilter(entry?.Key, scope);

            var update = Builders<BsonDocument>.Update
                .Set("Key", entry?.Key)
                .Set("Value", entry?.Value)
                .Set("Scope", entry?.Scope);

            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true
            };

            await _collection
                .FindOneAndUpdateAsync(filter, update, options)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Builds a MongoDB filter for the given key and optional scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>A filter definition for querying MongoDB documents.</returns>
    private static FilterDefinition<BsonDocument> BuildFilter(string? key, string? scope)
    {
        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Eq("Key", key);

        if (scope is not null)
        {
            filter &= builder.Eq("Scope", scope);
        }

        return filter;
    }
}
