using ConfigR.Abstractions;
using ConfigR.SqlServer;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConfigR.Tests.SqlServer;

[Collection("SqlServerConfigStore")]
public sealed class SqlServerConfigStoreTests
{
    private async Task<SqlServerConfigStore> CreateStoreAsync()
    {
        await SqlServerTestDatabase.EnsureDatabaseAndTableAsync().ConfigureAwait(false);
        await SqlServerTestDatabase.ClearTableAsync().ConfigureAwait(false);

        var options = Options.Create(new SqlServerConfigStoreOptions
        {
            ConnectionString = SqlServerTestDatabase.GetConnectionString(),
            Schema = "dbo",
            Table = "ConfigR",
            AutoCreateTable = true
        });

        return new SqlServerConfigStore(options);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1030:Do not call ConfigureAwait(false) in test method", Justification = "<Pending>")]
    public async Task UpsertAndGetAll_Works()
    {
        var store = await CreateStoreAsync().ConfigureAwait(false);

        var entries = new List<ConfigEntry>
        {
            new() { Key = "checkoutconfig.loginrequired", Value = "true", Scope = null },
            new() { Key = "checkoutconfig.maxitems", Value = "10", Scope = null }
        };

        await store.UpsertAsync(entries, null).ConfigureAwait(false);

        var all = await store.GetAllAsync(null).ConfigureAwait(false);

        all.Should().ContainKey("checkoutconfig.loginrequired");
        all.Should().ContainKey("checkoutconfig.maxitems");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1030:Do not call ConfigureAwait(false) in test method", Justification = "<Pending>")]
    public async Task Upsert_RespectsScope()
    {
        var store = await CreateStoreAsync().ConfigureAwait(false);

        var entriesGlobal = new List<ConfigEntry>
        {
            new() { Key = "checkoutconfig.loginrequired", Value = "true", Scope = null }
        };
        var entriesScoped = new List<ConfigEntry>
        {
            new() { Key = "checkoutconfig.loginrequired", Value = "false", Scope = "loja-1" }
        };

        await store.UpsertAsync(entriesGlobal, null).ConfigureAwait(false);
        await store.UpsertAsync(entriesScoped, "loja-1").ConfigureAwait(false);

        var global = await store.GetAllAsync(null).ConfigureAwait(false);
        var scoped = await store.GetAllAsync("loja-1").ConfigureAwait(false);

        global["checkoutconfig.loginrequired"].Value.Should().Be("true");
        scoped["checkoutconfig.loginrequired"].Value.Should().Be("false");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1030:Do not call ConfigureAwait(false) in test method", Justification = "<Pending>")]
    public async Task GetAsync_ReturnsNull_WhenNotExists()
    {
        var store = await CreateStoreAsync().ConfigureAwait(false);

        var result = await store.GetAsync("not.exists", null).ConfigureAwait(false);

        result.Should().BeNull();
    }
}
