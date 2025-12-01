# ??? Storage Providers - Visão Geral

Escolha o backend de armazenamento que melhor se encaixa na sua infraestrutura.

## ?? Comparação de Providers

| Característica | SQL Server | MySQL | PostgreSQL | MongoDB | Redis | RavenDB |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| **Tipo** | Relacional | Relacional | Relacional | NoSQL | Cache | Multi-modelo |
| **Performance** | ???? | ??? | ???? | ???? | ????? | ???? |
| **Escalabilidade** | ???? | ??? | ???? | ????? | ???? | ???? |
| **Facilidade** | ???? | ????? | ???? | ??? | ???? | ??? |
| **Custo** | ???? | ?? | ?? | ?? | ?? | ???? |
| **Persistência** | ? | ? | ? | ? | ?? | ? |

## ?? Recomendações por Cenário

### ?? Empresas e Produção

**?? Recomendado: SQL Server ou PostgreSQL**

```bash
dotnet add package ConfigR.SqlServer
# ou
dotnet add package ConfigR.Npgsql
```

**Por quê:**
- Confiabilidade extrema
- Compliance e auditoria
- Suporte corporativo
- Transações ACID garantidas
- Backup e recovery robustos

### ?? Startups e Escalabilidade

**?? Recomendado: MongoDB ou PostgreSQL**

```bash
dotnet add package ConfigR.MongoDB
# ou
dotnet add package ConfigR.Npgsql
```

**Por quê:**
- Escalabilidade horizontal
- Flexibilidade de schema
- Replicação nativa
- Performance sob alta demanda

### ? Alta Performance

**?? Recomendado: Redis**

```bash
dotnet add package ConfigR.Redis
```

**Por quê:**
- Latência < 1ms
- Throughput altíssimo
- Ideal para cache crítico
- Suporta features avançadas

### ?? Low-cost / Prototipagem

**?? Recomendado: MySQL**

```bash
dotnet add package ConfigR.MySql
```

**Por quê:**
- Licença aberta
- Hosting barato
- Fácil de configurar
- Suporte vasto

### ?? Segurança Crítica

**?? Recomendado: RavenDB**

```bash
dotnet add package ConfigR.RavenDB
```

**Por quê:**
- Criptografia built-in
- Controle de acesso fino
- Compliance ready
- Replicação segura

---

## ?? Detalhes por Provider

### [SQL Server](sql-server.md)

O padrão, recomendado para a maioria dos casos.

- ? Melhor performance em workloads OLTP
- ? Índices otimizados
- ? Enterprise-ready
- ? Pode ser caro

### [MySQL](mysql.md)

Leveza e compatibilidade universal.

- ? Mais leve que SQL Server
- ? Altamente compatível
- ? Fácil de hospedar
- ? Menos recursos avançados

### [PostgreSQL (Npgsql)](npgsql.md)

Open-source robusto e poderoso.

- ? Open source
- ? Features avançadas (JSON, arrays)
- ? Excelente performance
- ? Replicação nativa

### [MongoDB](mongodb.md)

NoSQL flexível e escalável.

- ? Schema flexível
- ? Escalabilidade horizontal
- ? Ótimo para dados semi-estruturados
- ? Menos eficiente que relacional para este caso

### [Redis](redis.md)

Cache ultra-rápido em memória.

- ? Latência mínima
- ? Altíssimo throughput
- ? Perfect para cache crítico
- ?? Sem persistência por padrão

### [RavenDB](ravendb.md)

Multi-modelo enterprise com recursos avançados.

- ? Segurança built-in
- ? ACID distribuído
- ? Replicação transparente
- ? Menos comum, comunidade menor

---

## ?? Quick Start por Provider

### SQL Server

```csharp
builder.Services
    .AddConfigR()
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### MySQL

```csharp
builder.Services
    .AddConfigR()
    .UseMySql(builder.Configuration.GetConnectionString("ConfigR"));
```

### PostgreSQL

```csharp
builder.Services
    .AddConfigR()
    .UseNpgsql(builder.Configuration.GetConnectionString("ConfigR"));
```

### MongoDB

```csharp
builder.Services
    .AddConfigR()
    .UseMongoDb("mongodb://localhost:27017", "ConfigR");
```

### Redis

```csharp
builder.Services
    .AddConfigR()
    .UseRedis("localhost:6379");
```

### RavenDB

```csharp
builder.Services
    .AddConfigR()
    .UseRavenDb(new[] { "http://localhost:8080" }, "ConfigR");
```

---

## ?? Migrar Entre Providers

Você pode migrar dados entre providers:

```csharp
// 1. Ler do provider antigo
var oldStore = new SqlServerConfigStore(/* ... */);
var allConfigs = await oldStore.GetAllAsync();

// 2. Escrever no novo provider
var newStore = new MySqlConfigStore(/* ... */);
foreach (var config in allConfigs)
{
    await newStore.SaveAsync(config.Key, config.Value, config.Scope);
}
```

---

## ?? Boas Práticas

### ? Faça

- Escolha o provider baseado em seus requisitos
- Teste a performance com seus dados reais
- Implemente backup adequado
- Monitore a saúde do banco de dados
- Use replicação para alta disponibilidade

### ? Evite

- Trocar de provider sem planejar
- Usar Redis como persistência principal
- Ignorar limites de conexão
- Não fazer backup
- Misturar providers sem motivo

---

## ?? Próximos Passos

- ?? [Escolha seu Provider](../configuration.md)
- ?? [Início Rápido](../getting-started.md)
- ?? [Referência da API](../api-reference.md)
