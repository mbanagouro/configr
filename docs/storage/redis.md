# Redis Provider

Uso do provider Redis no ConfigR.

---

## ðŸš€ InstalaÃ§Ã£o

```bash
dotnet add package ConfigR.Redis
```

---

## ðŸ”§ ConfiguraÃ§Ã£o

### Registrar no DI

```csharp
builder.Services
    .AddConfigR()
    .UseRedis("localhost:6379");
```

### appsettings.json

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

---

## ðŸ“Š Estrutura de Armazenamento

O Redis armazena as configuraÃ§Ãµes como strings serializadas em JSON:

```
Key (no Redis): "ConfigR:checkout.loginrequired:null"
Value: "{\"loginRequired\":true}"

Key (no Redis): "ConfigR:checkout.loginrequired:loja-1"
Value: "{\"loginRequired\":false}"
```

### PadrÃ£o de Chave

```
ConfigR:{config-key}:{scope}
```

- **ConfigR**: Prefixo padrÃ£o
- **{config-key}**: Chave da configuraÃ§Ã£o
- **{scope}**: Escopo (ou "null" se sem escopo)

---

## âš™ï¸ OpÃ§Ãµes de ConfiguraÃ§Ã£o

```csharp
var options = Options.Create(new RedisConfigStoreOptions
{
    ConnectionString = "localhost:6379",
    KeyPrefix = "ConfigR",    // Prefixo das chaves (padrÃ£o: "ConfigR")
    ExpirationMinutes = null  // TTL em minutos (null = sem expiraÃ§Ã£o)
});

var store = new RedisConfigStore(options);
```

---

## ðŸ“ Exemplo Completo

```csharp
// Classe de configuraÃ§Ã£o
public sealed class CheckoutConfig
{
    public bool LoginRequired { get; set; } = true;
    public int MaxItems { get; set; } = 20;
}

// Program.cs
builder.Services
    .AddConfigR()
    .UseRedis("localhost:6379");

// Uso em controller/service
var checkout = await _configR.GetAsync<CheckoutConfig>();

if (checkout.LoginRequired)
{
    // ...
}

checkout.MaxItems = 50;
await _configR.SaveAsync(checkout);
```

---

## ðŸ§ª Testes

O provider Redis possui testes completos incluindo:

- **ConfigStoreTests**: Testes de CRUD bÃ¡sico e scopes
- **IntegrationTests**: Testes de fluxo completo com tipos complexos
- **ConcurrencyTests**: Testes de leitura/escrita paralela

Para executar os testes do Redis:

```bash
# Iniciar container Redis
docker run -d --name redis-configr \
  -p 6379:6379 redis:7

# Executar testes
dotnet test

# Limpar
docker stop redis-configr && docker rm redis-configr
```

---

## ðŸ’¡ ConsideraÃ§Ãµes de Performance

- Redis Ã© extremamente rÃ¡pido para leitura/escrita (em memÃ³ria)
- Ideal para configuraÃ§Ãµes com alta frequÃªncia de acesso
- Use scopes para isolamento multi-tenant
- Cache duplo: Redis (persistÃªncia) + MemoryCache (aplicaÃ§Ã£o)
- TTL automÃ¡tico para limpeza de chaves antigas
- Suporta pub/sub para notificaÃ§Ãµes de mudanÃ§as
- Considere usar Redis Cluster para alta disponibilidade
- âš ï¸ Dados residem em memÃ³ria - nÃ£o ideal para grandes volumes
