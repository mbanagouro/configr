# Configuração

Entenda como configurar o ConfigR para suas necessidades.

## 🔧 Configuração Básica

### Registrar no DI

```csharp
// Program.cs
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

## 🧩 Escolher um Provider

### SQL Server (Padrão)

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

## 📝 Definir Classe de Configuração

```csharp
// Sua classe de configuração
public sealed class MyConfig
{
    // Propriedades com valores padrão
    public string Feature { get; set; } = "enabled";
    public int Timeout { get; set; } = 30;
    public bool Debug { get; set; } = false;
}
```

### Boas Práticas

- ✅ Use classes `sealed` para evitar herança
- ✅ Sempre forneça valores padrão (`= default;`)
- ✅ Use tipos simples quando possível (string, int, bool, decimal)
- ✅ Nomeie a classe com sufixo `Config`: `CheckoutConfig`, `PaymentConfig`
- ✅ Propriedades public com get/set

## 🗄️ appsettings.json

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

## 🚀 Uso em Controllers/Services

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

## 🧱 Usando Scopes

Para multi-tenant, use scopes:

```csharp
// Ler com escopo
var config = await _configR.GetAsync<MyConfig>("tenant-123");

// Salvar com escopo
await _configR.SaveAsync(config, "tenant-123");
```

Veja [Scopes](advanced/scopes.md) para mais detalhes.

## ⚙️ Opções Avançadas

### Configurar Cache

```csharp
builder.Services
    .AddConfigR(options =>
    {
        options.CacheDuration = TimeSpan.FromMinutes(5);
    })
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### Configurar Serialização

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

## 🔍 Troubleshooting

### Erro: "Connection refused"

- Verifique se o banco de dados está rodando
- Confirme a connection string em `appsettings.json`
- Teste a conexão manualmente

### Erro: "Tabela não existe"

- Crie a tabela manualmente (veja seu provider)
- Ou configure `AutoCreateTable = true` (se suportado)

### Erro: "Serialização falhou"

- Verifique se sua classe tem um construtor sem parâmetros
- Confirme que as propriedades são públicas
- Use tipos simples ou complexos (JSON-serializable)

## 📚 Próximos Passos

- 🧱 [Aprenda sobre Scopes](advanced/scopes.md)
- 🚀 [Otimize com Cache](advanced/caching.md)
- 🧩 [Escolha um Provider](storage/sql-server.md)