# PostgreSQL Provider (Npgsql)

Uso do provider PostgreSQL (Npgsql) no ConfigR.

---

## 🚀 Instalação

```bash
dotnet add package ConfigR.Npgsql
```

---

## 🔧 Configuração

### Registrar no DI

```csharp
builder.Services
    .AddConfigR()
    .UseNpgsql(builder.Configuration.GetConnectionString("ConfigR"));
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "ConfigR": "Host=localhost;Port=5432;Database=configr;Username=postgres;Password=postgres;"
  }
}
```

---

## 📊 Estrutura da Tabela

```sql
CREATE SCHEMA IF NOT EXISTS public;

CREATE TABLE IF NOT EXISTS public.configr (
    id SERIAL PRIMARY KEY,
    key TEXT NOT NULL,
    value TEXT NOT NULL,
    scope TEXT NULL,
    UNIQUE(key, scope)
);
```

### Campos

- **id**: Identificador único com auto-incremento (SERIAL)
- **key**: Chave da configuração (texto livre)
- **value**: Valor da configuração (texto livre, suporta JSON)
- **scope**: Escopo opcional para multi-tenant

---

## ⚙️ Opções de Configuração

```csharp
var options = Options.Create(new NpgsqlConfigStoreOptions
{
    ConnectionString = "Host=localhost;Port=5432;Database=configr;Username=postgres;Password=postgres;",
    Schema = "public",        // Schema do banco de dados
    Table = "configr",        // Nome da tabela
    AutoCreateTable = true    // Criar tabela automaticamente
});

var store = new NpgsqlConfigStore(options);
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
    .UseNpgsql(builder.Configuration.GetConnectionString("ConfigR"));

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

O provider PostgreSQL possui testes completos incluindo:

- **ConfigStoreTests**: Testes de CRUD básico e scopes
- **IntegrationTests**: Testes de fluxo completo com tipos complexos
- **ConcurrencyTests**: Testes de leitura/escrita paralela

Para executar os testes do PostgreSQL:

```bash
# Iniciar container PostgreSQL
docker run -d --name postgres-configr \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_DB=configr \
  -p 5432:5432 postgres:16

# Executar testes
dotnet test

# Limpar
docker stop postgres-configr && docker rm postgres-configr
```

---

## 💡 Considerações de Performance

- Índice único em `(key, scope)` garante integridade e performance
- Suporta valores muito grandes com `TEXT`
- Use scopes para isolamento multi-tenant
- Cache em memória (ConfigR.Core) reduz queries ao banco
- PostgreSQL é altamente otimizado para leitura/escrita paralela
- Considere usar `JSONB` para configurações complexas com `json_agg`
