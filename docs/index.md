# ðŸš€ ConfigR - DocumentaÃ§Ã£o

Bem-vindo Ã  documentaÃ§Ã£o do **ConfigR**, uma biblioteca leve, extensÃ­vel e altamente performÃ¡tica para **configuraÃ§Ãµes tipadas em runtime** em aplicaÃ§Ãµes .NET.

## ðŸ“š NavegaÃ§Ã£o RÃ¡pida

### âš¡ ComeÃ§ar RÃ¡pido

- **[InÃ­cio RÃ¡pido](getting-started.md)** - Instale e configure em 5 minutos
- **[ConfiguraÃ§Ã£o](configuration.md)** - Configure o ConfigR no seu projeto

### ðŸ—„ï¸ Providers de Armazenamento

Escolha o backend que melhor se encaixa na sua infraestrutura:

- **[SQL Server](storage/sql-server.md)** - PadrÃ£o, recomendado para a maioria dos casos
- **[MySQL](storage/mysql.md)** - Alternativa leve e popular
- **[PostgreSQL (Npgsql)](storage/npgsql.md)** - Open source e robusto
- **[MongoDB](storage/mongodb.md)** - Banco de dados NoSQL
- **[Redis](storage/redis.md)** - Cache em memÃ³ria de alta performance
- **[RavenDB](storage/ravendb.md)** - Banco de dados multi-modelo

### ðŸŽ¯ Conceitos AvanÃ§ados

- **[Scopes](advanced/scopes.md)** - Multi-tenant e isolamento de configuraÃ§Ãµes
- **[Cache](advanced/caching.md)** - Entenda como o cache em memÃ³ria funciona
- **[Extensibilidade](advanced/extensibility.md)** - Crie providers personalizados

### ðŸ§ª Testes e Qualidade

- **[Testes](testing.md)** - Como testar sua aplicaÃ§Ã£o com ConfigR
- **[API Reference](api-reference.md)** - DocumentaÃ§Ã£o completa da API

---

## ðŸŽ¯ Para que serve o ConfigR?

### âœ… Ideal para:

- **Backoffices configurÃ¡veis** - Altere comportamentos sem deploy
- **Plataformas multi-tenant** - Isolamento de configuraÃ§Ãµes por cliente
- **Feature flags** - Ative/desative funcionalidades dinamicamente
- **Sistemas evolutivos** - ConfiguraÃ§Ãµes que mudam em runtime
- **E-commerce, ERP, SaaS** - Plataformas que precisam de flexibilidade

### ðŸŽ CaracterÃ­sticas Principais

| CaracterÃ­stica | BenefÃ­cio |
|---|---|
| ðŸ”¥ Tipagem Forte | Type-safe, sem magic strings |
| ðŸš€ Cache em MemÃ³ria | Performance otimizada |
| ðŸ§© Providers PlugÃ¡veis | Escolha o backend desejado |
| ðŸ§± Multi-tenant | Isolamento de dados por escopo |
| ðŸ”§ CustomizÃ¡vel | SerializaÃ§Ã£o e comportamento flexÃ­veis |
| ðŸ§  Simples | Zero reflection pesada ou mÃ¡gica |

---

## ðŸš€ ComeÃ§ar em 1 Minuto

### 1. Instale

```bash
dotnet add package ConfigR.Core
dotnet add package ConfigR.SqlServer
```

### 2. Configure

```csharp
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### 3. Use

```csharp
public sealed class MeuConfig
{
    public string Valor { get; set; } = "padrÃ£o";
}

// Ler
var config = await configR.GetAsync<MeuConfig>();

// Atualizar
config.Valor = "novo";
await configR.SaveAsync(config);
```

---

## ðŸ“– PrÃ³ximos Passos

- ðŸ‘‰ **Comece agora**: [Guia de InÃ­cio RÃ¡pido](getting-started.md)
- ðŸ”§ **Configure seu provider**: [ConfiguraÃ§Ã£o](configuration.md)
- ðŸ“š **Aprenda conceitos avanÃ§ados**: [Scopes](advanced/scopes.md)