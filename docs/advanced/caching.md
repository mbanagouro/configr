# Cache - Performance

Entenda como o cache em memória do ConfigR funciona e como otimizá-lo.

## ?? Como Funciona o Cache

O ConfigR implementa cache em memória automático para melhorar performance:

```
???????????????????????
?   Sua Aplicação     ?
???????????????????????
           ?
      Solicita config
           ?
           ?
???????????????????????
?   Cache em Memória  ????? Rápido! (< 1ms)
?      (IMemoryCache) ?
???????????????????????
           ? Se não encontrado
           ?
???????????????????????
?   Backend Storage   ????? Mais lento (~50ms)
?   (SQL, MySQL, etc) ?
???????????????????????
```

## ?? Duração do Cache

Cada configuração é cacheada por um tempo configurável. Após expirar, a próxima leitura busca do banco:

```csharp
// Configurar duração do cache durante o registro
builder.Services
    .AddConfigR(options =>
    {
        options.CacheDuration = TimeSpan.FromMinutes(5); // Padrão
    })
    .UseSqlServer(builder.Configuration.GetConnectionString("ConfigR"));
```

### Durações Recomendadas

| Tipo de Configuração | Duração | Motivo |
|---|---|---|
| Feature flags | 1-5 minutos | Mudam frequentemente |
| Configurações comerciais | 10-30 minutos | Mudanças ocasionais |
| Constantes | 1-2 horas | Raramente mudam |
| Valores críticos | 1 minuto | Precisam de atualização rápida |

## ?? Invalidação de Cache

Quando você salva uma configuração, o cache é **automaticamente invalidado**:

```csharp
// Lê do cache
var config = await _configR.GetAsync<CheckoutConfig>();

// Atualiza e salva
config.MaxItems = 50;
await _configR.SaveAsync(config);  // ??? Cache invalidado aqui!

// Próxima leitura vem do banco
var newConfig = await _configR.GetAsync<CheckoutConfig>();  // Sempre fresco
```

## ?? Estratégias de Cache

### 1?? Cache Curto (Alta Disponibilidade)

```csharp
// Atualiza a cada 1 minuto
builder.Services
    .AddConfigR(options =>
    {
        options.CacheDuration = TimeSpan.FromMinutes(1);
    })
    .UseSqlServer(connectionString);
```

**Quando usar:**
- Valores que mudam frequentemente
- Feature flags ativas
- Configurações críticas

### 2?? Cache Longo (Máxima Performance)

```csharp
// Cacheado por 1 hora
builder.Services
    .AddConfigR(options =>
    {
        options.CacheDuration = TimeSpan.FromHours(1);
    })
    .UseSqlServer(connectionString);
```

**Quando usar:**
- Configurações que raramente mudam
- Constantes da aplicação
- Valores de inicialização

### 3?? Sem Cache (Sempre Fresco)

```csharp
// Nunca cacheia
builder.Services
    .AddConfigR(options =>
    {
        options.CacheDuration = TimeSpan.Zero;
    })
    .UseSqlServer(connectionString);
```

**?? Use com cuidado:** Pode sobrecarregar o banco de dados!

## ?? Monitore o Cache

### Logs

```csharp
// Configure logging para debug
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Debug);
});
```

### Métricas

Implemente coleta de métricas:

```csharp
public class CacheMetrics
{
    public int Hits { get; set; }
    public int Misses { get; set; }
    public decimal HitRatio => (decimal)Hits / (Hits + Misses);
}
```

## ?? Boas Práticas

### ? Faça

- Escolha duração apropriada para seu caso de uso
- Monitore hit/miss ratio do cache
- Invalide cache quando necessário
- Documente estratégia de cache

### ? Evite

- Cache muito longo para dados críticos
- Cache zero em produção (performance)
- Misturar estratégias sem motivo
- Confiar exclusivamente no cache

## ?? Otimizações

### Usar Diferentes Durações por Tipo

```csharp
public class ConfigService
{
    private readonly IConfigR _configR;

    public async Task<CheckoutConfig> GetCheckoutConfig()
    {
        // Esta chamada é cacheada
        return await _configR.GetAsync<CheckoutConfig>();
    }

    public async Task<PaymentConfig> GetPaymentConfig()
    {
        // Esta também, com sua própria cache
        return await _configR.GetAsync<PaymentConfig>();
    }
}
```

### Precarga de Cache

```csharp
// No startup, precarre configurações críticas
var app = builder.Build();

var configR = app.Services.GetRequiredService<IConfigR>();
await configR.GetAsync<CheckoutConfig>();      // Precarga
await configR.GetAsync<PaymentConfig>();       // Precarga
await configR.GetAsync<ShippingConfig>();      // Precarga
```

## ?? Próximos Passos

- ?? [Aprenda sobre Scopes](scopes.md)
- ?? [Crie Providers Personalizados](extensibility.md)
- ?? [Voltar para Configuração](../configuration.md)