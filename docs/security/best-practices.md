# Guia de Boas Práticas de Segurança - ConfigR

Este documento fornece orientações essenciais para uso seguro do ConfigR em ambientes de produção.
 
---

## O QUE NUNCA FAZER

### 1. Armazenar Senhas ou Secrets

```csharp
// NUNCA FAÇA ISSO
public class DatabaseConfig
{
    public string ConnectionString { get; set; }
    public string AdminPassword { get; set; }  // NUNCA!
    public string ApiKey { get; set; }          // NUNCA!
}
```

**Por quê?**
- ConfigR não criptografa dados por padrão
- Dados ficam em texto plano no banco
- Violação de compliance (PCI-DSS, SOC 2, ISO 27001)

**Use em vez disso:**
```csharp
// Azure Key Vault
builder.Configuration.AddAzureKeyVault(
    new Uri("https://myvault.vault.azure.net/"),
    new DefaultAzureCredential());

var apiKey = builder.Configuration["ApiKey"]; // Do Key Vault

// AWS Secrets Manager
builder.Configuration.AddSecretsManager();

// HashiCorp Vault
builder.Configuration.AddVaultConfiguration(...);
```

### 2. Expor APIs de Configuração Sem Autenticação

```csharp
// PERIGOSO - Qualquer um pode modificar
[HttpPost("config")]
[AllowAnonymous]  // NUNCA!
public async Task UpdateConfig([FromBody] MyConfig config)
{
    await _configR.SaveAsync(config);
}
```

**Use autenticação forte:**
```csharp
[HttpPost("config")]
[Authorize(Policy = "AdminOnly")]
[RequireHttps]
public async Task UpdateConfig([FromBody] MyConfig config)
{
    // Log de auditoria
    _logger.LogWarning(
        "Config {Type} modified by {User} from {IP}",
        typeof(MyConfig).Name,
        User.Identity?.Name,
        HttpContext.Connection.RemoteIpAddress);
    
    await _configR.SaveAsync(config);
}
```

### 3. Usar Connection Strings com Senhas Hardcoded

```json
// NÃO FAZER
{
  "ConnectionStrings": {
    "ConfigR": "Server=prod.db.com;User=admin;Password=Pass123!;"
  }
}
```

**Use User Secrets (Desenvolvimento):**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:ConfigR" "Server=localhost;..."
```

**Use Variáveis de Ambiente (Produção):**
```bash
# Linux/Docker
export ConnectionStrings__ConfigR="Server=prod;..."

# Azure App Service
# Configure em Application Settings
```

**Use Managed Identity:**
```csharp
// Azure SQL com Managed Identity
"Server=myserver.database.windows.net;Authentication=Active Directory Default;Database=configr;"

// AWS RDS com IAM
"Server=mydb.region.rds.amazonaws.com;Integrated Security=true;..."
```

### 4. Armazenar PII (Dados Pessoais)

```csharp
// VIOLAÇÃO DE LGPD/GDPR
public class UserConfig
{
    public string CPF { get; set; }           // NUNCA!
    public string Email { get; set; }          // NUNCA!
    public string Address { get; set; }        // NUNCA!
}
```

**Por quê?**
- ConfigR não tem recursos de anonimização
- Violação de LGPD/GDPR
- Não tem auditoria de acesso a PII

**Use sistemas apropriados:**
- Customer Data Platforms (CDPs)
- Bancos de dados com criptografia em campo
- Sistemas com consent management

---

## O QUE SEMPRE FAZER

### 1. Validar Todas as Entradas

```csharp
using System.ComponentModel.DataAnnotations;

public class CheckoutConfig
{
    [Range(1, 1000, ErrorMessage = "MaxItems must be between 1 and 1000")]
    public int MaxItems { get; set; } = 20;
    
    [StringLength(500, ErrorMessage = "Description too long")]
    public string Description { get; set; } = string.Empty;
    
    [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Invalid characters")]
    public string Feature { get; set; } = "enabled";
}

// Validar antes de salvar
[HttpPost("config")]
public async Task<IActionResult> UpdateConfig([FromBody] CheckoutConfig config)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    await _configR.SaveAsync(config);
    return Ok();
}
```

### 2. Implementar Audit Logging

```csharp
public class AuditConfigR : IConfigR
{
    private readonly IConfigR _inner;
    private readonly ILogger<AuditConfigR> _logger;
    private readonly IHttpContextAccessor _httpContext;

    public async Task SaveAsync<T>(T config) where T : new()
    {
        var user = _httpContext.HttpContext?.User.Identity?.Name ?? "Anonymous";
        var ip = _httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var type = typeof(T).Name;
        
        _logger.LogWarning(
            "AUDIT: Config {ConfigType} modified by user {User} from IP {IP} at {Timestamp}",
            type, user, ip, DateTime.UtcNow);
        
        await _inner.SaveAsync(config);
        
        _logger.LogInformation("AUDIT: Config {ConfigType} saved successfully", type);
    }

    public Task<T> GetAsync<T>() where T : new()
    {
        var user = _httpContext.HttpContext?.User.Identity?.Name ?? "Anonymous";
        _logger.LogInformation("AUDIT: Config {ConfigType} accessed by {User}", typeof(T).Name, user);
        
        return _inner.GetAsync<T>();
    }
}

// Registrar
builder.Services.AddSingleton<IConfigR, AuditConfigR>();
```

### 3. Usar TLS/SSL para Conexões de Banco

```csharp
// SQL Server
"Server=prod.db.com;Database=configr;Encrypt=True;TrustServerCertificate=False;..."

// MySQL
"Server=prod.db.com;Database=configr;SslMode=Required;SslCa=/path/to/ca.pem;..."

// PostgreSQL
"Host=prod.db.com;Database=configr;SSL Mode=Require;Trust Server Certificate=false;..."

// MongoDB
"mongodb://prod.db.com:27017/configr?ssl=true&tlsAllowInvalidCertificates=false"

// Redis
"prod.redis.com:6380,ssl=true,password=..."
```

### 4. Implementar Rate Limiting

```csharp
using Microsoft.AspNetCore.RateLimiting;

// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("config-api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded", token);
    };
});

// Controller
[EnableRateLimiting("config-api")]
[HttpPost("config")]
public async Task UpdateConfig([FromBody] MyConfig config) { }
```

### 5. Princípio do Menor Privilégio (Database)

```sql
-- SQL Server: Criar usuário com permissões mínimas
CREATE LOGIN configr_app WITH PASSWORD = 'StrongPassword!123';
CREATE USER configr_app FOR LOGIN configr_app;

-- Apenas SELECT, INSERT, UPDATE na tabela ConfigR
GRANT SELECT, INSERT, UPDATE ON dbo.ConfigR TO configr_app;

-- Remover permissões perigosas
REVOKE DELETE, DROP, ALTER, CREATE FROM configr_app;
DENY CONTROL ON DATABASE::configr TO configr_app;

-- MySQL
CREATE USER 'configr_app'@'%' IDENTIFIED BY 'StrongPassword!123';
GRANT SELECT, INSERT, UPDATE ON configr.ConfigR TO 'configr_app'@'%';
FLUSH PRIVILEGES;

-- PostgreSQL
CREATE ROLE configr_app WITH LOGIN PASSWORD 'StrongPassword!123';
GRANT SELECT, INSERT, UPDATE ON configr.configr TO configr_app;
REVOKE DELETE, DROP, TRUNCATE ON configr.configr FROM configr_app;
```

### 6. Monitoramento e Alertas

```csharp
// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Serilog com alertas
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
    .WriteTo.File("logs/configr-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("Application", "ConfigR")
    .CreateLogger();

// Alert em modificações
public class AlertingConfigR : IConfigR
{
    private readonly IConfigR _inner;
    private readonly IAlertService _alertService;

    public async Task SaveAsync<T>(T config) where T : new()
    {
        await _inner.SaveAsync(config);
        
        // Alertar time de segurança
        await _alertService.SendAlertAsync(new Alert
        {
            Severity = AlertSeverity.Warning,
            Message = $"Configuration {typeof(T).Name} was modified",
            Timestamp = DateTime.UtcNow
        });
    }
}
```

---

## Configurações de Segurança por Ambiente

### Desenvolvimento

```csharp
// appsettings.Development.json
{
  "ConfigR": {
    "CacheDuration": "00:00:30",  // 30 segundos para teste
    "EnableDetailedErrors": true,
    "EnableDebugLogs": true
  },
  "ConnectionStrings": {
    "ConfigR": "Server=localhost;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

### Staging

```csharp
// appsettings.Staging.json
{
  "ConfigR": {
    "CacheDuration": "00:05:00",  // 5 minutos
    "EnableDetailedErrors": true,
    "EnableDebugLogs": false
  }
}

// Use Azure Key Vault ou AWS Secrets Manager para connection string
```

### Produção

```csharp
// appsettings.Production.json
{
  "ConfigR": {
    "CacheDuration": "00:10:00",  // 10 minutos
    "EnableDetailedErrors": false,
    "EnableDebugLogs": false
  }
}

// Todas as secrets em Key Vault/Secrets Manager
// Managed Identity para autenticação
// TLS obrigatório
// Rate limiting ativo
// Audit logging obrigatório
```

---

## Testes de Segurança

```csharp
public class SecurityTests
{
    [Fact]
    public void Should_Reject_SQL_Injection_In_TableName()
    {
        // Arrange
        var maliciousName = "ConfigR'; DROP TABLE Users; --";
        
        // Act
        Action act = () => new SqlServerConfigStore(Options.Create(
            new SqlServerConfigStoreOptions 
            { 
                Table = maliciousName 
            }));
        
        // Assert
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public async Task Should_Reject_Oversized_Values()
    {
        // Arrange
        var store = CreateStore();
        var oversizedValue = new string('X', 200_000_000); // 200MB
        
        // Act
        Func<Task> act = async () => await store.UpsertAsync(new[] 
        { 
            new ConfigEntry { Key = "test", Value = oversizedValue } 
        });
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
```

---

## Checklist de Segurança

Antes de implantar em produção:

- [ ] Connection strings não têm senhas hardcoded
- [ ] TLS/SSL habilitado em todas as conexões
- [ ] Usuário de banco com permissões mínimas
- [ ] Autenticação obrigatória em endpoints de config
- [ ] Autorização baseada em roles (Admin, Operator, etc)
- [ ] Validação de entrada implementada
- [ ] Audit logging configurado
- [ ] Rate limiting ativo
- [ ] Monitoramento e alertas configurados
- [ ] Secrets em Key Vault/Secrets Manager
- [ ] Backups configurados
- [ ] Plano de resposta a incidentes documentado
- [ ] Testes de segurança automatizados
- [ ] Revisão de código focada em segurança
- [ ] Documentação de segurança atualizada

---

## Resposta a Incidentes

### Se suspeitar de comprometimento:

1. **Isolar Imediatamente**
   ```bash
   # Revogar acesso ao banco
   REVOKE ALL PRIVILEGES ON configr.* FROM 'configr_app'@'%';
   ```

2. **Investigar**
   - Revisar audit logs
   - Verificar configurações modificadas recentemente
   - Identificar acessos suspeitos

3. **Remediar**
   - Rotacionar todas as credenciais
   - Restaurar configurações de backup
   - Aplicar patches de segurança

4. **Documentar**
   - Post-mortem
   - Lições aprendidas
   - Melhorias necessárias

---

## Recursos Adicionais

- [OWASP Top 10](https://owasp.org/Top10/)
- [.NET Security Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [Azure Security Baseline](https://docs.microsoft.com/en-us/security/benchmark/azure/)
- [CIS Benchmarks](https://www.cisecurity.org/cis-benchmarks/)

---

## Contato de Segurança

**Email:** michel@leanwork.com.br
**Security Advisories:** https://github.com/mbanagouro/configr/security

---

**Lembre-se:** Segurança é um processo contínuo, não um produto final!
