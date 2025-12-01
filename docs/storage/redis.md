# Redis Provider

Uso do provider Redis no ConfigR.

---

## 🚀 Instalação

```bash
dotnet add package ConfigR.Redis
```

---

## 🔧 Configuração

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

## 📊 Estrutura de Armazenamento

O Redis armazena as configurações como strings serializadas em JSON:

```
Key (no Redis): "ConfigR:checkout.loginrequired:null"
Value: "{\"loginRequired\":true}"

Key (no Redis): "ConfigR:checkout.loginrequired:loja-1"
Value: "{\"loginRequired\":false}"
```

### Padrão de Chave

```
ConfigR:{config-key}:{scope}
```

- **ConfigR**: Prefixo padrão
- **{config-key}**: Chave da configuração
- **{scope}**: Escopo (ou "null" se sem escopo)

---

## ⚙️ Opções de Configuração

```csharp
var options = Options.Create(new RedisConfigStoreOptions
{
    ConnectionString = "localhost:6379",
    KeyPrefix = "ConfigR",    // Prefixo das chaves (padrão: "ConfigR")
    ExpirationMinutes = null  // TTL em minutos (null = sem expiração)
});

var store = new RedisConfigStore(options);
```

---

## 📝 Exemplo Completo

```csharp
// Classe de configuração
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

## 🧪 Testes

O provider Redis possui testes completos incluindo:

- **ConfigStoreTests**: Testes de CRUD básico e scopes
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

## 💡 Considerações de Performance

- Redis é extremamente rápido para leitura/escrita (em memória)
- Ideal para configurações com alta frequência de acesso
- Use scopes para isolamento multi-tenant
- Cache duplo: Redis (persistência) + MemoryCache (aplicação)
- TTL automático para limpeza de chaves antigas
- Suporta pub/sub para notificações de mudanças
- Considere usar Redis Cluster para alta disponibilidade
- ⚠️ Dados residem em memória - não ideal para grandes volumes
