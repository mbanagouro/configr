# Extensibilidade - Custom Providers

Crie seus próprios providers de armazenamento para o ConfigR.

## 🎯 Quando Criar um Custom Provider

- Você usa um banco de dados não suportado nativamente
- Precisa de lógica customizada (criptografia, compressão)
- Quer integrar com sistema legado
- Tem requisitos de performance específicos

## 🛠️ Implementar um Custom Provider

### 1️⃣ Entender a Interface

```csharp
// ConfigR.Abstractions
public interface IConfigStore
{
    /// <summary>
    /// Gets a configuration entry by key and optional scope.
    /// </summary>
    Task<ConfigEntry?> GetAsync(string key, string? scope = null);

    /// <summary>
    /// Gets all configuration entries for a given scope.
    /// </summary>
    Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null);

    /// <summary>
    /// Inserts or updates configuration entries.
    /// </summary>
    Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null);
}

public sealed class ConfigEntry
{
    public string? Key { get; init; }
    public string? Value { get; init; }
    public string? Scope { get; init; }
}
```

### 2️⃣ Criar Classe do Provider

```csharp
public class CustomConfigStore : IConfigStore
{
    private readonly CustomConfigStoreOptions _options;
    private readonly ILogger<CustomConfigStore> _logger;

    public CustomConfigStore(
        IOptions<CustomConfigStoreOptions> options,
        ILogger<CustomConfigStore> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ConfigEntry?> GetAsync(string key, string? scope = null)
    {
        try
        {
            _logger.LogInformation($"Getting config: {key}, scope: {scope}");
            
            // Sua lógica de leitura aqui
            var entry = await FetchFromCustomStore(key, scope);
            
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting config: {key}");
            throw;
        }
    }

    public async Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null)
    {
        try
        {
            _logger.LogInformation($"Getting all configs for scope: {scope}");
            
            // Sua lógica para buscar todas as entradas
            var entries = await FetchAllFromCustomStore(scope);
            
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all configs");
            throw;
        }
    }

    public async Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
    {
        try
        {
            _logger.LogInformation($"Upserting configs for scope: {scope}");
            
            // Sua lógica de escrita aqui
            await WriteToCustomStore(entries, scope);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting configs");
            throw;
        }
    }

    // Seus métodos privados aqui
    private async Task<ConfigEntry?> FetchFromCustomStore(string key, string? scope)
    {
        // TODO: Implementar
        return null;
    }

    private async Task<IReadOnlyDictionary<string, ConfigEntry>> FetchAllFromCustomStore(string? scope)
    {
        // TODO: Implementar
        return new Dictionary<string, ConfigEntry>();
    }

    private async Task WriteToCustomStore(IEnumerable<ConfigEntry> entries, string? scope)
    {
        // TODO: Implementar
    }
}
```

### 3️⃣ Criar Classe de Opções

```csharp
public class CustomConfigStoreOptions
{
    public string ConnectionString { get; set; }
    public string CustomSetting { get; set; }
    public int Timeout { get; set; } = 30;
}
```

### 4️⃣ Criar Método de Extensão

```csharp
public static class ConfigRBuilderExtensions
{
    public static IConfigRBuilder UseCustomStore(
        this IConfigRBuilder builder,
        string connectionString,
        Action<CustomConfigStoreOptions>? configureOptions = null)
    {
        var options = new CustomConfigStoreOptions
        {
            ConnectionString = connectionString
        };

        configureOptions?.Invoke(options);

        builder.Services.Configure<CustomConfigStoreOptions>(opt =>
        {
            opt.ConnectionString = options.ConnectionString;
            opt.CustomSetting = options.CustomSetting;
            opt.Timeout = options.Timeout;
        });

        builder.Services.AddScoped<IConfigStore, CustomConfigStore>();

        return builder;
    }
}
```

### 5️⃣ Registrar no DI

```csharp
builder.Services
    .AddConfigR()
    .UseCustomStore(
        connectionString: "custom://localhost",
        configureOptions: options =>
        {
            options.CustomSetting = "valor";
            options.Timeout = 60;
        });
```

## 📝 Exemplo Completo: Provider em Memória

```csharp
using System.Collections.Concurrent;
using ConfigR.Abstractions;

// Simples provider que armazena tudo na memória
public class InMemoryConfigStore : IConfigStore
{
    private readonly ConcurrentDictionary<string, ConfigEntry> _store 
        = new(StringComparer.OrdinalIgnoreCase);

    public Task<ConfigEntry?> GetAsync(string key, string? scope = null)
    {
        var fullKey = BuildKey(key, scope);
        _store.TryGetValue(fullKey, out var entry);
        return Task.FromResult<ConfigEntry?>(entry);
    }

    public Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null)
    {
        if (scope is null)
        {
            // Retorna todas as entradas
            return Task.FromResult<IReadOnlyDictionary<string, ConfigEntry>>(
                new Dictionary<string, ConfigEntry>(_store));
        }

        // Filtra entradas pelo scope
        var prefix = $"{scope}:";
        var filtered = _store
            .Where(kvp => kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                kvp => kvp.Key.Substring(prefix.Length),
                kvp => kvp.Value,
                StringComparer.OrdinalIgnoreCase
            );

        return Task.FromResult<IReadOnlyDictionary<string, ConfigEntry>>(filtered);
    }

    public Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null)
    {
        foreach (var entry in entries)
        {
            var fullKey = BuildKey(entry.Key ?? "", scope);
            _store[fullKey] = entry;
        }
       
        return Task.CompletedTask;
    }

    private static string BuildKey(string key, string? scope)
    {
        return scope != null ? $"{scope}:{key}" : key;
    }
}

// Extensão
public static class ConfigRBuilderExtensions
{
    public static IConfigRBuilder UseInMemory(this IConfigRBuilder builder)
    {
        builder.Services.AddScoped<IConfigStore, InMemoryConfigStore>();
        return builder;
    }
}
```

## 🧪 Testar seu Provider

```csharp
[TestFixture]
public class CustomConfigStoreTests
{
    private IConfigStore _store;

    [SetUp]
    public void Setup()
    {
        _store = new CustomConfigStore(
            Options.Create(new CustomConfigStoreOptions { /* ... */ }),
            new Mock<ILogger<CustomConfigStore>>().Object
        );
    }

    [Test]
    public async Task GetAsync_ShouldReturnValue()
    {
        // Arrange
        var entry = new ConfigEntry 
        { 
            Key = "key1", 
            Value = "value1" 
        };
        await _store.UpsertAsync(new[] { entry });

        // Act
        var result = await _store.GetAsync("key1");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("value1", result.Value);
    }

    [Test]
    public async Task GetAllAsync_WithScope_ShouldReturnFilteredEntries()
    {
        // Arrange
        var entries = new[]
        {
            new ConfigEntry { Key = "key1", Value = "value1" },
            new ConfigEntry { Key = "key2", Value = "value2" }
        };
        await _store.UpsertAsync(entries, scope: "scope1");

        // Act
        var result = await _store.GetAllAsync("scope1");

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.ContainsKey("key1"));
        Assert.AreEqual("value1", result["key1"].Value);
    }

    [Test]
    public async Task UpsertAsync_ShouldUpdateExistingEntry()
    {
        // Arrange
        var entry1 = new ConfigEntry { Key = "key1", Value = "value1" };
        await _store.UpsertAsync(new[] { entry1 });

        // Act
        var entry2 = new ConfigEntry { Key = "key1", Value = "updated" };
        await _store.UpsertAsync(new[] { entry2 });

        var result = await _store.GetAsync("key1");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("updated", result.Value);
    }
}
```

## 📚 Próximos Passos

- 🧱 [Aprenda sobre Scopes](scopes.md)
- 🚀 [Otimize com Cache](caching.md)
- 🔧 [Voltar para Configuração](../configuration.md)