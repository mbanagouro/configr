<p align="center">
  <h1 align="center">ConfigR</h1>
  <p align="center">Configuração tipada em runtime para aplicações .NET modernas</p>

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

## 🚀 O que é ConfigR?

**ConfigR** é uma biblioteca leve, extensível e altamente performática para **configurações tipadas em runtime** em aplicações .NET.

### Principais Características

- 💪 **Tipagem forte** - Classes de configuração type-safe
- 🚀 **Cache em memória** - Otimização de performance integrada
- 🧩 **Providers plugáveis** - Múltiplos opções de armazenamento
- 🧱 **Scopes multi-tenant** - Configurações isoladas por tenant
- 🔧 **Serialização customizável** - Formatação flexível de dados
- 🧠 **Zero mágica** - Sem reflection pesada ou comportamentos implícitos

### Casos de Uso

- Backoffices configuráveis
- Plataformas multi-tenant
- Feature flags
- Sistemas com configurações dinâmicas
- Substituir `appsettings.json` para configurações em runtime
- E-commerce, ERP e plataformas SaaS

---

## 📦 Instalação

Instale o pacote core e o provider de armazenamento desejado:

```bash
dotnet add package ConfigR.Core
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

---

## 🚀 Início Rápido

### 1. Defina sua classe de configuração

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

### 3. Leia a configuração tipada

```csharp
var checkout = await configR.GetAsync<CheckoutConfig>();

if (checkout.LoginRequired)
{
    // Sua lógica aqui
}
```

### 4. Atualize em runtime

```csharp
checkout.LoginRequired = false;
await configR.SaveAsync(checkout);
```

---

## 🗄️ Providers de Armazenamento

O ConfigR suporta múltiplos backends de armazenamento. Escolha o que melhor se encaixa na sua infraestrutura:

| Provider | Pacote | Status |
|----------|--------|--------|
| SQL Server | `ConfigR.SqlServer` | ✅ Pronto |
| MySQL | `ConfigR.MySql` | ✅ Pronto |
| PostgreSQL | `ConfigR.Npgsql` | ✅ Pronto |
| MongoDB | `ConfigR.MongoDB` | ✅ Pronto |
| Redis | `ConfigR.Redis` | ✅ Pronto |
| RavenDB | `ConfigR.RavenDB` | ✅ Pronto |

Para instruções de configuração específicas de cada provider, consulte a [documentação oficial](https://mbanagouro.github.io/configr).

---

## 🗄️ Configuração SQL Server (Exemplo Padrão)

### 1. Crie a tabela de configuração

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

### 2. Configure na sua aplicação

```csharp
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

---

## 🗝️ Arquitetura

```
ConfigR.Abstractions  → Interfaces e contratos base
ConfigR.Core          → Implementação core (cache, serialização, DI, formatação de chaves)
ConfigR.SqlServer     → Provider SQL Server
ConfigR.MySql         → Provider MySQL
ConfigR.Npgsql        → Provider PostgreSQL
ConfigR.MongoDB       → Provider MongoDB
ConfigR.Redis         → Provider Redis
ConfigR.RavenDB       → Provider RavenDB
```

---

## 📖 Documentação

Documentação completa e guias disponíveis em:

👉 **https://mbanagouro.github.io/configr**

---

## 🧪 Testes

### Início Rápido com Docker Compose

```bash
# Clone o repositório
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
test-all.bat test            # Executar todos os testes
test-all.bat test-sql        # Executar apenas testes SQL Server
test-all.bat test-mysql      # Executar apenas testes MySQL
test-all.bat test-postgres   # Executar apenas testes PostgreSQL
test-all.bat test-mongo      # Executar apenas testes MongoDB
test-all.bat test-redis      # Executar apenas testes Redis
test-all.bat test-raven      # Executar apenas testes RavenDB
test-all.bat down            # Parar serviços
test-all.bat clean           # Limpar tudo
```

**Linux/macOS:**
```bash
./test-all.sh up             # Iniciar serviços
./test-all.sh test           # Executar todos os testes
./test-all.sh test-sql       # Executar apenas testes SQL Server
./test-all.sh test-mysql     # Executar apenas testes MySQL
./test-all.sh test-postgres  # Executar apenas testes PostgreSQL
./test-all.sh test-mongo     # Executar apenas testes MongoDB
./test-all.sh test-redis     # Executar apenas testes Redis
./test-all.sh test-raven     # Executar apenas testes RavenDB
./test-all.sh down           # Parar serviços
./test-all.sh clean          # Limpar tudo
```

Para testes manuais por provider, consulte [TESTING_GUIDE.md](TESTING_GUIDE.md).

---

## 🚀 CI/CD

- ✅ Build e testes (todos os providers)
- ✅ Publicação automática no NuGet em novas releases
- ✅ Deploy automático da documentação

---

## 🤝 Contribuição

1. Faça um fork do repositório
2. Crie uma branch para sua feature: `git checkout -b feature/minha-feature`
3. Commit suas mudanças: `git commit -am 'Adicionar minha feature'`
4. Push para a branch: `git push origin feature/minha-feature`
5. Abra um pull request
6. Certifique-se de que todos os testes passam ✓

---

## 📄 Licença

MIT License - consulte o arquivo [LICENSE](LICENSE) para detalhes.

---

## 👨‍💻 Autor

**Michel Banagouro**  
CTO na Leanwork · Arquiteto e Especialista em ASP.NET

- GitHub: https://github.com/mbanagouro
- YouTube: https://youtube.com/@aspnetpro