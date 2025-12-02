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

```csharp
Task<string?> GetAsync(string key, string? scope = null);
```

### SaveAsync

```csharp
Task SaveAsync(string key, string value, string? scope = null);
```

### DeleteAsync

```csharp
Task DeleteAsync(string key, string? scope = null);
```

### GetAllAsync

```csharp
Task<IEnumerable<ConfigItem>> GetAllAsync(string? scope = null);
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
- Serializadores padrão

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
    options.JsonSerializerOptions = new JsonSerializerOptions 
    { 
        PropertyNameCaseInsensitive = true 
    };
});
```

**Propriedades:**

| Propriedade | Tipo | Padrão | Descrição |
|---|---|---|---|
| `CacheDuration` | `TimeSpan?` | `TimeSpan.FromMinutes(10)` | Duração do cache em memória. `null` ou `TimeSpan.Zero` desabilita o cache. |
| `DefaultScope` | `Func<string>` | `() => "Default"` | Função que retorna o escopo padrão para configurações. |
| `JsonSerializerOptions` | `JsonSerializerOptions` | Padrão do .NET | Opções de serialização JSON customizadas. |

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

```csharp
bool TryGetAll(string scope, out IReadOnlyDictionary<string, ConfigEntry> entries, TimeSpan? cacheDuration = null);
```

**Parâmetros:**
- `scope` - Chave do escopo
- `entries` - Saída com as entradas em cache
- `cacheDuration` - Duração configurada para validação de expiração

**Retorno:**
- `true` se encontrado em cache e não expirado
- `false` se não encontrado ou expirado

### SetAll

```csharp
void SetAll(string scope, IReadOnlyDictionary<string, ConfigEntry> entries, TimeSpan? cacheDuration = null);
```

**Parâmetros:**
- `scope` - Chave do escopo
- `entries` - Entradas a cachear
- `cacheDuration` - Duração do cache. Se `null` ou `TimeSpan.Zero`, não caches.

### Clear

```csharp
void Clear(string scope);
```

Limpa cache de um escopo específico.

### ClearAll

```csharp
void ClearAll();
```

Limpa todo o cache.

---

## 🚨 Exceções

### ConfigNotFoundException

Lançada quando uma configuração não é encontrada e não pode ser criada.

```csharp
try
{
    var config = await configR.GetAsync<MyConfig>();
}
catch (ConfigNotFoundException ex)
{
    Console.WriteLine($"Config not found: {ex.Message}");
}
```

### SerializationException

Lançada quando há erro na serialização/desserialização.

```csharp
try
{
    await configR.SaveAsync(config);
}
catch (SerializationException ex)
{
    Console.WriteLine($"Serialization error: {ex.Message}");
}
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