# API Reference

DocumentaÃ§Ã£o completa da API do ConfigR.

## ðŸŽ¯ Interface Principal - `IConfigR`

Interface central para ler e escrever configuraÃ§Ãµes.

### GetAsync

Recupera uma configuraÃ§Ã£o tipada.

```csharp
Task<T> GetAsync<T>(string? scope = null) where T : class, new();
```

**ParÃ¢metros:**
- `scope` (optional) - Escopo/tenant da configuraÃ§Ã£o. Se omitido, usa configuraÃ§Ã£o padrÃ£o.

**Retorno:**
- InstÃ¢ncia tipada da configuraÃ§Ã£o
- Se nÃ£o encontrada, retorna nova instÃ¢ncia com valores padrÃ£o

**Exemplos:**

```csharp
// ConfiguraÃ§Ã£o padrÃ£o
var config = await configR.GetAsync<CheckoutConfig>();

// ConfiguraÃ§Ã£o de um tenant especÃ­fico
var tenantConfig = await configR.GetAsync<CheckoutConfig>("tenant-123");
```

### SaveAsync

Salva uma configuraÃ§Ã£o tipada.

```csharp
Task SaveAsync<T>(T value, string? scope = null) where T : class;
```

**ParÃ¢metros:**
- `value` - InstÃ¢ncia de configuraÃ§Ã£o a salvar
- `scope` (optional) - Escopo/tenant onde salvar

**Comportamento:**
- Se existir, atualiza a configuraÃ§Ã£o
- Se nÃ£o existir, cria nova
- Invalida automaticamente o cache

**Exemplos:**

```csharp
var config = new CheckoutConfig { MaxItems = 50 };

// Salvar como padrÃ£o
await configR.SaveAsync(config);

// Salvar para tenant especÃ­fico
await configR.SaveAsync(config, "tenant-123");
```

### DeleteAsync

Remove uma configuraÃ§Ã£o.

```csharp
Task DeleteAsync<T>(string? scope = null) where T : class;
```

**ParÃ¢metros:**
- `scope` (optional) - Escopo/tenant de onde remover

**Exemplos:**

```csharp
// Remover configuraÃ§Ã£o padrÃ£o
await configR.DeleteAsync<CheckoutConfig>();

// Remover configuraÃ§Ã£o de tenant
await configR.DeleteAsync<CheckoutConfig>("tenant-123");
```

### GetAllAsync

Recupera todas as configuraÃ§Ãµes de um escopo.

```csharp
Task<IEnumerable<(string Key, string Value)>> GetAllAsync(string? scope = null);
```

**ParÃ¢metros:**
- `scope` (optional) - Escopo/tenant

**Retorno:**
- EnumerÃ¡vel com tuplas (Key, Value)

**Exemplos:**

```csharp
// Todas as configuraÃ§Ãµes padrÃ£o
var all = await configR.GetAllAsync();

// Todas as configuraÃ§Ãµes de um tenant
var tenantAll = await configR.GetAllAsync("tenant-123");

// Iterar
foreach (var (key, value) in await configR.GetAllAsync())
{
    Console.WriteLine($"{key}: {value}");
}
```

---

## ðŸª Interface de Armazenamento - `IConfigStore`

Interface de baixo nÃ­vel para implementaÃ§Ã£o de providers.

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

## âš™ï¸ InjeÃ§Ã£o de DependÃªncia

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

## ðŸ“¦ Pacotes NuGet

### Core

```bash
dotnet add package ConfigR.Core
```

Inclui:
- `IConfigR` interface
- DI extensions
- Cache em memÃ³ria
- Serializadores padrÃ£o

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

## ðŸ”§ OpÃ§Ãµes de ConfiguraÃ§Ã£o

### ConfigROptions

ConfiguraÃ§Ãµes do ConfigR core:

```csharp
builder.Services.AddConfigR(options =>
{
    options.CacheDuration = TimeSpan.FromMinutes(5);
    options.JsonSerializerOptions = new JsonSerializerOptions 
    { 
        PropertyNameCaseInsensitive = true 
    };
});
```

**Propriedades:**
- `CacheDuration` - DuraÃ§Ã£o do cache em memÃ³ria (padrÃ£o: 5 minutos)
- `JsonSerializerOptions` - OpÃ§Ãµes de serializaÃ§Ã£o JSON

---

## ðŸš¨ ExceÃ§Ãµes

### ConfigNotFoundException

LanÃ§ada quando uma configuraÃ§Ã£o nÃ£o Ã© encontrada e nÃ£o pode ser criada.

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

LanÃ§ada quando hÃ¡ erro na serializaÃ§Ã£o/desserializaÃ§Ã£o.

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

## ðŸ” Logging

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

## ðŸ“Š Exemplo Completo

```csharp
// Classe de configuraÃ§Ã£o
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
        // Ler
        var config = await _configR.GetAsync<AppConfig>();
        Console.WriteLine($"Feature: {config.Feature}");

        // Modificar
        config.Timeout = 60;

        // Salvar
        await _configR.SaveAsync(config);
    }
}
```

---

## ðŸ“š PrÃ³ximos Passos

- ðŸ“– [Voltar para DocumentaÃ§Ã£o](../index.md)
- ðŸš€ [InÃ­cio RÃ¡pido](../getting-started.md)
- ðŸ§ª [Testes](../testing.md)