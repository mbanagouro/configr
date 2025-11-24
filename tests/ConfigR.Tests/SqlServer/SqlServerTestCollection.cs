using Xunit;

namespace ConfigR.Tests.SqlServer;

[CollectionDefinition("SqlServerTests")]
public sealed class SqlServerTestCollection : ICollectionFixture<SqlServerTestFixture>
{
}

public sealed class SqlServerTestFixture
{
    // Placeholder for future shared setup if needed.
}
