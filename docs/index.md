# 🚀 ConfigR - Documentação

Bem-vindo à documentação do **ConfigR**, uma biblioteca leve, extensível e altamente performática para **configurações tipadas em runtime** em aplicações .NET.

## 📚 Navegação Rápida

### ⚡ Começar Rápido

- **[Início Rápido](getting-started.md)** - Instale e configure em 5 minutos
- **[Configuração](configuration.md)** - Configure o ConfigR no seu projeto

### 🗄️ Providers de Armazenamento

Escolha o backend que melhor se encaixa na sua infraestrutura:

- **[SQL Server](storage/sql-server.md)** - Padrão, recomendado para a maioria dos casos
- **[MySQL](storage/mysql.md)** - Alternativa leve e popular
- **[PostgreSQL (Npgsql)](storage/npgsql.md)** - Open source e robusto
- **[MongoDB](storage/mongodb.md)** - Banco de dados NoSQL
- **[Redis](storage/redis.md)** - Cache em memória de alta performance
- **[RavenDB](storage/ravendb.md)** - Banco de dados multi-modelo

### 🎯 Conceitos Avançados

- **[Scopes](advanced/scopes.md)** - Multi-tenant e isolamento de configurações
- **[Cache](advanced/caching.md)** - Entenda como o cache em memória funciona
- **[Extensibilidade](advanced/extensibility.md)** - Crie providers personalizados

### 🧪 Testes e Qualidade

- **[Testes](testing.md)** - Como testar sua aplicação com ConfigR
- **[API Reference](api-reference.md)** - Documentação completa da API

---

## 🎯 Para que serve o ConfigR?

### ✅ Ideal para:

- **Backoffices configuráveis** - Altere comportamentos sem deploy
- **Plataformas multi-tenant** - Isolamento de configurações por cliente
- **Feature flags** - Ative/desative funcionalidades dinamicamente
- **Sistemas evolutivos** - Configurações que mudam em runtime
- **E-commerce, ERP, SaaS** - Plataformas que precisam de flexibilidade

### 🎁 Características Principais

| Característica | Benefício |
|---|---|
| 🔥 Tipagem Forte | Type-safe, sem magic strings |
| 🚀 Cache em Memória | Performance otimizada |
| 🧩 Providers Plugáveis | Escolha o backend desejado |
| 🧱 Multi-tenant | Isolamento de dados por escopo |
| 🔧 Customizável | Serialização e comportamento flexíveis |
| 🧠 Simples | Zero reflection pesada ou mágica |

---

## 🚀 Começar em 1 Minuto

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
    public string Valor { get; set; } = "padrão";
}

// Ler
var config = await configR.GetAsync<MeuConfig>();

// Atualizar
config.Valor = "novo";
await configR.SaveAsync(config);
```

---

## 📖 Próximos Passos

- 👉 **Comece agora**: [Guia de Início Rápido](getting-started.md)
- 🔧 **Configure seu provider**: [Configuração](configuration.md)
- 📚 **Aprenda conceitos avançados**: [Scopes](advanced/scopes.md)