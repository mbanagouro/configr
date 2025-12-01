# MySQL Provider

Uso do provider MySQL no ConfigR.

---

## 🚀 Instalação

```bash
dotnet add package ConfigR.MySql
```

---

## 🔧 Configuração

### Registrar no DI

```csharp
builder.Services
    .AddConfigR()
    .UseMySql(builder.Configuration.GetConnectionString("ConfigR"));
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "ConfigR": "Server=localhost;Database=configr;User Id=root;Password=root;"
  }
}
```

---

## 📊 Estrutura da Tabela

```sql
CREATE TABLE IF NOT EXISTS configr (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    cfg_key VARCHAR(255) NOT NULL,
    cfg_value TEXT NOT NULL,
    scope VARCHAR(255) NULL,
    UNIQUE KEY uk_config (cfg_key, scope)
);
```

### Campos

- **id**: Identificador único auto-incremento
- **cfg_key**: Chave da configuração (até 255 caracteres)
- **cfg_value**: Valor da configuração (texto livre, suporta JSON)
- **scope**: Escopo opcional para multi-tenant (até 255 caracteres)

---

## ⚙️ Opções de Configuração

```csharp
var options = Options.Create(new MySqlConfigStoreOptions
{
    ConnectionString = "Server=localhost;Database=configr;User Id=root;Password=root;",
    Table = "configr"  // Nome da tabela (padrão: "configr")
});

var store = new MySqlConfigStore(options);
```

---

## 📝 Exemplo Completo

```csharp
// Classe de configuração
public sealed class CheckoutConfig
{
    public bool LoginRequired { get; set; } = true;
    public int MaxItems { get; set; } = 20;
}

// Program.cs
builder.Services
    .AddConfigR()
    .UseMySql(builder.Configuration.GetConnectionString("ConfigR"))

// Uso em controller/service
var checkout = await _configR.GetAsync<CheckoutConfig>();

if (checkout.LoginRequired)
{
    // ...
}

checkout.MaxItems = 50;
await _configR.SaveAsync(checkout);
```

---

## 🧪 Testes

O provider MySQL possui testes completos incluindo:

- **ConfigStoreTests**: Testes de CRUD básico e scopes
- **IntegrationTests**: Testes de fluxo completo com tipos complexos
- **ConcurrencyTests**: Testes de leitura/escrita paralela

Para executar os testes do MySQL:

```bash
# Iniciar container MySQL
docker run -d --name mysql-configr -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=configr_test -p 3306:3306 mysql:8

# Executar testes
dotnet test

# Limpar
docker stop mysql-configr && docker rm mysql-configr
```

---

## 💡 Considerações de Performance

- Índice único em `(cfg_key, scope)` garante integridade e performance
- Use scopes para isolamento multi-tenant
- Cache em memória (ConfigR.Core) reduz queries ao banco
- Textos longos são suportados com `TEXT`
