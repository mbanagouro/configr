# ConfiguraÃ§Ã£o

Entenda como configurar o ConfigR para suas necessidades.

## ðŸ”§ ConfiguraÃ§Ã£o BÃ¡sica

### Registrar no DI

```csharp
// Program.cs
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

## ðŸ§© Escolher um Provider

### SQL Server (PadrÃ£o)

```csharp
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### MySQL

```csharp
builder.Services
    .AddConfigR()
    .UseMySql(builder.Configuration.GetConnectionString("ConfigR"));
```

### PostgreSQL (Npgsql)

```csharp
builder.Services
    .AddConfigR()
    .UseNpgsql(builder.Configuration.GetConnectionString("ConfigR"));
```

### MongoDB

```csharp
builder.Services
    .AddConfigR()
    .UseMongoDb(
        connectionString: "mongodb://localhost:27017",
        databaseName: "ConfigR"
    );
```

### Redis

```csharp
builder.Services
    .AddConfigR()
    .UseRedis("localhost:6379");
```

### RavenDB

```csharp
builder.Services
    .AddConfigR()
    .UseRavenDb(
        urls: new[] { "http://localhost:8080" },
        databaseName: "ConfigR"
    );
```

## ðŸ“ Definir Classe de ConfiguraÃ§Ã£o

```csharp
// Sua classe de configuraÃ§Ã£o
public sealed class MyConfig
{
    // Propriedades com valores padrÃ£o
    public string Feature { get; set; } = "enabled";
    public int Timeout { get; set; } = 30;
    public bool Debug { get; set; } = false;
}
```

### Boas PrÃ¡ticas

- âœ… Use classes `sealed` para evitar heranÃ§a
- âœ… Sempre forneÃ§a valores padrÃ£o (`= default;`)
- âœ… Use tipos simples quando possÃ­vel (string, int, bool, decimal)
- âœ… Nomeie a classe com sufixo `Config`: `CheckoutConfig`, `PaymentConfig`
- âœ… Propriedades public com get/set

## ðŸ—„ï¸ appsettings.json

Configure a connection string:

```json
{
  "ConnectionStrings": {
    "ConfigR": "Server=localhost,1433;Database=configr;User Id=sa;Password=Pass@123;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## ðŸš€ Uso em Controllers/Services

### Injetar IConfigR

```csharp
[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfigR _configR;

    public ConfigController(IConfigR configR)
    {
        _configR = configR;
    }

    [HttpGet]
    public async Task<MyConfig> GetConfig()
    {
        return await _configR.GetAsync<MyConfig>();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateConfig([FromBody] MyConfig config)
    {
        await _configR.SaveAsync(config);
        return Ok();
    }
}
```

## ðŸ§± Usando Scopes

Para multi-tenant, use scopes:

```csharp
// Ler com escopo
var config = await _configR.GetAsync<MyConfig>("tenant-123");

// Salvar com escopo
await _configR.SaveAsync(config, "tenant-123");
```

Veja [Scopes](advanced/scopes.md) para mais detalhes.

## âš™ï¸ OpÃ§Ãµes AvanÃ§adas

### Configurar Cache

```csharp
builder.Services
    .AddConfigR(options =>
    {
        options.CacheDuration = TimeSpan.FromMinutes(5);
    })
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### Configurar SerializaÃ§Ã£o

```csharp
builder.Services
    .AddConfigR(options =>
    {
        options.JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    })
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

## ðŸ” Troubleshooting

### Erro: "Connection refused"

- Verifique se o banco de dados estÃ¡ rodando
- Confirme a connection string em `appsettings.json`
- Teste a conexÃ£o manualmente

### Erro: "Tabela nÃ£o existe"

- Crie a tabela manualmente (veja seu provider)
- Ou configure `AutoCreateTable = true` (se suportado)

### Erro: "SerializaÃ§Ã£o falhou"

- Verifique se sua classe tem um construtor sem parÃ¢metros
- Confirme que as propriedades sÃ£o pÃºblicas
- Use tipos simples ou complexos (JSON-serializable)

## ðŸ“š PrÃ³ximos Passos

- ðŸ§± [Aprenda sobre Scopes](advanced/scopes.md)
- ðŸš€ [Otimize com Cache](advanced/caching.md)
- ðŸ§© [Escolha um Provider](storage/sql-server.md)