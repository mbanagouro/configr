# Testes do ConfigR

## Testes unitários

Os testes de unidade não dependem de banco de dados e podem ser executados diretamente com:

```bash
dotnet test
```

## Testes de integração com SQL Server (Docker)

Para executar os testes de integração do provider SQL Server, suba um container Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@123" \
  -p 1433:1433 --name sql-configr -d mcr.microsoft.com/mssql/server:2022-latest
```

A connection string padrão usada nos testes é:

```text
Server=localhost,1433;Database=ConfigR_Test;User Id=sa;Password=Pass@123;TrustServerCertificate=True;
```

Se quiser sobrescrever, defina a variável de ambiente:

```bash
set CONFIGR_TEST_SQL_CONN=Server=localhost,1433;Database=ConfigR_Test;User Id=sa;Password=Pass@123;TrustServerCertificate=True;
dotnet test
```

Em Linux/macOS:

```bash
export CONFIGR_TEST_SQL_CONN="Server=localhost,1433;Database=ConfigR_Test;User Id=sa;Password=Pass@123;TrustServerCertificate=True;"
dotnet test
```
