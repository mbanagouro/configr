<p align="center">
  <h1 align="center">ConfigR</h1>
  <p align="center">Strongly-typed runtime configuration for modern .NET apps</p>

  <p align="center">
    <img src="https://img.shields.io/badge/.NET-10.0-blueviolet" />
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

## 🚀 O que é o ConfigR?

**ConfigR** é uma biblioteca leve, extensível e altamente performática para **configurações tipadas em runtime** em aplicações .NET.

Ele permite salvar e carregar configurações em tempo de execução usando:

- 🔥 Tipagem forte
- 🚀 Cache em memória integrado
- 🧩 Providers de armazenamento plugáveis
- 🧱 Scopes multi-tenant
- 🔧 Serialização customizável
- 🧠 Zero reflection pesada ou mágica

Ideal para:

- Backoffices configuráveis  
- Plataformas multi-loja  
- Feature flags  
- Sistemas que evoluem em runtime  
- Substituir appsettings.json para configurações dinâmicas  
- Ecommerces, ERPs, plataformas SaaS  

---

## 📦 Instalação

```bash
dotnet add package ConfigR.Core

dotnet add package ConfigR.SqlServer
dotnet add package ConfigR.MySql
dotnet add package ConfigR.Npgsql
dotnet add package ConfigR.MongoDB
dotnet add package ConfigR.Redis
dotnet add package ConfigR.RavenDB
```

---

## 🧱 Como funciona?

### 1. Crie sua classe de configuração

```csharp
public sealed class CheckoutConfig
{
    public bool LoginRequired { get; set; } = true;
    public int MaxItems { get; set; } = 20;
}
```

### 2. Registre o ConfigR no DI (Escolha o provider que deseja)

```csharp
// SQL Server
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));

// MySQL
builder.Services
    .AddConfigR()
    .UseMySql(builder.Configuration.GetConnectionString("ConfigR"));

// Npgsql
builder.Services
    .AddConfigR()
    .UseNpgsql(builder.Configuration.GetConnectionString("ConfigR"));

// MongoDB
builder.Services
    .AddConfigR()
    .UseMongoDb("mongodb://localhost:27017", "ConfigR");

// Redis
builder.Services
    .AddConfigR()
    .UseRedis("localhost:6379");

// RavenDB
builder.Services
    .AddConfigR()
    .UseRavenDb(new[] { "http://localhost:8080" }, "ConfigR");
```

### 3. Leia a configuração tipada

```csharp
var checkout = await _configR.GetAsync<CheckoutConfig>();

if (checkout.LoginRequired)
{
    // ...
}
```

### 4. Atualize em runtime

```csharp
checkout.LoginRequired = false;
await _configR.SaveAsync(checkout);
```

---

## 🧩 Providers de Armazenamento

| Provider | Pacote | Status | Docs |
|---------|--------|--------|------|
| SQL Server | ConfigR.SqlServer | ✅ Incluído | [📖](docs/storage/sql-server.md) |
| MySQL | ConfigR.MySql | ✅ Incluído | [📖](docs/storage/mysql.md) |
| PostgreSQL (Npgsql) | ConfigR.Npgsql | ✅ Incluído | [📖](docs/storage/npgsql.md) |
| MongoDB | ConfigR.MongoDB | ✅ Incluído | [📖](docs/storage/mongodb.md) |
| Redis | ConfigR.Redis | ✅ Incluído | [📖](docs/storage/redis.md) |
| RavenDB | ConfigR.RavenDB | Pronto | [docs/storage/ravendb.md](docs/storage/ravendb.md) |

---

## 🗄 Estrutura da Tabela (SQL Server)

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

---

## 🗄 Estrutura da Tabela (MySQL)

```sql
CREATE TABLE IF NOT EXISTS configr (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    cfg_key VARCHAR(255) NOT NULL,
    cfg_value TEXT NOT NULL,
    scope VARCHAR(255) NULL,
    UNIQUE KEY uk_config (cfg_key, scope)
);
```

---

## 🗄 Estrutura da Tabela (PostgreSQL)

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

---

## 🧠 Arquitetura do ConfigR

```
ConfigR.Abstractions  → Interfaces e contratos base
ConfigR.Core          → Implementação padrão (cache, serializer, DI, key formatter)
ConfigR.SqlServer     → Provider SQL Server
ConfigR.MySql         → Provider MySQL
ConfigR.Npgsql        → Provider PostgreSQL
ConfigR.MongoDB       → Provider MongoDB
ConfigR.Redis         → Provider Redis
ConfigR.RavenDB       → Provider RavenDB
```

---

## 📘 Documentação Oficial

Disponível em:

👉 **https://mbanagouro.github.io/configr**

### Guias Rápidos

- 🚀 [Iniciando](https://mbanagouro.github.io/configr/getting-started/)
- 🔧 [Configuração](https://mbanagouro.github.io/configr/configuration/)
- 🧪 [Testes](TESTING_GUIDE.md) - Guia completo com Docker Compose
- 📚 [API Reference](https://mbanagouro.github.io/configr/api-reference/)

---

## 🧪 Testes

### Quickstart com Docker Compose

```bash
# Clone e entre na pasta
git clone https://github.com/mbanagouro/configr.git
cd configr

# Inicie todos os serviços (SQL Server, MySQL, PostgreSQL, MongoDB, Redis, RavenDB)
docker-compose up -d

# Aguarde ~30 segundos para os serviços ficarem prontos

# Execute todos os testes
dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj

# Pare os serviços
docker-compose down
```

### Scripts Auxiliares

**Windows:**
```bash
test-all.bat up              # Iniciar serviços
test-all.bat test            # Rodar testes
test-all.bat test-sql        # Rodar testes SQL Server
test-all.bat test-mysql      # Rodar apenas testes MySQL
test-all.bat test-postgres   # Rodar testes PostgreSQL
test-all.bat test-mongo      # Rodar testes MongoDB
test-all.bat test-redis      # Rodar testes Redis
test-all.bat test-raven      # Rodar testes RavenDB
test-all.bat down            # Parar serviços
test-all.bat clean           # Limpar tudo
```

**Linux/macOS:**
```bash
./test-all.sh up             # Iniciar serviços
./test-all.sh test           # Rodar testes
./test-all.sh test-sql       # Rodar testes SQL Server
./test-all.sh test-mysql     # Rodar apenas testes MySQL
./test-all.sh test-postgres  # Rodar testes PostgreSQL
./test-all.sh test-mongo     # Rodar testes MongoDB
./test-all.sh test-redis     # Rodar testes Redis
./test-all.sh test-raven     # Rodar testes RavenDB
./test-all.sh down           # Parar serviços
./test-all.sh clean          # Limpar tudo
```

### Testes Manuais por Provider

Para rodar testes de um provider específico sem Docker Compose, veja [TESTING_GUIDE.md](TESTING_GUIDE.md#execução-manual-por-provider).

---

## 🚀 CI/CD

- ✅ Build + Testes (todos os 5 providers)
- ✅ Publicação automática no NuGet em novas releases
- ✅ Deploy automático da documentação

---

## 🤝 Contribuição

1. Fork  
2. Branch: `feature/minha-feature`  
3. PR  
4. Tests devem passar ✔  

---

## 📄 Licença

MIT License.

---

## 👨‍💻 Autor

**Michel Banagouro**  
CTO na Leanwork · Arquiteto e Especialista em ASP .NET  
https://github.com/mbanagouro
https://youtube.com/@aspnetpro