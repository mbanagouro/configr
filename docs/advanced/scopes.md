# Scopes - Multi-tenant

Entenda como usar scopes para isolar configurações em aplicações multi-tenant.

## ?? O que é um Scope?

Um **scope** é um identificador que permite isolar configurações por contexto. Perfeito para:

- **Multi-tenant**: Cada cliente tem suas próprias configurações
- **Multi-loja**: Cada loja tem suas próprias regras
- **Ambientes**: Produção, staging, desenvolvimento com configurações diferentes
- **Regiões**: Diferentes configurações por localização

## ?? Uso Básico

### Salvar com Scope

```csharp
var config = new CheckoutConfig
{
    LoginRequired = true,
    MaxItems = 20
};

// Salvar configuração para o tenant "acme-corp"
await _configR.SaveAsync(config, scope: "acme-corp");

// Salvar configuração padrão (sem scope)
await _configR.SaveAsync(config);
```

### Ler com Scope

```csharp
// Ler configuração do tenant "acme-corp"
var acmeConfig = await _configR.GetAsync<CheckoutConfig>("acme-corp");

// Ler configuração padrão
var defaultConfig = await _configR.GetAsync<CheckoutConfig>();
```

## ?? Padrão: Fallback para Padrão

Implemente um padrão de fallback para configurações não encontradas:

```csharp
public async Task<CheckoutConfig> GetCheckoutConfig(string tenantId)
{
    try
    {
        // Tentar ler config específica do tenant
        return await _configR.GetAsync<CheckoutConfig>(tenantId);
    }
    catch (ConfigNotFoundException)
    {
        // Fallback para configuração padrão
        return await _configR.GetAsync<CheckoutConfig>();
    }
}
```

## ?? Exemplo: Multi-tenant em Middleware

```csharp
// Middleware para extrair tenant do header
app.Use(async (context, next) =>
{
    var tenantId = context.Request.Headers["X-Tenant-Id"];
    context.Items["TenantId"] = tenantId;
    await next();
});

// Usar em service
public class CheckoutService
{
    private readonly IConfigR _configR;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CheckoutService(IConfigR configR, IHttpContextAccessor httpContextAccessor)
    {
        _configR = configR;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CheckoutConfig> GetConfig()
    {
        var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"]?.ToString();
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            return await _configR.GetAsync<CheckoutConfig>(tenantId);
        }

        return await _configR.GetAsync<CheckoutConfig>();
    }
}
```

## ??? Estrutura no Banco de Dados

Os scopes são armazenados junto com a chave no banco:

### SQL Server
```sql
-- Configuração padrão (sem scope)
SELECT * FROM ConfigR WHERE Key = 'CheckoutConfig' AND Scope IS NULL;

-- Configuração de um tenant específico
SELECT * FROM ConfigR WHERE Key = 'CheckoutConfig' AND Scope = 'acme-corp';

-- Todas as configurações de um tenant
SELECT * FROM ConfigR WHERE Scope = 'acme-corp';
```

## ?? Listar Scopes Disponíveis

Para recuperar todos os scopes cadastrados:

```csharp
// Implementar em seu provider ou consultar diretamente:
var sql = "SELECT DISTINCT Scope FROM ConfigR WHERE Scope IS NOT NULL";
```

## ?? Boas Práticas

### ? Faça

- Use nomes significativos: `"acme-corp"`, `"loja-sp"`, `"producao"`
- Sempre implemente fallback para padrão
- Documente quais scopes sua app usa
- Use UUID ou identificadores únicos para tenants

### ? Evite

- Nomes ambíguos: `"tenant1"`, `"config2"`
- Esquecer de gerenciar o ciclo de vida dos scopes
- Misturar separação de dados com scopes (use multitenancy adequadamente)
- Armazenar dados sensíveis em scopes públicos

## ?? Segurança

```csharp
// ? Sempre validar se o tenant tem acesso
public async Task<CheckoutConfig> GetTenantConfig(string tenantId)
{
    // Validar se o usuário atual tem permissão para este tenant
    if (!await AuthorizeForTenant(tenantId))
    {
        throw new UnauthorizedAccessException();
    }

    return await _configR.GetAsync<CheckoutConfig>(tenantId);
}
```

## ?? Próximos Passos

- ?? [Otimize com Cache](caching.md)
- ?? [Crie Providers Personalizados](extensibility.md)
- ?? [Voltar para Configuração](../configuration.md)