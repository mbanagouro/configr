# Extensibilidade - Custom Providers

Crie seus próprios providers de armazenamento para o ConfigR.

## ?? Quando Criar um Custom Provider

- Você usa um banco de dados não suportado nativamente
- Precisa de lógica customizada (criptografia, compressão)
- Quer integrar com sistema legado
- Tem requisitos de performance específicos

## ??? Implementar um Custom Provider

### 1?? Entender a Interface

```csharp
// ConfigR.Abstractions
public interface IConfigStore
{
    Task<string?> GetAsync(string key, string? scope = null);
    Task SaveAsync(string key, string value, string? scope = null);
    Task DeleteAsync(string key, string? scope = null);
    Task<IEnumerable<ConfigItem>> GetAllAsync(string? scope = null);
}

public class ConfigItem
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string? Scope { get; set; }
}
```

### 2?? Criar Classe do Provider

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

    public async Task<string?> GetAsync(string key, string? scope = null)
    {
        try
        {
            _logger.LogInformation($"Getting config: {key}, scope: {scope}");
            
            // Sua lógica de leitura aqui
            var value = await FetchFromCustomStore(key, scope);
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting config: {key}");
            throw;
        }
    }

    public async Task SaveAsync(string key, string value, string? scope = null)
    {
        try
        {
            _logger.LogInformation($"Saving config: {key}, scope: {scope}");
            
            // Sua lógica de escrita aqui
            await WriteToCustomStore(key, value, scope);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving config: {key}");
            throw;
        }
    }

    public async Task DeleteAsync(string key, string? scope = null)
    {
        // Implementar conforme necessário
        await RemoveFromCustomStore(key, scope);
    }

    public async Task<IEnumerable<ConfigItem>> GetAllAsync(string? scope = null)
    {
        // Implementar para listar todas as configs
        return await FetchAllFromCustomStore(scope);
    }

    // Seus métodos privados aqui
    private async Task<string?> FetchFromCustomStore(string key, string? scope)
    {
        // TODO: Implementar
        return null;
    }

    private async Task WriteToCustomStore(string key, string value, string? scope)
    {
        // TODO: Implementar
    }

    private async Task RemoveFromCustomStore(string key, string? scope)
    {
        // TODO: Implementar
    }

    private async Task<IEnumerable<ConfigItem>> FetchAllFromCustomStore(string? scope)
    {
        // TODO: Implementar
        return Enumerable.Empty<ConfigItem>();
    }
}
```

### 3?? Criar Classe de Opções

```csharp
public class CustomConfigStoreOptions
{
    public string ConnectionString { get; set; }
    public string CustomSetting { get; set; }
    public int Timeout { get; set; } = 30;
}
```

### 4?? Criar Método de Extensão

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

### 5?? Registrar no DI

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

## ?? Exemplo Completo: Provider em Memória

```csharp
// Simples provider que armazena tudo na memória
public class InMemoryConfigStore : IConfigStore
{
    private readonly Dictionary<string, ConfigItem> _store 
        = new();

    public Task<string?> GetAsync(string key, string? scope = null)
    {
        var fullKey = BuildKey(key, scope);
        var value = _store.TryGetValue(fullKey, out var item) 
            ? item.Value 
            : null;
        return Task.FromResult(value);
    }

    public Task SaveAsync(string key, string value, string? scope = null)
    {
        var fullKey = BuildKey(key, scope);
        _store[fullKey] = new ConfigItem 
        { 
            Key = key, 
            Value = value, 
            Scope = scope 
        };
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, string? scope = null)
    {
        var fullKey = BuildKey(key, scope);
        _store.Remove(fullKey);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ConfigItem>> GetAllAsync(string? scope = null)
    {
        var items = _store.Values
            .Where(i => scope == null || i.Scope == scope)
            .ToList();
        return Task.FromResult(items.AsEnumerable());
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

## ?? Testar seu Provider

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
        await _store.SaveAsync("key1", "value1");

        // Act
        var result = await _store.GetAsync("key1");

        // Assert
        Assert.AreEqual("value1", result);
    }

    [Test]
    public async Task GetAsync_WithScope_ShouldIsolateValues()
    {
        // Arrange
        await _store.SaveAsync("key1", "value1", scope: "scope1");
        await _store.SaveAsync("key1", "value2", scope: "scope2");

        // Act
        var result1 = await _store.GetAsync("key1", "scope1");
        var result2 = await _store.GetAsync("key1", "scope2");

        // Assert
        Assert.AreEqual("value1", result1);
        Assert.AreEqual("value2", result2);
    }
}
```

## ?? Próximos Passos

- ?? [Aprenda sobre Scopes](scopes.md)
- ?? [Otimize com Cache](caching.md)
- ?? [Voltar para Configuração](../configuration.md)