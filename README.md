<p align="center">
  <h1 align="center">ConfigR</h1>
  <p align="center">ConfiguraÃ§Ã£o tipada em runtime para aplicaÃ§Ãµes .NET modernas</p>

  <p align="center">
    <img src="https://img.shields.io/badge/.NET-8.0+-blueviolet" />
    <img src="https://img.shields.io/badge/license-MIT-green.svg" />
    <img src="https://github.com/mbanagouro/configr/actions/workflows/ci-cd.yml/badge.svg" />
    <img src="https://github.com/mbanagouro/configr/actions/workflows/docs.yml/badge.svg" />
    <a href="https://www.nuget.org/packages/ConfigR.Core">
      <img src="https://img.shields.io/nuget/v/ConfigR.Core.svg" />
    </a>
    <a href="https://github.com/mbanagouro/configr">
      <img src="https://img.shields.io/github/last-commit/mbanagouro/configr" />
    </a>
  </p>
</p>

---

## ðŸš€ O que Ã© ConfigR?

**ConfigR** Ã© uma biblioteca leve, extensÃ­vel e altamente performÃ¡tica para **configuraÃ§Ãµes tipadas em runtime** em aplicaÃ§Ãµes .NET.

### Principais CaracterÃ­sticas

- ðŸ”¥ **Tipagem forte** - Classes de configuraÃ§Ã£o type-safe
- ðŸš€ **Cache em memÃ³ria** - OtimizaÃ§Ã£o de performance integrada
- ðŸ§© **Providers plugÃ¡veis** - MÃºltiplos opÃ§Ãµes de armazenamento
- ðŸ§± **Scopes multi-tenant** - ConfiguraÃ§Ãµes isoladas por tenant
- ðŸ”§ **SerializaÃ§Ã£o customizÃ¡vel** - FormataÃ§Ã£o flexÃ­vel de dados
- ðŸ§  **Zero mÃ¡gica** - Sem reflection pesada ou comportamentos implÃ­citos

### Casos de Uso

- Backoffices configurÃ¡veis
- Plataformas multi-tenant
- Feature flags
- Sistemas com configuraÃ§Ãµes dinÃ¢micas
- Substituir `appsettings.json` para configuraÃ§Ãµes em runtime
- E-commerce, ERP e plataformas SaaS

---

## ðŸ“¦ InstalaÃ§Ã£o

Instale o pacote core e o provider de armazenamento desejado:

```bash
dotnet add package ConfigR.Core
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

---

## ðŸš€ InÃ­cio RÃ¡pido

### 1. Defina sua classe de configuraÃ§Ã£o

```csharp
public sealed class CheckoutConfig
{
    public bool LoginRequired { get; set; } = true;
    public int MaxItems { get; set; } = 20;
}
```

### 2. Registre o ConfigR no container DI (exemplo com SQL Server)

```csharp
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### 3. Leia a configuraÃ§Ã£o tipada

```csharp
var checkout = await configR.GetAsync<CheckoutConfig>();

if (checkout.LoginRequired)
{
    // Sua lÃ³gica aqui
}
```

### 4. Atualize em runtime

```csharp
checkout.LoginRequired = false;
await configR.SaveAsync(checkout);
```

---

## ðŸ—„ï¸ Providers de Armazenamento

O ConfigR suporta mÃºltiplos backends de armazenamento. Escolha o que melhor se encaixa na sua infraestrutura:

| Provider | Pacote | Status |
|----------|--------|--------|
| SQL Server | `ConfigR.SqlServer` | âœ… Pronto |
| MySQL | `ConfigR.MySql` | âœ… Pronto |
| PostgreSQL | `ConfigR.Npgsql` | âœ… Pronto |
| MongoDB | `ConfigR.MongoDB` | âœ… Pronto |
| Redis | `ConfigR.Redis` | âœ… Pronto |
| RavenDB | `ConfigR.RavenDB` | âœ… Pronto |

Para instruÃ§Ãµes de configuraÃ§Ã£o especÃ­ficas de cada provider, consulte a [documentaÃ§Ã£o oficial](https://mbanagouro.github.io/configr).

---

## ðŸ—„ï¸ ConfiguraÃ§Ã£o SQL Server (Exemplo PadrÃ£o)

### 1. Crie a tabela de configuraÃ§Ã£o

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

### 2. Configure na sua aplicaÃ§Ã£o

```csharp
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

---

## ðŸ—ï¸ Arquitetura

```
ConfigR.Abstractions  â†’ Interfaces e contratos base
ConfigR.Core          â†’ ImplementaÃ§Ã£o core (cache, serializaÃ§Ã£o, DI, formataÃ§Ã£o de chaves)
ConfigR.SqlServer     â†’ Provider SQL Server
ConfigR.MySql         â†’ Provider MySQL
ConfigR.Npgsql        â†’ Provider PostgreSQL
ConfigR.MongoDB       â†’ Provider MongoDB
ConfigR.Redis         â†’ Provider Redis
ConfigR.RavenDB       â†’ Provider RavenDB
```

---

## ðŸ“– DocumentaÃ§Ã£o

DocumentaÃ§Ã£o completa e guias disponÃ­veis em:

ðŸ‘‰ **https://mbanagouro.github.io/configr**

---

## ðŸ§ª Testes

### InÃ­cio RÃ¡pido com Docker Compose

```bash
# Clone o repositÃ³rio
git clone https://github.com/mbanagouro/configr.git
cd configr

# Inicie todos os serviÃ§os (SQL Server, MySQL, PostgreSQL, MongoDB, Redis, RavenDB)
docker-compose up -d

# Aguarde ~30 segundos para os serviÃ§os ficarem prontos

# Execute todos os testes
dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj

# Pare os serviÃ§os
docker-compose down
```

### Scripts Auxiliares

**Windows:**
```bash
test-all.bat up              # Iniciar serviÃ§os
test-all.bat test            # Executar todos os testes
test-all.bat test-sql        # Executar apenas testes SQL Server
test-all.bat test-mysql      # Executar apenas testes MySQL
test-all.bat test-postgres   # Executar apenas testes PostgreSQL
test-all.bat test-mongo      # Executar apenas testes MongoDB
test-all.bat test-redis      # Executar apenas testes Redis
test-all.bat test-raven      # Executar apenas testes RavenDB
test-all.bat down            # Parar serviÃ§os
test-all.bat clean           # Limpar tudo
```

**Linux/macOS:**
```bash
./test-all.sh up             # Iniciar serviÃ§os
./test-all.sh test           # Executar todos os testes
./test-all.sh test-sql       # Executar apenas testes SQL Server
./test-all.sh test-mysql     # Executar apenas testes MySQL
./test-all.sh test-postgres  # Executar apenas testes PostgreSQL
./test-all.sh test-mongo     # Executar apenas testes MongoDB
./test-all.sh test-redis     # Executar apenas testes Redis
./test-all.sh test-raven     # Executar apenas testes RavenDB
./test-all.sh down           # Parar serviÃ§os
./test-all.sh clean          # Limpar tudo
```

Para testes manuais por provider, consulte [TESTING_GUIDE.md](TESTING_GUIDE.md).

---

## ðŸš€ CI/CD

- âœ… Build e testes (todos os providers)
- âœ… PublicaÃ§Ã£o automÃ¡tica no NuGet em novas releases
- âœ… Deploy automÃ¡tico da documentaÃ§Ã£o

---

## ðŸ¤ ContribuiÃ§Ã£o

1. FaÃ§a um fork do repositÃ³rio
2. Crie uma branch para sua feature: `git checkout -b feature/minha-feature`
3. Commit suas mudanÃ§as: `git commit -am 'Adicionar minha feature'`
4. Push para a branch: `git push origin feature/minha-feature`
5. Abra um pull request
6. Certifique-se de que todos os testes passam âœ”

---

## ðŸ“„ LicenÃ§a

MIT License - consulte o arquivo [LICENSE](LICENSE) para detalhes.

---

## ðŸ‘¨â€ðŸ’» Autor

**Michel Banagouro**  
CTO na Leanwork Â· Arquiteto e Especialista em ASP.NET

- GitHub: https://github.com/mbanagouro
- YouTube: https://youtube.com/@aspnetpro