# Guia de Contribuição

Agradecemos seu interesse em contribuir para o ConfigR! Este guia vai ajudar você a colaborar efetivamente.

## Como Contribuir

### 1. Fork e Clone

```bash
# Fork o repositório no GitHub
git clone https://github.com/seu-usuario/configr.git
cd configr
```

### 2. Crie uma Branch

```bash
git checkout -b feature/minha-feature
# ou
git checkout -b bugfix/meu-bug
```

### 3. Faça Suas Mudanças

- Siga o estilo de código existente
- Adicione testes para novas funcionalidades
- Atualize a documentação se necessário

### 4. Commit e Push

```bash
git add .
git commit -m "Adicionar descrição clara e concisa"
git push origin feature/minha-feature
```

### 5. Abra um Pull Request

No GitHub, abra um PR descrevendo:
- O que você fez
- Por que fez
- Como testar

## Checklist antes de Submeter

- [ ] Código segue o estilo do projeto
- [ ] Testes foram adicionados/atualizados
- [ ] Documentação foi atualizada
- [ ] Build passa localmente (`dotnet build`)
- [ ] Testes passam (`dotnet test`)
- [ ] Não há warnings

## Executar Testes Localmente

### Testes Unitários

```bash
dotnet test
```

### Testes com Docker Compose

```bash
docker-compose up -d
dotnet test
docker-compose down
```

### Testes por Provider

```bash
# SQL Server
dotnet test --filter "TestCategory=SqlServer"

# MySQL
dotnet test --filter "TestCategory=MySql"

# PostgreSQL
dotnet test --filter "TestCategory=Npgsql"

# MongoDB
dotnet test --filter "TestCategory=MongoDB"

# Redis
dotnet test --filter "TestCategory=Redis"

# RavenDB
dotnet test --filter "TestCategory=RavenDB"
```

## Tipos de Contribuição

### Reportar Bugs

Abra uma issue com:
- Título descritivo
- Descrição clara do problema
- Passos para reproduzir
- Comportamento esperado
- Comportamento atual
- Ambiente (SO, versão .NET, etc)

### Sugerir Features

Abra uma discussion ou issue com:
- Caso de uso
- Benefícios
- Exemplos de uso
- Possível implementação

### Melhorar Documentação

- Corrija typos
- Adicione exemplos
- Melhore clareza
- Traduza para outros idiomas

### Novos Providers

1. Crie um fork
2. Implemente `IConfigStore`
3. Adicione testes completos
4. Atualize documentação
5. Abra um PR

## Padrões de Código

### Nomenclatura

- Classes: `PascalCase`
- Métodos: `PascalCase`
- Variáveis: `camelCase`
- Constantes: `UPPER_SNAKE_CASE`

### Exemplo

```csharp
public sealed class MyConfigStore : IConfigStore
{
    private readonly string _connectionString;
    private const int DEFAULT_TIMEOUT = 30;

    public async Task<string?> GetAsync(string key, string? scope = null)
    {
        // Implementação
    }
}
```

### Documentação de Código

```csharp
/// <summary>
/// Recupera uma configuração do armazenamento.
/// </summary>
/// <param name="key">Chave da configuração</param>
/// <param name="scope">Escopo opcional</param>
/// <returns>Valor da configuração ou null</returns>
public async Task<string?> GetAsync(string key, string? scope = null);
```

## Segurança

Se encontrar uma vulnerabilidade de segurança:

1. **NÃO** abra uma issue pública
2. Envie um email para [security@configr.dev]
3. Descreva a vulnerabilidade
4. Aguarde a resposta

## Licença

Ao contribuir, você concorda que seu código será licenciado sob a MIT License.

## ?? Agradecimentos

Muito obrigado por contribuir! Seu trabalho ajuda a melhorar ConfigR para todos.

---

**Dúvidas?** Abra uma [discussion](https://github.com/mbanagouro/configr/discussions) no GitHub!
