# Início Rápido

Comece a usar o ConfigR em menos de 5 minutos.

## 📦 Pré-requisitos

- .NET 8.0 ou superior
- Um banco de dados (SQL Server, MySQL, PostgreSQL, MongoDB, Redis ou RavenDB)

## 1️⃣ Instalação

Instale o pacote core e o provider desejado:

```bash
# Core
dotnet add package ConfigR.Core

# Escolha um provider (exemplo: SQL Server)
dotnet add package ConfigR.SqlServer
```

**Outros providers disponíveis:**

```bash
dotnet add package ConfigR.MySql
dotnet add package ConfigR.Npgsql          # PostgreSQL
dotnet add package ConfigR.MongoDB
dotnet add package ConfigR.Redis
dotnet add package ConfigR.RavenDB
```

## 2️⃣ Defina sua Classe de Configuração

Crie uma classe POCO (Plain Old C# Object) com suas configurações:

```csharp
public sealed class CheckoutConfig
{
    public bool LoginRequired { get; set; } = true;
    public int MaxItems { get; set; } = 20;
    public decimal MaxAmount { get; set; } = 10000m;
}
```

## 3️⃣ Registre no Container DI

Configure o ConfigR no seu `Program.cs`:

```csharp
// SQL Server (exemplo padrão)
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### Configuração do appsettings.json

```json
{
  "ConnectionStrings": {
    "ConfigR": "Server=localhost,1433;Database=configr;User Id=sa;Password=Pass@123;TrustServerCertificate=True;"
  }
}
```

## 4️⃣ Use em seu Código

Injete o `IConfigR` e use:

### Ler Configuração

```csharp
public class CheckoutService
{
    private readonly IConfigR _configR;

    public CheckoutService(IConfigR configR)
    {
        _configR = configR;
    }

    public async Task<bool> CanCheckout()
    {
        var config = await _configR.GetAsync<CheckoutConfig>();
        return !config.LoginRequired || User.IsAuthenticated;
    }
}
```

### Atualizar Configuração

```csharp
public async Task UpdateCheckoutConfig()
{
    var config = await _configR.GetAsync<CheckoutConfig>();
    
    config.LoginRequired = false;
    config.MaxItems = 50;
    
    await _configR.SaveAsync(config);
}
```

## 5️⃣ Crie a Tabela (SQL Server)

Execute o script SQL para criar a tabela:

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

!!! tip "Dica"
    Alguns providers como SQL Server suportam criação automática da tabela se você configurar `AutoCreateTable = true`.

## 🎉 Pronto!

Você está pronto para usar o ConfigR! 

### Próximos Passos

- 📚 [Entenda a Configuração](configuration.md) - Explore todas as opções
- 🧩 [Escolha um Provider](../storage/sql-server.md) - Veja detalhes de cada backend
- 🧱 [Aprenda sobre Scopes](../advanced/scopes.md) - Configure isolamento multi-tenant
- 💡 [Explore Conceitos Avançados](../advanced/caching.md) - Otimize sua aplicação

## ❓ Dúvidas Comuns

### P: Posso usar múltiplas classes de configuração?

**R:** Sim! Você pode ter quantas classes de configuração precisar. Cada uma é armazenada com sua própria chave.

```csharp
var checkout = await _configR.GetAsync<CheckoutConfig>();
var shipping = await _configR.GetAsync<ShippingConfig>();
```

### P: Como funciona o cache?

**R:** ConfigR caches automaticamente as configurações em memória. Alterações são refletidas imediatamente após `SaveAsync()`.

### P: Posso usar Scopes?

**R:** Sim! Scopes são perfeitos para multi-tenant. Veja [Scopes](../advanced/scopes.md) para detalhes.

### P: Qual provider devo escolher?

**R:** 
- **SQL Server**: Recomendado para a maioria dos casos
- **PostgreSQL**: Se você já usa PostgreSQL
- **MySQL**: Leveza e compatibilidade
- **MongoDB**: Se você prefere bancos NoSQL
- **Redis**: Para cache de altíssima performance
- **RavenDB**: Para ACID completo e replicação