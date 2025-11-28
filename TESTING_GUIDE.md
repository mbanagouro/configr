# ?? Guia Completo de Testes do ConfigR

Este documento explica como configurar e executar todos os testes do ConfigR com suporte a 5 providers de banco de dados.

---

## ?? Índice

- [Pré-requisitos](#pré-requisitos)
- [Quickstart com Docker Compose](#quickstart-com-docker-compose)
- [Execução Manual por Provider](#execução-manual-por-provider)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Troubleshooting](#troubleshooting)
- [Scripts Auxiliares](#scripts-auxiliares)

---

## ?? Pré-requisitos

### Obrigatório
- **.NET 8.0+** (recomendado: 10.0)
- **Docker** e **Docker Compose**
- **Git** (para clonar o repositório)

### Verificar instalação
```bash
dotnet --version
docker --version
docker-compose --version
```

---

## ?? Quickstart com Docker Compose

### 1?? Clonar o Repositório

```bash
git clone https://github.com/mbanagouro/configr.git
cd configr
```

### 2?? Iniciar Todos os Serviços

```bash
# Windows
test-all.bat up

# Linux/macOS
./test-all.sh up
```

Ou manualmente:

```bash
docker-compose up -d
```

### 3?? Executar Todos os Testes

```bash
# Windows
test-all.bat test

# Linux/macOS
./test-all.sh test

# Ou manualmente
dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj
```

### 4?? Parar os Serviços

```bash
# Windows
test-all.bat down

# Linux/macOS
./test-all.sh down

# Ou manualmente
docker-compose down
```

---

## ??? Execução Manual por Provider

### SQL Server

```bash
# Iniciar container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@123" \
  -p 1433:1433 --name configr-sqlserver -d mcr.microsoft.com/mssql/server:2022-latest

# Executar testes
dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "SqlServer"

# Parar
docker stop configr-sqlserver && docker rm configr-sqlserver
```

### MySQL

```bash
# Iniciar container
docker run -d --name configr-mysql \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=ConfigR_Test \
  -p 3306:3306 mysql:8

# Executar testes
dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "MySql"

# Parar
docker stop configr-mysql && docker rm configr-mysql
```

### PostgreSQL (Npgsql)

```bash
# Iniciar container
docker run -d --name configr-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_DB=configr_test \
  -p 5432:5432 postgres:16

# Executar testes
dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "Npgsql"

# Parar
docker stop configr-postgres && docker rm configr-postgres
```

### MongoDB

```bash
# Iniciar container
docker run -d --name configr-mongo \
  -p 27017:27017 mongo:7

# Executar testes
dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "Mongo"

# Parar
docker stop configr-mongo && docker rm configr-mongo
```

### Redis

```bash
# Iniciar container
docker run -d --name configr-redis \
  -p 6379:6379 redis:7

# Executar testes
dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "Redis"

# Parar
docker stop configr-redis && docker rm configr-redis
```

---

## ?? Variáveis de Ambiente

As variáveis de ambiente permitem customizar as connection strings dos testes.

### Copiar arquivo de exemplo

```bash
cp .env.example .env
```

### Variáveis disponíveis

```env
# SQL Server
CONFIGR_TEST_SQL_CONN=Server=localhost,1433;Database=ConfigR_Test;User Id=sa;Password=Pass@123;TrustServerCertificate=True;

# MySQL
CONFIGR_TEST_MYSQL_CONN=Server=localhost;Port=3306;Database=ConfigR_Test;User=root;Password=root;SslMode=None

# PostgreSQL
CONFIGR_TEST_POSTGRES_CONN=Host=localhost;Port=5432;Database=configr_test;Username=postgres;Password=postgres;

# MongoDB
CONFIGR_TEST_MONGO_CONN=mongodb://localhost:27017

# Redis
CONFIGR_TEST_REDIS_CONN=localhost:6379
```

### Usar variáveis customizadas

**Windows (PowerShell)**
```powershell
$env:CONFIGR_TEST_MYSQL_CONN = "Server=seu-servidor;..."
dotnet test
```

**Windows (CMD)**
```cmd
set CONFIGR_TEST_MYSQL_CONN=Server=seu-servidor;...
dotnet test
```

**Linux/macOS**
```bash
export CONFIGR_TEST_MYSQL_CONN="Server=seu-servidor;..."
dotnet test
```

---

## ?? Estrutura dos Testes

Cada provider possui três categorias de testes:

### 1. ConfigStoreTests
- Testes de operações CRUD básicas
- Testes de scopes/multi-tenant
- Tratamento de valores nulos e edge cases

**Exemplos:**
- `UpsertAndGetAll_Works()`
- `Upsert_RespectsScope()`
- `GetAsync_ReturnsNull_WhenNotExists()`

### 2. IntegrationTests
- Fluxo completo de save/load
- Serialização de tipos complexos
- Testes com classes aninhadas e coleções
- Sobrescrita de valores existentes

**Exemplos:**
- `Should_Save_And_Load_Config()`
- `Should_Override_Existing_Key_On_Upsert()`

### 3. ConcurrencyTests
- Leituras paralelas (100+ tarefas)
- Leitura/escrita concorrente
- Verificação de integridade sob pressão
- Thread-safety

**Exemplos:**
- `ParallelReads_Should_Be_Consistent()`
- `ParallelWriteRead_Should_Not_Corrupt_State()`

---

## ?? Troubleshooting

### Erro: "Connection refused"

**Causa**: Containers não estão rodando ou ainda estão iniciando.

**Solução**:
```bash
# Verificar se containers estão rodando
docker ps

# Aguardar um pouco mais
sleep 30

# Verificar saúde dos containers
docker-compose ps
```

### Erro: "Port already in use"

**Causa**: Porta já está em uso por outro processo.

**Solução**:
```bash
# Encontrar processo na porta (exemplo: 3306)
# Windows
netstat -ano | findstr :3306

# Linux/macOS
lsof -i :3306

# Matar o processo ou usar porta diferente no docker-compose
```

### Erro: "Authentication failed"

**Causa**: Credentials incorretas nas connection strings.

**Solução**:
1. Verificar `.env` com variáveis corretas
2. Verificar credenciais no `docker-compose.yml`
3. Verificar se container tem permissões corretas

### Testes falham intermitentemente

**Causa**: Timing issues ou recursos insuficientes.

**Solução**:
1. Aumentar timeouts no `docker-compose.yml`
2. Verificar recursos do Docker (memória, CPU)
3. Verificar logs dos containers: `docker-compose logs`

### Teste com um provider específico

```bash
# Filtrar por nome de classe
dotnet test --filter "ClassName~MySql"

# Filtrar por nome de teste
dotnet test --filter "Name~Should_Save"

# Combinar filtros
dotnet test --filter "ClassName~MySql&Name~ConfigStore"
```

---

## ??? Scripts Auxiliares

### Windows (test-all.bat)

```batch
test-all.bat up              # Iniciar serviços
test-all.bat test            # Rodar todos os testes
test-all.bat test-mysql      # Rodar apenas testes MySQL
test-all.bat down            # Parar serviços
test-all.bat clean           # Limpar tudo
test-all.bat logs            # Ver logs
```

### Linux/macOS (test-all.sh)

```bash
./test-all.sh up             # Iniciar serviços
./test-all.sh test           # Rodar todos os testes
./test-all.sh test-mysql     # Rodar apenas testes MySQL
./test-all.sh down           # Parar serviços
./test-all.sh clean          # Limpar tudo
./test-all.sh logs           # Ver logs
```

### Comandos Manual

```bash
# Ver status dos serviços
docker-compose ps

# Ver logs de um serviço específico
docker-compose logs mysql

# Acessar shell de um container
docker-compose exec mysql mysql -u root -proot

# Reiniciar um serviço
docker-compose restart mysql

# Remover volumes (limpar dados)
docker-compose down -v
```

---

## ?? Cobertura de Providers

| Provider | Teste Store | Teste Integration | Teste Concurrency | Docker |
|----------|-------------|------------------|-------------------|--------|
| SQL Server | ? | ? | ? | ? |
| MySQL | ? | ? | ? | ? |
| PostgreSQL | ? | ? | ? | ? |
| MongoDB | ? | ? | ? | ? |
| Redis | ? | ? | ? | ? |

---

## ?? Fluxo Recomendado de Teste

```
1. Clone o repositório
   ?
2. Inicie os serviços (docker-compose up -d)
   ?
3. Aguarde 30 segundos
   ?
4. Execute testes por provider (opcional)
   ?
5. Execute todos os testes (dotnet test)
   ?
6. Verifique resultados
   ?
7. Pause os serviços (docker-compose down)
```

---

## ?? Dicas de Performance

### Para máquinas com recursos limitados

```bash
# Executar testes de um provider por vez
dotnet test --filter "ClassName~SqlServer"
# depois
dotnet test --filter "ClassName~MySql"
```

### Para máquinas com muitos recursos

```bash
# Rodar testes em paralelo
dotnet test /p:ParallelizeAssemblyTests=true
```

### Limpar cache entre testes

```bash
dotnet clean
dotnet build
dotnet test
```

---

## ?? Referências

- [Documentação oficial ConfigR](https://mbanagouro.github.io/configr)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [xUnit Documentation](https://xunit.net/)
- [Documentação do .NET](https://docs.microsoft.com/dotnet/)

---

## ? FAQ

**P: Preciso executar todos os 5 providers?**
R: Não! Use `--filter` para executar apenas o provider que precisa testar.

**P: Como testar com um banco de dados remoto?**
R: Defina a connection string na variável de ambiente (ex: `CONFIGR_TEST_MYSQL_CONN`).

**P: Os testes deletam dados de produção?**
R: Não! Os testes usam bancos de dados específicos (`ConfigR_Test`). Certifique-se de usar as variáveis de ambiente corretas.

**P: Quanto tempo leva para rodar todos os testes?**
R: ~5-10 minutos dependendo do hardware. Testes de concorrência são os mais lentos.

---

## ?? Próximos Passos

1. Consulte a [Documentação Oficial](https://mbanagouro.github.io/configr)
2. Veja exemplos de uso em [docs/getting-started.md](../docs/getting-started.md)
3. Explore providers específicos em `docs/storage/`
