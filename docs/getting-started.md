# InÃ­cio RÃ¡pido

Comece a usar o ConfigR em menos de 5 minutos.

## ðŸ“¦ PrÃ©-requisitos

- .NET 8.0 ou superior
- Um banco de dados (SQL Server, MySQL, PostgreSQL, MongoDB, Redis ou RavenDB)

## 1ï¸âƒ£ InstalaÃ§Ã£o

Instale o pacote core e o provider desejado:

```bash
# Core
dotnet add package ConfigR.Core

# Escolha um provider (exemplo: SQL Server)
dotnet add package ConfigR.SqlServer
```

**Outros providers disponÃ­veis:**

```bash
dotnet add package ConfigR.MySql
dotnet add package ConfigR.Npgsql          # PostgreSQL
dotnet add package ConfigR.MongoDB
dotnet add package ConfigR.Redis
dotnet add package ConfigR.RavenDB
```

## 2ï¸âƒ£ Defina sua Classe de ConfiguraÃ§Ã£o

Crie uma classe POCO (Plain Old C# Object) com suas configuraÃ§Ãµes:

```csharp
public sealed class CheckoutConfig
{
    public bool LoginRequired { get; set; } = true;
    public int MaxItems { get; set; } = 20;
    public decimal MaxAmount { get; set; } = 10000m;
}
```

## 3ï¸âƒ£ Registre no Container DI

Configure o ConfigR no seu `Program.cs`:

```csharp
// SQL Server (exemplo padrÃ£o)
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### ConfiguraÃ§Ã£o do appsettings.json

```json
{
  "ConnectionStrings": {
    "ConfigR": "Server=localhost,1433;Database=configr;User Id=sa;Password=Pass@123;TrustServerCertificate=True;"
  }
}
```

## 4ï¸âƒ£ Use em seu CÃ³digo

Injete o `IConfigR` e use:

### Ler ConfiguraÃ§Ã£o

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

### Atualizar ConfiguraÃ§Ã£o

```csharp
public async Task UpdateCheckoutConfig()
{
    var config = await _configR.GetAsync<CheckoutConfig>();
    
    config.LoginRequired = false;
    config.MaxItems = 50;
    
    await _configR.SaveAsync(config);
}
```

## 5ï¸âƒ£ Crie a Tabela (SQL Server)

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
    Alguns providers como SQL Server suportam criaÃ§Ã£o automÃ¡tica da tabela se vocÃª configurar `AutoCreateTable = true`.

## ðŸŽ‰ Pronto!

VocÃª estÃ¡ pronto para usar o ConfigR! 

### PrÃ³ximos Passos

- ðŸ“š [Entenda a ConfiguraÃ§Ã£o](configuration.md) - Explore todas as opÃ§Ãµes
- ðŸ§© [Escolha um Provider](../storage/sql-server.md) - Veja detalhes de cada backend
- ðŸ§± [Aprenda sobre Scopes](../advanced/scopes.md) - Configure isolamento multi-tenant
- ðŸ’¡ [Explore Conceitos AvanÃ§ados](../advanced/caching.md) - Otimize sua aplicaÃ§Ã£o

## â“ DÃºvidas Comuns

### P: Posso usar mÃºltiplas classes de configuraÃ§Ã£o?

**R:** Sim! VocÃª pode ter quantas classes de configuraÃ§Ã£o precisar. Cada uma Ã© armazenada com sua prÃ³pria chave.

```csharp
var checkout = await _configR.GetAsync<CheckoutConfig>();
var shipping = await _configR.GetAsync<ShippingConfig>();
```

### P: Como funciona o cache?

**R:** ConfigR caches automaticamente as configuraÃ§Ãµes em memÃ³ria. AlteraÃ§Ãµes sÃ£o refletidas imediatamente apÃ³s `SaveAsync()`.

### P: Posso usar Scopes?

**R:** Sim! Scopes sÃ£o perfeitos para multi-tenant. Veja [Scopes](../advanced/scopes.md) para detalhes.

### P: Qual provider devo escolher?

**R:** 
- **SQL Server**: Recomendado para a maioria dos casos
- **PostgreSQL**: Se vocÃª jÃ¡ usa PostgreSQL
- **MySQL**: Leveza e compatibilidade
- **MongoDB**: Se vocÃª prefere bancos NoSQL
- **Redis**: Para cache de altÃ­ssima performance
- **RavenDB**: Para ACID completo e replicaÃ§Ã£o