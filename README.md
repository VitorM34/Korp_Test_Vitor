# Korp_Teste_Vitor — Sistema de Emissão de Notas Fiscais

Projeto técnico desenvolvido como parte do processo seletivo da Korp.

## Tecnologias

| Camada | Stack |
|--------|-------|
| Frontend | Angular 18 + Angular Material |
| Backend | C# / .NET 10 (microsserviços) |
| Banco de dados | PostgreSQL (um por microsserviço) |
| Infraestrutura | Docker Compose |
| Resiliência | Polly v8 (via `Microsoft.Extensions.Http.Resilience`) |
| ORM | Entity Framework Core 10 |

## Arquitetura

```
Angular SPA (porta 4200)
    ↕ HTTP
┌──────────────┐   HTTP   ┌─────────────────┐
│ Estoque.Api  │◄─────────│ Faturamento.Api │
│ (porta 5001) │          │ (porta 5002)    │
└──────┬───────┘          └────────┬────────┘
       │                           │
  EstoqueDb                  FaturamentoDb
  (PostgreSQL)               (PostgreSQL)
```

Cada microsserviço possui seu próprio banco de dados (*database-per-service*).
A comunicação entre serviços é exclusivamente via HTTP com Polly (retry + circuit breaker).

## Como executar

### Com Docker Compose (recomendado)

```bash
docker compose up --build
```

Acesse: http://localhost:4200

### Desenvolvimento local

**Backend — Estoque.Api:**
```bash
cd backend/Estoque.Api
dotnet run
# Swagger: http://localhost:5001/swagger
```

**Backend — Faturamento.Api:**
```bash
cd backend/Faturamento.Api
dotnet run
# Swagger: http://localhost:5002/swagger
```

**Frontend:**
```bash
cd frontend/notas-fiscais-app
npm install
npm start
# http://localhost:4200
```

> Requer PostgreSQL local. Ajuste as strings de conexão em `appsettings.Development.json`.

## Demonstração do cenário de falha

```bash
# Derrubar o serviço de estoque
docker stop estoque-api

# Tentar imprimir uma nota no Angular
# → O circuit breaker (Polly) abre após as tentativas
# → Angular exibe mensagem: "Serviço de estoque indisponível"

# Restaurar
docker start estoque-api
```

## Funcionalidades

- ✅ Cadastro de produtos (código, descrição, saldo)
- ✅ Listagem de produtos com saldo em tempo real
- ✅ Criação de notas fiscais (numeração sequencial automática)
- ✅ Inclusão de itens na nota (produto + quantidade)
- ✅ Impressão de nota: debita estoque, fecha nota, exibe indicador de progresso
- ✅ Tratamento de falhas com circuit breaker (Polly)
- ✅ Concorrência otimista (`RowVersion` no EF Core)
- ✅ Idempotência na baixa de saldo (`ChaveIdempotencia`)
- ✅ Compensação automática em falha parcial de impressão

## Detalhamento técnico

### Angular — ciclos de vida utilizados
- `ngOnInit`: carregar dados iniciais via `HttpClient`
- `ngOnDestroy`: unsubscribe via `Subject + takeUntil` (prevenção de memory leak)

### RxJS utilizado
- `pipe`, `tap`, `catchError`, `EMPTY`, `switchMap`: no fluxo de impressão
- `takeUntil`: gerenciamento de subscriptions
- Lazy loading de rotas: `loadComponent` com imports dinâmicos

### Angular Material
- `MatTable` — listagens de produtos e notas
- `MatFormField` + `MatInput` + `MatSelect` — formulários com Reactive Forms
- `MatProgressSpinner` — indicador de carregamento/impressão
- `MatSnackBar` — feedback de sucesso/erro
- `MatToolbar`, `MatButton`, `MatCard`, `MatIcon` — layout geral

### Backend C# — tratamento de erros
- `ExceptionHandlingMiddleware` global: captura todas as exceções não tratadas
- Exceções de domínio tipadas: `SaldoInsuficienteException`, `NotaFiscalFechadaException`, etc.
- Resposta no padrão `ProblemDetails` (RFC 7807) para todos os erros

### LINQ utilizado
```csharp
// Listagem ordenada com projeção para DTO
db.Produtos.OrderBy(p => p.Codigo)
           .Select(p => new ProdutoResponse(...))
           .ToListAsync()

// Próxima numeração sequencial
db.NotasFiscais.MaxAsync(n => n.Numeracao)

// Include para eager loading
db.NotasFiscais.Include(n => n.Itens).FirstOrDefaultAsync(n => n.Id == id)
```

### Frameworks e bibliotecas
- `Microsoft.Extensions.Http.Resilience` (Polly v8): retry e circuit breaker
- `Npgsql.EntityFrameworkCore.PostgreSQL`: provider PostgreSQL para EF Core
- `Swashbuckle.AspNetCore`: documentação Swagger/OpenAPI
