# RavenDB Provider

Uso do provider RavenDB no ConfigR.

---

## ✅ Instalação

```bash
dotnet add package ConfigR.RavenDB
```

---

## ⚙️ Configuração

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

## 🗄 Estrutura de Documentos

O RavenDB armazena cada chave de configuração como um documento:

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

## 🔧 Opções de Configuração

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

## 🚀 Exemplo Completo

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
    .UseRavenDb(new[] { "http://localhost:8080" }, "ConfigR");

// Uso em controller/service
var checkout = await _configR.GetAsync<CheckoutConfig>();

checkout.MaxItems = 50;
await _configR.SaveAsync(checkout);
```

---

## 🧪 Testes

O provider RavenDB possui testes de ConfigStore, integração e concorrência.

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

## 📈 Considerações de Performance

- Armazenamento orientado a documentos, ideal para configurações complexas
- IDs únicos por chave/escopo evitam duplicidade
- Consultas usam `WaitForNonStaleResults` para consistência imediata nos testes
- Cache em memória do ConfigR reduz leituras no cluster
- Configure réplicas do RavenDB para alta disponibilidade
