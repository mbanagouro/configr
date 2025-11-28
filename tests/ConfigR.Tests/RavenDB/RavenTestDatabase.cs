using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace ConfigR.Tests.RavenDB;

public static class RavenTestDatabase
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static IDocumentStore? _store;

    public static string[] GetUrls()
    {
        var fromEnv = Environment.GetEnvironmentVariable("CONFIGR_TEST_RAVEN_URLS");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        return new[] { "http://localhost:8080" };
    }

    public static string GetDatabaseName()
    {
        var fromEnv = Environment.GetEnvironmentVariable("CONFIGR_TEST_RAVEN_DB");
        return string.IsNullOrWhiteSpace(fromEnv) ? "ConfigR_Test" : fromEnv;
    }

    public static async Task<IDocumentStore> GetDocumentStoreAsync()
    {
        if (_store is not null)
        {
            return _store;
        }

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_store is null)
            {
                var store = new DocumentStore
                {
                    Urls = GetUrls(),
                    Database = GetDatabaseName()
                };

                store.Initialize();
                await EnsureDatabaseAsync(store).ConfigureAwait(false);
                _store = store;
            }
        }
        finally
        {
            _lock.Release();
        }

        return _store;
    }

    private static async Task EnsureDatabaseAsync(IDocumentStore store)
    {
        try
        {
            await store.Maintenance.ForDatabase(GetDatabaseName())
                .SendAsync(new GetStatisticsOperation())
                .ConfigureAwait(false);
        }
        catch (DatabaseDoesNotExistException)
        {
            var record = new DatabaseRecord(GetDatabaseName());
            await store.Maintenance.Server
                .SendAsync(new CreateDatabaseOperation(record))
                .ConfigureAwait(false);
        }
    }

    public static async Task ClearDatabaseAsync()
    {
        var store = await GetDocumentStoreAsync().ConfigureAwait(false);

        var operation = await store.Operations
            .SendAsync(new DeleteByQueryOperation(new IndexQuery
            {
                Query = "from @all_docs"
            }))
            .ConfigureAwait(false);

        await operation.WaitForCompletionAsync().ConfigureAwait(false);
    }
}
