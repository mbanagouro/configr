## ConfigR — Strongly typed runtime configuration for modern .NET apps.

ConfigR é uma biblioteca leve e extensível para gerenciamento de configurações tipadas em aplicações .NET.

Ele permite salvar e carregar configurações em tempo de execução usando reflection mínima, cache inteligente e providers pluggáveis de armazenamento — começando por SQL Server (ADO.NET puro), com suporte planejado para Redis, arquivos e muito mais.

Ideal para sistemas multi-tenant, backoffices, módulos configuráveis e aplicações que precisam evitar dependência de appsettings.json para ajustes dinâmicos.

API simples, forte tipagem, zero mágica, alta performance.

## Pacotes

- `ConfigR.Abstractions`
- `ConfigR.Core`
- `ConfigR.SqlServer`

## Instalação

```bash
dotnet add package ConfigR.Core
dotnet add package ConfigR.SqlServer
```

## Registro via DI

```csharp
builder.Services
    .AddConfigR(options =>
    {
        options.DefaultScope = "Loja-123"; // opcional
    })
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"), opt =>
    {
        opt.AutoCreateTable = true;
        opt.Schema = "dbo";
        opt.Table = "Configuracoes";
    });
```

## Uso

```csharp
public sealed class CheckoutConfig
{
    public bool LoginRequired { get; set; } = true;
    public int MaxItemsPerOrder { get; set; } = 50;
}

public sealed class CheckoutService
{
    private readonly IConfigR _configR;

    public CheckoutService(IConfigR configR)
    {
        _configR = configR;
    }

    public async Task ProcessarAsync()
    {
        var checkoutConfig = await _configR.GetAsync<CheckoutConfig>();

        if (checkoutConfig.LoginRequired)
        {
            // ...
        }
    }

    public async Task AtualizarAsync()
    {
        var checkoutConfig = await _configR.GetAsync<CheckoutConfig>();

        checkoutConfig.LoginRequired = false;

        await _configR.SaveAsync(checkoutConfig);
    }
}
```

## Estrutura da tabela SQL Server

```sql
CREATE TABLE [dbo].[ConfigR] (
    [Id]    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Key]   NVARCHAR(256) NOT NULL,
    [Value] NVARCHAR(MAX) NOT NULL,
    [Scope] NVARCHAR(128) NULL
);

CREATE UNIQUE INDEX IX_ConfigR_Key_Scope
    ON [dbo].[ConfigR] ([Key], [Scope]);
```
