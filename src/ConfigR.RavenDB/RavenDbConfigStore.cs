using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace ConfigR.RavenDB;

/// <summary>
/// RavenDB implementation of the configuration store.
/// </summary>
public sealed class RavenDbConfigStore : IConfigStore
{
    private readonly IDocumentStore _store;
    private readonly RavenDbConfigStoreOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RavenDbConfigStore"/> class.
    /// </summary>
    /// <param name="store">The RavenDB document store.</param>
    /// <param name="options">The RavenDB configuration store options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> or <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when Urls or Database are not provided.</exception>
    public RavenDbConfigStore(
        IDocumentStore store,
        IOptions<RavenDbConfigStoreOptions> options)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (_options.Urls is null || _options.Urls.Length == 0)
        {
            throw new ArgumentException("At least one RavenDB URL must be provided.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(_options.Database))
        {
            throw new ArgumentException("Database must be provided.", nameof(options));
        }

        EnsureDatabaseExists();
    }

    /// <summary>
    /// Ensures that the target database exists, creating it if necessary.
    /// </summary>
    private void EnsureDatabaseExists()
    {
        try
        {
            _store.Maintenance.ForDatabase(_options.Database).Send(new GetStatisticsOperation());
        }
        catch (DatabaseDoesNotExistException)
        {
            var record = new DatabaseRecord(_options.Database);
            _store.Maintenance.Server.Send(new CreateDatabaseOperation(record));
        }
    }

    /// <summary>
    /// Builds the RavenDB document identifier for a given key and scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The formatted document identifier.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or whitespace.</exception>
    private string BuildDocumentId(string key, string? scope)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must be provided.", nameof(key));
        }

        var scopeSegment = scope is null ? "__default__" : scope;
        return $"{_options.KeyPrefix}/{scopeSegment}/{key}";
    }

    /// <summary>
    /// Opens a new asynchronous session for the configured database.
    /// </summary>
    private IAsyncDocumentSession OpenSession()
        => _store.OpenAsyncSession(new SessionOptions
        {
            Database = _options.Database
        });

    /// <summary>
    /// Gets a configuration entry by key and optional scope.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="scope">The optional scope.</param>
    /// <returns>The configuration entry if found; otherwise, null.</returns>
    public async Task<ConfigEntry?> GetAsync(string key, string? scope = null)
    {
        using var session = OpenSession();

        var document = await session
            .LoadAsync<RavenConfigDocument>(BuildDocumentId(key, scope))
            .ConfigureAwait(false);

        if (document is null)
        {
            return null;
        }

        return new ConfigEntry
        {
            Key = document.Key,
            Value = document.Value,
            Scope = document.Scope
        };
    }

    /// <summary>
    /// Gets all configuration entries for a given scope.
    /// </summary>
    /// <param name="scope">The optional scope.</param>
    /// <returns>A read-only dictionary of configuration entries.</returns>
    public async Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null)
    {
        using var session = OpenSession();

        var query = session.Query<RavenConfigDocument>()
            .Customize(x => x.WaitForNonStaleResults());

        query = scope is null
            ? query.Where(x => x.Scope == null)
            : query.Where(x => x.Scope == scope);

        var documents = await query.ToListAsync().ConfigureAwait(false);

        var dict = new Dictionary<string, ConfigEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var doc in documents)
        {
            dict[doc.Key] = new ConfigEntry
            {
                Key = doc.Key,
                Value = doc.Value,
                Scope = doc.Scope
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

        using var session = OpenSession();

        foreach (var entry in entries)
        {
            if (entry is null)
            {
                continue;
            }

            var scopeToUse = scope ?? entry.Scope;

            var document = new RavenConfigDocument
            {
                Id = BuildDocumentId(entry.Key, scopeToUse),
                Key = entry.Key,
                Value = entry.Value,
                Scope = scopeToUse
            };

            await session
                .StoreAsync(document, document.Id)
                .ConfigureAwait(false);
        }

        await session.SaveChangesAsync().ConfigureAwait(false);
    }

    private sealed class RavenConfigDocument
    {
        public required string Id { get; set; }

        public required string Key { get; set; }

        public string? Value { get; set; }

        public string? Scope { get; set; }
    }
}
