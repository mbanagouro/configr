# Testes do ConfigR

## Testes unitÃ¡rios

Os testes de unidade nÃ£o dependem de banco de dados e podem ser executados diretamente com:

```bash
dotnet test
```

## Testes de integraÃ§Ã£o com SQL Server (Docker)

Para executar os testes de integraÃ§Ã£o do provider SQL Server, suba um container Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@123" \
  -p 1433:1433 --name sql-configr -d mcr.microsoft.com/mssql/server:2022-latest
```

A connection string padrÃ£o usada nos testes Ã©:

```text
Server=localhost,1433;Database=ConfigR_Test;User Id=sa;Password=Pass@123;TrustServerCertificate=True;
```

Se quiser sobrescrever, defina a variÃ¡vel de ambiente:

```bash
set CONFIGR_TEST_SQL_CONN=Server=localhost,1433;Database=ConfigR_Test;User Id=sa;Password=Pass@123;TrustServerCertificate=True;
dotnet test
```

Em Linux/macOS:

```bash
export CONFIGR_TEST_SQL_CONN="Server=localhost,1433;Database=ConfigR_Test;User Id=sa;Password=Pass@123;TrustServerCertificate=True;"
dotnet test
```

---

## Testes de integraÃ§Ã£o com MySQL (Docker)

Para executar os testes de integraÃ§Ã£o do provider MySQL, suba um container Docker:

```bash
docker run -d --name mysql-configr \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=ConfigR_Test \
  -p 3306:3306 mysql:8
```

A connection string padrÃ£o usada nos testes Ã©:

```text
Server=localhost;Database=ConfigR_Test;User Id=root;Password=root;
```

Se quiser sobrescrever, defina a variÃ¡vel de ambiente:

```bash
set CONFIGR_TEST_MYSQL_CONN=Server=localhost;Database=ConfigR_Test;User Id=root;Password=root;
dotnet test
```

Em Linux/macOS:

```bash
export CONFIGR_TEST_MYSQL_CONN="Server=localhost;Database=ConfigR_Test;User Id=root;Password=root;"
dotnet test
```

---

## Testes de integraÃ§Ã£o com PostgreSQL (Npgsql) (Docker)

Para executar os testes de integraÃ§Ã£o do provider Npgsql, suba um container Docker:

```bash
docker run -d --name postgres-configr \
  -e POSTGRES_PASSWORD=123456 \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_DB=configr_test \
  -p 5432:5432 postgres:16
```

A connection string padrÃ£o usada nos testes Ã©:

```text
Host=localhost;Port=5432;Database=configr_test;Username=postgres;Password=123456;
```

Se quiser sobrescrever, defina a variÃ¡vel de ambiente:

```bash
set CONFIGR_TEST_POSTGRES_CONN=Host=localhost;Port=5432;Database=configr_test;Username=postgres;Password=123456;
dotnet test
```

Em Linux/macOS:

```bash
export CONFIGR_TEST_POSTGRES_CONN="Host=localhost;Port=5432;Database=configr_test;Username=postgres;Password=123456;"
dotnet test
```

---

## Testes de integraÃ§Ã£o com MongoDB (Docker)

Para executar os testes de integraÃ§Ã£o do provider MongoDB, suba um container Docker:

```bash
docker run -d --name mongo-configr \
  -p 27017:27017 mongo:7
```

A connection string padrÃ£o usada nos testes Ã©:

```text
mongodb://localhost:27017
```

Se quiser sobrescrever, defina a variÃ¡vel de ambiente:

```bash
set CONFIGR_TEST_MONGO_CONN=mongodb://localhost:27017
dotnet test
```

Em Linux/macOS:

```bash
export CONFIGR_TEST_MONGO_CONN="mongodb://localhost:27017"
dotnet test
```

---

## Testes de integraÃ§Ã£o com Redis (Docker)

Para executar os testes de integraÃ§Ã£o do provider Redis, suba um container Docker:

```bash
docker run -d --name redis-configr \
  -p 6379:6379 redis:7
```

A connection string padrÃ£o usada nos testes Ã©:

```text
localhost:6379
```

Se quiser sobrescrever, defina a variÃ¡vel de ambiente:

```bash
set CONFIGR_TEST_REDIS_CONN=localhost:6379
dotnet test
```

Em Linux/macOS:

```bash
export CONFIGR_TEST_REDIS_CONN="localhost:6379"
dotnet test
```

---

## Testes de integraï¿½ï¿½Çœo com RavenDB (Docker)

Para executar os testes de integraï¿½ï¿½Çœo do provider RavenDB, suba um container Docker:

```bash
docker run -d --name ravendb-configr \
  -p 8080:8080 \
  -e RAVEN_Setup_Mode=None \
  -e RAVEN_License_Eula_Accepted=true \
  -e RAVEN_Security_UnsecuredAccessAllowed=PublicNetwork \
  ravendb/ravendb:6.0-ubuntu-latest
```

A URL padrÇœo usada nos testes Ç¸:

```text
http://localhost:8080
```

Se quiser sobrescrever, defina as variï¿½ï¿½veis de ambiente:

```bash
set CONFIGR_TEST_RAVEN_URLS=http://localhost:8080
set CONFIGR_TEST_RAVEN_DB=ConfigR_Test
dotnet test
```

Em Linux/macOS:

```bash
export CONFIGR_TEST_RAVEN_URLS="http://localhost:8080"
export CONFIGR_TEST_RAVEN_DB="ConfigR_Test"
dotnet test
```

---

## Rodar todos os testes com Docker Compose

Para rodar todos os testes simultaneamente com todos os provedores, use docker-compose:

```bash
docker-compose up -d
dotnet test
docker-compose down
```

---

## Estrutura dos Testes

Cada provider tem seus prÃ³prios testes organizados em trÃªs categorias:

### 1. **ConfigStoreTests**
- CRUD bÃ¡sico (Create, Read, Update, Delete)
- OperaÃ§Ãµes com scopes
- Tratamento de valores nulos

### 2. **IntegrationTests**
- Fluxo completo com tipos complexos (aninhados, coleÃ§Ãµes)
- SerializaÃ§Ã£o e desserializaÃ§Ã£o
- Sobrescrita de valores existentes

### 3. **ConcurrencyTests**
- Leituras paralelas (100+ tarefas simultÃ¢neas)
- Leitura/escrita concorrente (race conditions)
- Integridade de estado sob pressÃ£o

---

## Troubleshooting

### Erro de conexÃ£o no teste

Certifique-se de que:
1. O container estÃ¡ rodando: `docker ps`
2. A porta estÃ¡ correta: `docker port <container-name>`
3. O firewall permite a conexÃ£o
4. A connection string estÃ¡ correta

### Teste falha intermitentemente

Verifique:
1. Recursos disponÃ­veis (memÃ³ria, CPU)
2. LatÃªncia de rede
3. Timeout do container (`--health-timeout`)
4. Logs do container: `docker logs <container-name>`
