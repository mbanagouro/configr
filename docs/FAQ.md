# ❓ Perguntas Frequentes (FAQ)

Respostas para perguntas comuns sobre ConfigR.

## 📦 Instalação e Setup

### P: Qual é a versão mínima de .NET suportada?

**R:** ConfigR suporta .NET 8.0 e superiores (.NET 8, 9, 10).

### P: Posso usar ConfigR com .NET Framework?

**R:** Não. ConfigR é exclusivamente para .NET moderno (.NET 8+).

### P: Qual provider devo escolher?

**R:** 
- **SQL Server**: Padrão, recomendado para maioria dos casos
- **PostgreSQL**: Se você já usa PostgreSQL
- **MySQL**: Mais leve, hosting barato
- **MongoDB**: Para dados semi-estruturados
- **Redis**: Para performance crítica
- **RavenDB**: Para segurança enterprise

### P: Posso usar múltiplos providers?

**R:** Não simultaneamente. Registre apenas um provider por aplicação.

## 🎯 Uso

### P: Posso usar múltiplas classes de configuração?

**R:** Sim! Você pode ter quantas classes quiser. Cada uma é armazenada com sua própria chave.

```csharp
var checkout = await configR.GetAsync<CheckoutConfig>();
var payment = await configR.GetAsync<PaymentConfig>();
var shipping = await configR.GetAsync<ShippingConfig>();
```

### P: Como lido com tipos complexos?

**R:** ConfigR serializa automaticamente para JSON. Use tipos JSON-serializable:

```csharp
public sealed class AdvancedConfig
{
    public string Name { get; set; }
    public int[] Ids { get; set; }
    public Dictionary<string, string> Settings { get; set; }
}
```

### P: Posso herdar de uma classe de configuração?

**R:** Não é recomendado. Use composição:

```csharp
// ❌ Evitar
public class MyConfig : BaseConfig { }

// ✅ Fazer
public class MyConfig
{
    public BaseSettings Base { get; set; }
    public MySpecificSettings Specific { get; set; }
}
```

### P: O que acontece se a configuração não existir?

**R:** Retorna uma nova instância com valores padrão da classe.

```csharp
// Se não existir CheckoutConfig no banco:
var config = await configR.GetAsync<CheckoutConfig>();
// Retorna: new CheckoutConfig { LoginRequired = true, MaxItems = 20 }
```

## ⏱️ Cache e Performance

### P: Como funciona o cache?

**R:** ConfigR caches em memória automaticamente. Cada configuração é cacheada por um tempo configurável (padrão: 5 minutos).

### P: Como invalido o cache?

**R:** Automaticamente quando você chama `SaveAsync()`:

```csharp
var config = await configR.GetAsync<MyConfig>();
config.Value = "new";
await configR.SaveAsync(config);  // Cache invalidado aqui!
```

### P: Posso desabilitar o cache?

**R:** Sim, configurando duração zero:

```csharp
builder.Services.AddConfigR(options =>
{
    options.CacheDuration = TimeSpan.Zero;
})
```

⚠️ Use com cuidado em produção!

### P: Como monitoro a performance?

**R:** Configure logging:

```csharp
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Debug);
});
```

## 🌐 Multi-tenant

### P: Como uso ConfigR em aplicação multi-tenant?

**R:** Use scopes:

```csharp
// Ler config de tenant específico
var tenantConfig = await configR.GetAsync<MyConfig>("tenant-123");

// Salvar config de tenant específico
await configR.SaveAsync(config, "tenant-123");
```

### P: Como extraio o tenant ID automaticamente?

**R:** Use middleware ou `IHttpContextAccessor`:

```csharp
public class ConfigService
{
    private readonly IConfigR _configR;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<MyConfig> GetConfig()
    {
        var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"]?.ToString();
        return await _configR.GetAsync<MyConfig>(tenantId);
    }
}
```

### P: Posso compartilhar configurações entre tenants?

**R:** Sim, usando configurações sem scope como "padrão" e fallback:

```csharp
var tenantConfig = await configR.GetAsync<MyConfig>("tenant-123");
if (tenantConfig == null)
{
    tenantConfig = await configR.GetAsync<MyConfig>();  // Padrão
}
```

## 🔒 Segurança

### P: As configurações são criptografadas?

**R:** Por padrão, não. Para criptografia:
- Implemente um custom serializer
- Use RavenDB (suporte nativo)
- Criptografe dados sensíveis antes de salvar

### P: Como lido com dados sensíveis?

**R:** 

1. **Não armazene em ConfigR**: Use Azure Key Vault, AWS Secrets Manager
2. **Criptografe**: Se precisar armazenar, criptografe antes
3. **Audit**: Implemente logging de acesso

```csharp
// Exemplo: Criptografar antes de salvar
var config = new MyConfig { ApiKey = Encrypt("my-secret") };
await configR.SaveAsync(config);

// Descriptografar ao ler
var saved = await configR.GetAsync<MyConfig>();
var apiKey = Decrypt(saved.ApiKey);
```

### P: Posso usar ConfigR com dados PII?

**R:** Não recomendado. ConfigR é para configurações de negócio, não dados pessoais.

## 🧪 Testes

### P: Como testo com ConfigR?

**R:** Use um provider em memória ou mock:

```csharp
// Mock
var mockConfigR = new Mock<IConfigR>();
mockConfigR.Setup(x => x.GetAsync<MyConfig>(null, default))
    .ReturnsAsync(new MyConfig { /* ... */ });

var service = new MyService(mockConfigR.Object);
```

### P: Como rodo testes de integração?

**R:** Use Docker Compose ou containers locais:

```bash
docker-compose up -d
dotnet test
docker-compose down
```

## 🔄 Migração

### P: Como migro de appsettings.json para ConfigR?

**R:**

1. Identifique configurações dinâmicas
2. Crie classes de configuração
3. Salve valores iniciais no banco
4. Remova de appsettings.json

```csharp
// Antes: appsettings.json
"MaxItems": 20

// Depois: Classe + ConfigR
public class CheckoutConfig
{
    public int MaxItems { get; set; } = 20;
}

await configR.SaveAsync(new CheckoutConfig());
```

### P: Posso coexistir appsettings.json e ConfigR?

**R:** Sim! Use ambos:

```csharp
// Valores estáticos do appsettings.json
var staticConfig = configuration.GetSection("StaticSettings");

// Valores dinâmicos do ConfigR
var dynamicConfig = await configR.GetAsync<DynamicSettings>();
```

## 📖 Documentação

### P: Onde encontro documentação?

**R:** Em https://mbanagouro.github.io/configr

### P: A documentação está em qual idioma?

**R:** Português (pt-BR) e inglês (interface material).

### P: Como contribuo para a documentação?

**R:** Veja [Guia de Contribuição](CONTRIBUTING.md)

## 🔧 Troubleshooting

### P: Recebo erro "Connection refused"

**R:** 
1. Verifique se banco de dados está rodando
2. Confirme connection string
3. Teste conectividade: `ping localhost`

### P: Erro "Tabela não existe"

**R:** Crie a tabela manualmente ou configure `AutoCreateTable = true`.

### P: Erro "Serialização falhou"

**R:** 
- Use tipos JSON-serializable
- Classe precisa de construtor sem parâmetros
- Propriedades devem ser públicas

### P: Performance está lenta

**R:**
1. Verifique cache (deve estar ativo)
2. Analise queries no banco
3. Considere aumentar `CacheDuration`

## ❓ Ainda tem dúvidas?

- 💬 Abra uma [discussion](https://github.com/mbanagouro/configr/discussions)
- 🐛 Reporte um [bug](https://github.com/mbanagouro/configr/issues)
- 📧 Entre em contato com autor

---

**Atualizado:** Dezembro 2024
