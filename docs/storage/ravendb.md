# RavenDB Provider

Uso do provider RavenDB no ConfigR.

---

## âœ… InstalaÃ§Ã£o

```bash
dotnet add package ConfigR.RavenDB
```

---

## âš™ï¸ ConfiguraÃ§Ã£o

### Registrar no DI

```csharp
builder.Services
    .AddConfigR()
    .UseRavenDb(new[] { "http://localhost:8080" }, "ConfigR");
```

### appsettings.json

```json
{
  "RavenDB": {
    "Urls": [ "http://localhost:8080" ],
    "Database": "ConfigR"
  }
}
```

---

## ðŸ—„ Estrutura de Documentos

O RavenDB armazena cada chave de configuraÃ§Ã£o como um documento:

```json
{
  "Id": "ConfigR/__default__/checkout.loginrequired",
  "Key": "checkout.loginrequired",
  "Value": "{\"loginRequired\":true}",
  "Scope": null
}
```

- **Id**: `{KeyPrefix}/{scope||__default__}/{key}`
- **Key**: Chave formatada do ConfigR
- **Value**: JSON serializado
- **Scope**: Escopo opcional (null para global)

---

## ðŸ”§ OpÃ§Ãµes de ConfiguraÃ§Ã£o

```csharp
var documentStore = new DocumentStore
{
    Urls = new[] { "http://localhost:8080" },
    Database = "ConfigR"
};
documentStore.Initialize();

var options = Options.Create(new RavenDbConfigStoreOptions
{
    Urls = new[] { "http://localhost:8080" },
    Database = "ConfigR",
    KeyPrefix = "configr"
});
```

---

## ðŸš€ Exemplo Completo

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
    .UseRavenDb(new[] { "http://localhost:8080" }, "ConfigR");

// Uso em controller/service
var checkout = await _configR.GetAsync<CheckoutConfig>();

checkout.MaxItems = 50;
await _configR.SaveAsync(checkout);
```

---

## ðŸ§ª Testes

O provider RavenDB possui testes de ConfigStore, integraÃ§Ã£o e concorrÃªncia.

```bash
docker run -d --name ravendb-configr \
  -p 8080:8080 \
  -e RAVEN_Setup_Mode=None \
  -e RAVEN_License_Eula_Accepted=true \
  -e RAVEN_Security_UnsecuredAccessAllowed=PublicNetwork \
  ravendb/ravendb:6.0-ubuntu-latest

dotnet test

docker stop ravendb-configr && docker rm ravendb-configr
```

---

## ðŸ“ˆ ConsideraÃ§Ãµes de Performance

- Armazenamento orientado a documentos, ideal para configuraÃ§Ãµes complexas
- IDs Ãºnicos por chave/escopo evitam duplicidade
- Consultas usam `WaitForNonStaleResults` para consistÃªncia imediata nos testes
- Cache em memÃ³ria do ConfigR reduz leituras no cluster
- Configure rÃ©plicas do RavenDB para alta disponibilidade
