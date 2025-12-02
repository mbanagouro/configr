# API Reference

Documentação completa da API do ConfigR.

## 🎯 Interface Principal - `IConfigR`

Interface central para ler e escrever configurações.

### GetAsync

Recupera uma configuração tipada.

```csharp
Task<T> GetAsync<T>(string? scope = null) where T : class, new();
```

**Parâmetros:**
- `scope` (optional) - Escopo/tenant da configuração. Se omitido, usa configuração padrão.

**Retorno:**
- Instância tipada da configuração
- Se não encontrada, retorna nova instância com valores padrão

**Exemplos:**

```csharp
// Configuração padrão
var config = await configR.GetAsync<CheckoutConfig>();

// Configuração de um tenant específico
var tenantConfig = await configR.GetAsync<CheckoutConfig>("tenant-123");
```

### SaveAsync

Salva uma configuração tipada.

```csharp
Task SaveAsync<T>(T value, string? scope = null) where T : class;
```

**Parâmetros:**
- `value` - Instância de configuração a salvar
- `scope` (optional) - Escopo/tenant onde salvar

**Comportamento:**
- Se existir, atualiza a configuração
- Se não existir, cria nova
- Invalida automaticamente o cache

**Exemplos:**

```csharp
var config = new CheckoutConfig { MaxItems = 50 };

// Salvar como padrão
await configR.SaveAsync(config);

// Salvar para tenant específico
await configR.SaveAsync(config, "tenant-123");
```

### DeleteAsync

Remove uma configuração.

```csharp
Task DeleteAsync<T>(string? scope = null) where T : class;
```

**Parâmetros:**
- `scope` (optional) - Escopo/tenant de onde remover

**Exemplos:**

```csharp
// Remover configuração padrão
await configR.DeleteAsync<CheckoutConfig>();

// Remover configuração de tenant
await configR.DeleteAsync<CheckoutConfig>("tenant-123");
```

### GetAllAsync

Recupera todas as configurações de um escopo.

```csharp
Task<IEnumerable<(string Key, string Value)>> GetAllAsync(string? scope = null);
```

**Parâmetros:**
- `scope` (optional) - Escopo/tenant

**Retorno:**
- Enumerável com tuplas (Key, Value)

**Exemplos:**

```csharp
// Todas as configurações padrão
var all = await configR.GetAllAsync();

// Todas as configurações de um tenant
var tenantAll = await configR.GetAllAsync("tenant-123");

// Iterar
foreach (var (key, value) in await configR.GetAllAsync())
{
    Console.WriteLine($"{key}: {value}");
}
```

---

## 🔪 Interface de Armazenamento - `IConfigStore`

Interface de baixo nível para implementação de providers.

### GetAsync

Obtém uma entrada de configuração por chave e escopo opcional.

```csharp
Task<ConfigEntry?> GetAsync(string key, string? scope = null);
```

**Parâmetros:**
- `key` - Chave da configuração
- `scope` (optional) - Escopo opcional

**Retorno:**
- `ConfigEntry` se encontrado
- `null` se não encontrado

### GetAllAsync

Obtém todas as entradas de configuração para um escopo.

```csharp
Task<IReadOnlyDictionary<string, ConfigEntry>> GetAllAsync(string? scope = null);
```

**Parâmetros:**
- `scope` (optional) - Escopo opcional

**Retorno:**
- Dicionário somente leitura com todas as entradas do escopo

### UpsertAsync

Insere ou atualiza entradas de configuração.

```csharp
Task UpsertAsync(IEnumerable<ConfigEntry> entries, string? scope = null);
```

**Parâmetros:**
- `entries` - Coleção de entradas a serem inseridas/atualizadas
- `scope` (optional) - Escopo opcional

**ConfigEntry:**

```csharp
public sealed class ConfigEntry
{
    public string? Key { get; init; }
    public string? Value { get; init; }
    public string? Scope { get; init; }
}
```

---

## ⚙️ Injeção de Dependência

### Registrar ConfigR

```csharp
builder.Services
    .AddConfigR()
    .UseSqlServer(connectionString);
```

### Resolver IConfigR

```csharp
// Em controller, service, etc
public class MyService
{
    private readonly IConfigR _configR;

    public MyService(IConfigR configR)
    {
        _configR = configR;
    }
}
```

---

## 📦 Pacotes NuGet

### Core

```bash
dotnet add package ConfigR.Core
```

Inclui:
- `IConfigR` interface
- DI extensions
- Cache em memória com duração configurável
- Serializador JSON padrão (System.Text.Json)

### Providers

```bash
# SQL Server
dotnet add package ConfigR.SqlServer

# MySQL
dotnet add package ConfigR.MySql

# PostgreSQL
dotnet add package ConfigR.Npgsql

# MongoDB
dotnet add package ConfigR.MongoDB

# Redis
dotnet add package ConfigR.Redis

# RavenDB
dotnet add package ConfigR.RavenDB
```

---

## 🔧 Opções de Configuração

### ConfigROptions

Configurações do ConfigR core:

```csharp
builder.Services.AddConfigR(options =>
{
    options.CacheDuration = TimeSpan.FromMinutes(10);
    options.DefaultScope = () => "Default";
});
```

**Propriedades:**

| Propriedade | Tipo | Padrão | Descrição |
|---|---|---|---|
| `CacheDuration` | `TimeSpan?` | `TimeSpan.FromMinutes(10)` | Duração do cache em memória. `null` ou `TimeSpan.Zero` desabilita o cache. |
| `DefaultScope` | `Func<string>` | `() => "Default"` | Função que retorna o escopo padrão para configurações. |

**Exemplos:**

```csharp
// Cache curto (1 minuto)
builder.Services.AddConfigR(options =>
{
    options.CacheDuration = TimeSpan.FromMinutes(1);
});

// Cache longo (1 hora)
builder.Services.AddConfigR(options =>
{
    options.CacheDuration = TimeSpan.FromHours(1);
});

// Sem cache (sempre fresco do banco)
builder.Services.AddConfigR(options =>
{
    options.CacheDuration = TimeSpan.Zero;
});

// Customizar escopo padrão
builder.Services.AddConfigR(options =>
{
    options.DefaultScope = () => Environment.GetEnvironmentVariable("TENANT_ID") ?? "Default";
});
 ```

---

## 💾 Interface de Cache - `IConfigCache`

Interface para implementação customizada de cache.

### TryGetAll

Tenta recuperar todas as entradas de cache para um escopo.

```csharp
bool TryGetAll(string scope, out IReadOnlyDictionary<string, ConfigEntry> entries);
```

**Parâmetros:**
- `scope` - Chave do escopo
- `entries` - Saída com as entradas em cache

**Retorno:**
- `true` se encontrado em cache e não expirado
- `false` se não encontrado ou expirado

**Comportamento:**
- Verifica automaticamente a expiração baseada na duração definida em `SetAll`
- Remove automaticamente entradas expiradas do cache

### SetAll

Armazena todas as entradas de configuração para um escopo no cache.

```csharp
void SetAll(string scope, IReadOnlyDictionary<string, ConfigEntry> entries, TimeSpan? cacheDuration = null);
```

**Parâmetros:**
- `scope` - Chave do escopo
- `entries` - Entradas a cachear
- `cacheDuration` - Duração do cache. Se `null` ou `TimeSpan.Zero`, não armazena no cache.

**Comportamento:**
- Define o tempo de expiração baseado em `cacheDuration`
- Se `cacheDuration` for `null` ou `TimeSpan.Zero`, o cache é desabilitado para essa entrada

### Clear

Limpa cache de um escopo específico.

```csharp
void Clear(string scope);
```

**Uso:**
```csharp
// Invalida cache após salvar
await configR.SaveAsync(config);
_cache.Clear(scopeKey); // Chamado automaticamente
```

### ClearAll

Limpa todo o cache.

```csharp
void ClearAll();
```

**Uso:**
```csharp
// Útil em cenários de teste ou reset completo
_cache.ClearAll();
```

---

## 🔄 Serialização

ConfigR usa `System.Text.Json` para serializar/desserializar valores. O serializador padrão está em `ConfigR.Core.DefaultConfigSerializer`.

### IConfigSerializer

```csharp
public interface IConfigSerializer
{
    string Serialize(object? value);
    object? Deserialize(string serializedValue, Type targetType);
}
```

**Comportamento Padrão:**
- Usa `JsonSerializerOptions.Web` como base
- Null retorna string vazia na serialização
- String vazia/null retorna valor padrão na desserialização
- Suporta tipos complexos e coleções

**Exemplo de Serialização:**

```csharp
var config = new CheckoutConfig 
{ 
    MaxItems = 50,
    Tags = new List<string> { "tag1", "tag2" }
};

// Internamente serializado como JSON
// {"MaxItems":50,"Tags":["tag1","tag2"]}
```

---

## 🔍 Logging

ConfigR usa `ILogger<T>` para logging:

```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});
```

**Categorias de Log:**
- `ConfigR.Core.*` - Logs do core
- `ConfigR.SqlServer.*` - Logs do provider SQL Server
- (similar para outros providers)

---

## 📊 Exemplo Completo

```csharp
// Classe de configuração
public sealed class AppConfig
{
    public string Feature { get; set; } = "enabled";
    public int Timeout { get; set; } = 30;
}

// Program.cs
builder.Services
    .AddConfigR(options =>
    {
        options.CacheDuration = TimeSpan.FromMinutes(10);
        options.DefaultScope = () => "Global";
    })
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));

// Service
public class AppService
{
    private readonly IConfigR _configR;

    public AppService(IConfigR configR)
    {
        _configR = configR;
    }

    public async Task Run()
    {
        // Ler (usa cache se disponível)
        var config = await _configR.GetAsync<AppConfig>();
        Console.WriteLine($"Feature: {config.Feature}");

        // Modificar
        config.Timeout = 60;

        // Salvar (invalida cache)
        await _configR.SaveAsync(config);
    }
}
```

---

## 📚 Próximos Passos

- 📖 [Voltar para Documentação](../index.md)
- 🚀 [Início Rápido](../getting-started.md)
- ⏱️ [Entenda o Cache](../advanced/caching.md)
- 🧪 [Testes](../testing.md)