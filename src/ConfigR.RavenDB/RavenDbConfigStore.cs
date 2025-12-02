using ConfigR.Abstractions;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System.Text.RegularExpressions;

namespace ConfigR.RavenDB;

/// <summary>
/// RavenDB implementation of the configuration store.
/// </summary>
public sealed class RavenDbConfigStore : IConfigStore
{
    private readonly IDocumentStore _store;
    private readonly RavenDbConfigStoreOptions _options;
    
    // Regex para validar prefixos seguros
    private static readonly Regex SafeKeyPrefixRegex = new(@"^[a-zA-Z0-9_\-/]{1,100}$", RegexOptions.Compiled);
    
    // Tamanho máximo para valores de configuração (100MB)
    private const int MaxValueSize = 100 * 1024 * 1024;
    private const int MaxKeySize = 256;
    private const int MaxScopeSize = 128;

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
        
        // Validar key prefix
        if (!string.IsNullOrWhiteSpace(_options.KeyPrefix))
        {
            ValidateKeyPrefix(_options.KeyPrefix);
        }

        EnsureDatabaseExists();
    }
    
    /// <summary>
    /// Valida que um prefixo de chave é seguro.
    /// </summary>
    private static void ValidateKeyPrefix(string prefix)
    {
        if (!SafeKeyPrefixRegex.IsMatch(prefix))
        {
            throw new ArgumentException(
                $"Invalid key prefix '{prefix}'. Only alphanumeric characters, underscores, hyphens, and forward slashes are allowed (max 100 characters).",
                nameof(prefix));
        }
    }
    
    /// <summary>
    /// Valida o tamanho do valor para prevenir ataques de negação de serviço.
    /// </summary>
    private static void ValidateValueSize(string? value, string key)
    {
        if (value?.Length > MaxValueSize)
        {
            throw new ArgumentException(
                $"Configuration value for key '{key}' exceeds maximum allowed size of {MaxValueSize} bytes.",
                nameof(value));
        }
    }
    
    /// <summary>
    /// Valida tamanhos de key e scope.
    /// </summary>
    private static void ValidateInputSizes(string key, string? scope)
    {
        if (key.Length > MaxKeySize)
        {
            throw new ArgumentException(
                $"Configuration key '{key}' exceeds maximum allowed length of {MaxKeySize} characters.",
                nameof(key));
        }
        
        if (scope?.Length > MaxScopeSize)
        {
            throw new ArgumentException(
                $"Scope '{scope}' exceeds maximum allowed length of {MaxScopeSize} characters.",
                nameof(scope));
        }
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
        
        ValidateInputSizes(key, scope);

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
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key must be provided.", nameof(key));
        }
        
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
        if (scope?.Length > MaxScopeSize)
        {
            throw new ArgumentException(
                $"Scope '{scope}' exceeds maximum allowed length of {MaxScopeSize} characters.",
                nameof(scope));
        }
        
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
    /// <exception cref="ArgumentException">Thrown when entry validation fails.</exception>
    public async Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
    {
        ArgumentNullException.ThrowIfNull(entries);

        using var session = OpenSession();

        foreach (var entry in entries)
        {
            if (entry is null || string.IsNullOrWhiteSpace(entry.Key))
            {
                continue;
            }

            var scopeToUse = scope ?? entry.Scope;
            
            // Validar tamanhos
            ValidateInputSizes(entry.Key, scopeToUse);
            ValidateValueSize(entry.Value, entry.Key);

            var document = new RavenConfigDocument
            {
                Id = BuildDocumentId(entry.Key, scopeToUse),
                Key = entry.Key,
                Value = entry.Value ?? string.Empty,
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
