using MongoDB.Bson;
using MongoDB.Driver;

namespace ConfigR.Tests.MongoDB;

public static class MongoTestDatabase
{
    public static string GetConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("CONFIGR_TEST_MONGO_CONN");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv;

        return "mongodb://localhost:27017";
    }

    public static string GetDatabaseName() => "ConfigR_Test";

    public static async Task ClearCollectionAsync()
    {
        var client = new MongoClient(GetConnectionString());
        var db = client.GetDatabase(GetDatabaseName());

        var cursor = await db.ListCollectionNamesAsync().ConfigureAwait(false);

        foreach (var collectionName in cursor.ToList())
        {
            var collection = db.GetCollection<BsonDocument>(collectionName);
            
            await collection
                .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty)
                .ConfigureAwait(false);
        }
    }
}
