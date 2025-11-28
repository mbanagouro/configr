# MongoDB Provider

Uso do provider MongoDB no ConfigR.

---

## 🚀 Instalação

```bash
dotnet add package ConfigR.MongoDB
```

---

## 🔧 Configuração

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

## 📊 Estrutura da Coleção

O MongoDB armazena as configurações em uma coleção chamada `configr`:

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

- **_id**: Identificador único do documento (ObjectId)
- **key**: Chave da configuração
- **value**: Valor da configuração (JSON stringificado)
- **scope**: Escopo opcional para multi-tenant
- **createdAt**: Data de criação do documento
- **updatedAt**: Data da última atualização

---

## ⚙️ Opções de Configuração

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

## 🧪 Testes

O provider MongoDB possui testes completos incluindo:

- **ConfigStoreTests**: Testes de CRUD básico e scopes
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

## 💡 Considerações de Performance

- MongoDB é excelente para documentos aninhados e JSON complexos
- Use scopes para isolamento multi-tenant
- Cache em memória (ConfigR.Core) reduz queries ao banco
- Índices automáticos em `(key, scope)` para performance
- Ideal para configurações dinâmicas e evolutivas
- Suporta replicação e alta disponibilidade nativa
- Considere usar TTL (Time To Live) para expiração automática de configurações
