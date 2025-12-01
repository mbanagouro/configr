# MongoDB Provider

Uso do provider MongoDB no ConfigR.

---

## ðŸš€ InstalaÃ§Ã£o

```bash
dotnet add package ConfigR.MongoDB
```

---

## ðŸ”§ ConfiguraÃ§Ã£o

### Registrar no DI

```csharp
builder.Services
    .AddConfigR()
    .UseMongoDb("mongodb://localhost:27017", "ConfigR");
```

### appsettings.json

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "ConfigR"
  }
}
```

---

## ðŸ“Š Estrutura da ColeÃ§Ã£o

O MongoDB armazena as configuraÃ§Ãµes em uma coleÃ§Ã£o chamada `configr`:

```json
{
  "_id": ObjectId("..."),
  "key": "checkout.loginrequired",
  "value": "{\"loginRequired\":true}",
  "scope": null,
  "createdAt": ISODate("2024-01-01T00:00:00Z"),
  "updatedAt": ISODate("2024-01-01T00:00:00Z")
}
```

### Campos

- **_id**: Identificador Ãºnico do documento (ObjectId)
- **key**: Chave da configuraÃ§Ã£o
- **value**: Valor da configuraÃ§Ã£o (JSON stringificado)
- **scope**: Escopo opcional para multi-tenant
- **createdAt**: Data de criaÃ§Ã£o do documento
- **updatedAt**: Data da Ãºltima atualizaÃ§Ã£o

---

## âš™ï¸ OpÃ§Ãµes de ConfiguraÃ§Ã£o

```csharp
var options = Options.Create(new MongoConfigStoreOptions
{
    ConnectionString = "mongodb://localhost:27017",
    DatabaseName = "ConfigR",
    CollectionName = "configr"
});

var store = new MongoConfigStore(options);
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
    .UseMongoDb("mongodb://localhost:27017", "ConfigR");

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

O provider MongoDB possui testes completos incluindo:

- **ConfigStoreTests**: Testes de CRUD bÃ¡sico e scopes
- **IntegrationTests**: Testes de fluxo completo com tipos complexos
- **ConcurrencyTests**: Testes de leitura/escrita paralela

Para executar os testes do MongoDB:

```bash
# Iniciar container MongoDB
docker run -d --name mongo-configr \
  -p 27017:27017 mongo:7

# Executar testes
dotnet test

# Limpar
docker stop mongo-configr && docker rm mongo-configr
```

---

## ðŸ’¡ ConsideraÃ§Ãµes de Performance

- MongoDB Ã© excelente para documentos aninhados e JSON complexos
- Use scopes para isolamento multi-tenant
- Cache em memÃ³ria (ConfigR.Core) reduz queries ao banco
- Ãndices automÃ¡ticos em `(key, scope)` para performance
- Ideal para configuraÃ§Ãµes dinÃ¢micas e evolutivas
- Suporta replicaÃ§Ã£o e alta disponibilidade nativa
- Considere usar TTL (Time To Live) para expiraÃ§Ã£o automÃ¡tica de configuraÃ§Ãµes
