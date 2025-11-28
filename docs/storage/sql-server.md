# SQL Server Provider

Uso do provider SQL Server no ConfigR.

---

## 🚀 Instalação

```bash
dotnet add package ConfigR.SqlServer
```

---

## 🔧 Configuração

### Registrar no DI

```csharp
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "ConfigR": "Server=localhost,1433;Database=configr;User Id=sa;Password=Pass@123;TrustServerCertificate=True;"
  }
}
```

---

## 📊 Estrutura da Tabela

```sql
CREATE TABLE [dbo].[ConfigR] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Key] NVARCHAR(256) NOT NULL,
    [Value] NVARCHAR(MAX) NOT NULL,
    [Scope] NVARCHAR(128) NULL
);

CREATE UNIQUE INDEX IX_ConfigR_Key_Scope
    ON [dbo].[ConfigR] ([Key], [Scope]);
```

### Campos

- **Id**: Identificador único com auto-incremento
- **Key**: Chave da configuração (até 256 caracteres)
- **Value**: Valor da configuração (suporta até 2GB com NVARCHAR(MAX))
- **Scope**: Escopo opcional para multi-tenant (até 128 caracteres)

---

## ⚙️ Opções de Configuração

```csharp
var options = Options.Create(new SqlServerConfigStoreOptions
{
    ConnectionString = "Server=localhost,1433;Database=configr;User Id=sa;Password=Pass@123;TrustServerCertificate=True;",
    Schema = "dbo",           // Schema do banco de dados
    Table = "ConfigR",        // Nome da tabela
    AutoCreateTable = true    // Criar tabela automaticamente
});

var store = new SqlServerConfigStore(options);
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
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));

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

O provider SQL Server possui testes completos incluindo:

- **ConfigStoreTests**: Testes de CRUD básico e scopes
- **IntegrationTests**: Testes de fluxo completo com tipos complexos
- **ConcurrencyTests**: Testes de leitura/escrita paralela

Para executar os testes do SQL Server:

```bash
# Iniciar container SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@123" \
  -p 1433:1433 --name sqlserver-configr -d mcr.microsoft.com/mssql/server:2022-latest

# Executar testes
dotnet test

# Limpar
docker stop sqlserver-configr && docker rm sqlserver-configr
```

---

## 💡 Considerações de Performance

- Índice único em `(Key, Scope)` garante integridade e performance
- Suporta valores muito grandes com `NVARCHAR(MAX)`
- Use scopes para isolamento multi-tenant
- Cache em memória (ConfigR.Core) reduz queries ao banco
- Índices automáticos para buscas rápidas
