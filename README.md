<p align="center">
  <h1 align="center">ConfigR</h1>
  <p align="center">Strongly-typed runtime configuration for modern .NET apps</p>

  <p align="center">
    <img src="https://img.shields.io/badge/.NET-8.0-blueviolet" />
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
- ⚡ Acesso rápido via ADO.NET
- 🧩 Providers de armazenamento plugáveis (SQL Server incluso)
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

### 2. Registre o ConfigR no DI

```csharp
builder.Services
    .AddConfigR(options =>
    {
        options.DefaultScope = "loja-1"; // opcional
    })
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"), opt =>
    {
        opt.AutoCreateTable = true;
    });
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
| Redis | ConfigR.Redis | 🔜 Planejado |
| FileSystem | ConfigR.FileSystem | 🔜 Planejado |
| Azure (Table/Cosmos) | ConfigR.Azure | 🔜 Planejado |

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

## 🧠 Arquitetura do ConfigR

```
ConfigR.Abstractions  → Interfaces e contratos base
ConfigR.Core          → Implementação padrão (cache, serializer, DI, key formatter)
ConfigR.SqlServer     → Provider SQL Server (ADO.NET)
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

Para rodar integração manualmente:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@123" -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
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
