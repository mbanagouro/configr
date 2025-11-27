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

| Provider | Pacote | Status |
|---------|--------|--------|
| SQL Server | ConfigR.SqlServer | ✅ Incluído |
| MySQL | ConfigR.MySQL | ✅ Incluído |
| Npgsql | ConfigR.Npgsql | ✅ Incluído |
| MongoDB | ConfigR.MongoDB | ✅ Incluído |
| Redis | ConfigR.Redis | ✅ Incluído |
| RavenDB | ConfigR.RavenDB | 🔜 Planejado |

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

## 🗄 Estrutura da Tabela (Npsql)

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
ConfigR.Npgsql        → Provider Npgsql
ConfigR.MongoDB       → Provider MongoDB
ConfigR.Redis         → Provider Redis
```

---

## 📘 Documentação Oficial

Disponível em:

👉 **https://mbanagouro.github.io/configr**

---

## 🧪 Testes

```bash
dotnet test
```

Para rodar integração manualmente (SQL Server):

```bash
docker run --name sqlserver-configr -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@123" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

Para rodar integração manualmente (MySQL):

```bash
docker run -d --name mysql-configr -e MYSQL_ROOT_PASSWORD=123456 -e MYSQL_DATABASE=configr_test -p 3306:3306 mysql:8
```

Para rodar integração manualmente (Npgsql)

```bash
docker run --name pg-configr -e POSTGRES_PASSWORD=123456 -e POSTGRES_USER=postgres -e POSTGRES_DB=configr_test -p 5432:5432 -d postgres:16

```

Para rodar integração manualmente (MongoDB):

```bash
docker run -d --name mongo-configr -p 27017:27017 mongo:7
```

Para rodar integração manualmente (Redis)

```bash
docker run -d --name redis-configr -p 6379:6379 redis:7

```

---

## 🚀 CI/CD

- Build + Testes
- Publicação automática no NuGet em novas releases
- Deploy automático da documentação

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
