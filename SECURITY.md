# ?? Política de Segurança do ConfigR

## Versões Suportadas

| Versão | Suportada          |
| ------ | ------------------ |
| 1.x    | :white_check_mark: |
| < 1.0  | :x:                |

## Reportando Vulnerabilidades de Segurança

### ?? NÃO abra issues públicas para vulnerabilidades de segurança

Se você descobriu uma vulnerabilidade de segurança, por favor reporte de forma responsável:

**Email:** michel@leanwork.com.br (ou abra um Security Advisory no GitHub)

### Informações para incluir no reporte:

1. **Descrição da vulnerabilidade**
2. **Passos para reproduzir**
3. **Impacto potencial**
4. **Versões afetadas**
5. **Mitigações temporárias (se houver)**

### Processo de Resposta:

1. **Confirmação:** Responderemos em até 48 horas
2. **Avaliação:** Análise e classificação da severidade (1-5 dias úteis)
3. **Correção:** Desenvolvimento da correção
4. **Disclosure:** Publicação coordenada após correção estar disponível

## Melhores Práticas de Segurança

### ?? Nunca Faça:

1. **Armazene senhas ou secrets em ConfigR**
   ```csharp
   // ? NUNCA FAÇA ISSO
   public class AppConfig
   {
       public string DatabasePassword { get; set; }
       public string ApiSecret { get; set; }
   }
   ```
   **Use:** Azure Key Vault, AWS Secrets Manager, HashiCorp Vault

2. **Armazene dados PII (Personally Identifiable Information)**
   - ConfigR é para configurações de negócio, não dados pessoais
   - Violação da LGPD/GDPR

3. **Exponha ConfigR em APIs públicas sem autenticação**
   ```csharp
   // ? PERIGOSO
   [HttpGet("config")]
   [AllowAnonymous]
   public async Task<MyConfig> GetConfig() { }
   ```

### ? Sempre Faça:

1. **Use autenticação forte**
   ```csharp
   [Authorize(Policy = "AdminOnly")]
   [HttpPost("config")]
   public async Task UpdateConfig([FromBody] MyConfig config)
   {
       await _configR.SaveAsync(config);
   }
   ```

2. **Valide entradas**
   ```csharp
   public class CheckoutConfig
   {
       [Range(1, 1000)]
       public int MaxItems { get; set; }
       
       [StringLength(500)]
       public string Description { get; set; }
   }
   ```

3. **Use connection strings seguras**
   ```json
   {
     "ConnectionStrings": {
       "ConfigR": "Server=localhost;Database=configr;Integrated Security=true"
     }
   }
   ```

4. **Habilite TLS/SSL**
   ```csharp
   // SQL Server
   "TrustServerCertificate=False;Encrypt=True;"
   
   // MySQL
   "SslMode=Required;"
   
   // PostgreSQL
   "SSL Mode=Require;"
   ```

5. **Implemente audit logging**
   ```csharp
   public class AuditConfigR : IConfigR
   {
       private readonly IConfigR _inner;
       private readonly ILogger<AuditConfigR> _logger;
       
       public async Task SaveAsync<T>(T config)
       {
           _logger.LogWarning("Config {Type} modified by {User}", 
               typeof(T).Name, 
               _httpContext.User.Identity.Name);
           
           await _inner.SaveAsync(config);
       }
   }
   ```

## Configurações de Segurança Recomendadas

### SQL Server

```sql
-- Crie um usuário específico com permissões mínimas
CREATE LOGIN configr_app WITH PASSWORD = 'StrongPassword123!';
CREATE USER configr_app FOR LOGIN configr_app;

-- Permissões apenas nas tabelas necessárias
GRANT SELECT, INSERT, UPDATE ON dbo.ConfigR TO configr_app;

-- Remova permissões de DDL
REVOKE CREATE TABLE, DROP TABLE, ALTER FROM configr_app;
```

### MySQL

```sql
-- Usuário com privilégios limitados
CREATE USER 'configr_app'@'localhost' IDENTIFIED BY 'StrongPassword123!';
GRANT SELECT, INSERT, UPDATE ON configr.* TO 'configr_app'@'localhost';
FLUSH PRIVILEGES;
```

### PostgreSQL

```sql
-- Role específico
CREATE ROLE configr_app WITH LOGIN PASSWORD 'StrongPassword123!';
GRANT SELECT, INSERT, UPDATE ON configr.configr TO configr_app;
```

### Redis

```bash
# redis.conf
requirepass "YourStrongPasswordHere"
protected-mode yes
bind 127.0.0.1

# Use connection string com senha
"localhost:6379,password=YourStrongPasswordHere,ssl=true"
```

### MongoDB

```javascript
// Crie usuário com permissões mínimas
use ConfigR
db.createUser({
  user: "configr_app",
  pwd: "StrongPassword123!",
  roles: [
    { role: "readWrite", db: "ConfigR" }
  ]
})

// Connection string
"mongodb://configr_app:StrongPassword123!@localhost:27017/ConfigR?authSource=ConfigR"
```

## Criptografia de Dados Sensíveis

Se você **absolutamente precisar** armazenar dados sensíveis no ConfigR:

```csharp
public class SecureConfigSerializer : IConfigSerializer
{
    private readonly IDataProtector _protector;
    
    public SecureConfigSerializer(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("ConfigR.SecureData");
    }
    
    public string Serialize(object? value)
    {
        if (value is null) return string.Empty;
        
        var json = JsonSerializer.Serialize(value);
        return _protector.Protect(json);
    }
    
    public object? Deserialize(string serializedValue, Type targetType)
    {
        if (string.IsNullOrEmpty(serializedValue))
            return Activator.CreateInstance(targetType);
        
        var json = _protector.Unprotect(serializedValue);
        return JsonSerializer.Deserialize(json, targetType);
    }
}

// Registrar
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IConfigSerializer, SecureConfigSerializer>();
```

## Checklist de Segurança

Antes de usar ConfigR em produção:

- [ ] Connection strings não têm senhas hardcoded
- [ ] TLS/SSL está habilitado
- [ ] Usuário do banco tem apenas permissões necessárias
- [ ] Autenticação está habilitada em endpoints de configuração
- [ ] Validação de entrada está implementada
- [ ] Audit logging está configurado
- [ ] Rate limiting está configurado
- [ ] Configurações não contêm dados sensíveis
- [ ] Backups estão configurados
- [ ] Monitoramento de alterações está ativo

## Dependências e CVEs

ConfigR usa as seguintes dependências. Mantenha-as atualizadas:

```bash
# Verificar vulnerabilidades conhecidas
dotnet list package --vulnerable --include-transitive
```

**Dependências principais:**
- Microsoft.Data.SqlClient
- MySqlConnector
- Npgsql
- StackExchange.Redis
- MongoDB.Driver
- RavenDB.Client

## Recursos Adicionais

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [.NET Security Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [SQL Injection Prevention](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html)

## Contato

Para questões de segurança: security@configr.dev

**Agradecemos relatos responsáveis de vulnerabilidades!**

---

**Última atualização:** Janeiro 2025
