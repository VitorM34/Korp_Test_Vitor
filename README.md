![Banner do projeto](docs/banner.png)

# Korp_Teste_Vitor — Sistema de Emissão de Notas Fiscais

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-18-DD0031?logo=angular&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

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
│ Inventory.Api│◄─────────│ Billing.Api     │
│ (porta 5001) │          │ (porta 5002)    │
└──────┬───────┘          └────────┬────────┘
       │                           │
   inventory                    billing
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

Suba apenas os bancos de dados via Docker Compose (as APIs já vêm pré-configuradas em `appsettings.Development.json` para apontar para eles):
```bash
docker compose up -d inventory-db billing-db
```

**Backend — Inventory.Api:**
```bash
cd backend/Inventory.Api
dotnet run
# Swagger: http://localhost:5001/swagger
```

**Backend — Billing.Api:**
```bash
cd backend/Billing.Api
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


## Demonstração do cenário de falha

```bash
# Derrubar o serviço de estoque
docker stop inventory-api

# Tentar imprimir uma nota no Angular
# → O circuit breaker (Polly) abre após as tentativas
# → Angular exibe mensagem: "Serviço de estoque indisponível"

# Restaurar
docker start inventory-api
```

## Funcionalidades

- ✅ Cadastro de produtos (código, descrição, saldo)
- ✅ Listagem de produtos com saldo em tempo real
- ✅ Criação de notas fiscais (numeração sequencial automática)
- ✅ Inclusão de itens na nota (produto + quantidade)
- ✅ Impressão de nota: debita estoque, fecha nota, exibe indicador de progresso
- ✅ Tratamento de falhas com circuit breaker (Polly)
- ✅ Concorrência otimista (coluna de sistema `xmin` do PostgreSQL como token de concorrência no EF Core)
- ✅ Idempotência na baixa de saldo (`IdempotencyKey`)
- ✅ Compensação automática em falha parcial de impressão

## Detalhamento técnico

### Angular — ciclos de vida utilizados
- `ngOnInit`: carregar dados iniciais via `HttpClient`
- `DestroyRef.onDestroy`: limpeza de estado/efeitos colaterais ao destruir o componente (substitui `ngOnDestroy` + `Subject/takeUntil`)

### RxJS utilizado
- `pipe`, `tap`, `catchError`, `EMPTY`, `switchMap`: no fluxo de impressão
- Requisições via `HttpClient` completam sozinhas (sem necessidade de unsubscribe manual); `DestroyRef.onDestroy` cobre limpeza de estado compartilhado (ex.: `pageContext`)
- Lazy loading de rotas: `loadComponent` com imports dinâmicos

### Angular Material
- `MatTable` — listagens de produtos e notas
- `MatFormField` + `MatInput` + `MatSelect` — formulários com Reactive Forms
- `MatProgressSpinner` — indicador de carregamento/impressão
- `MatSnackBar` — feedback de sucesso/erro
- `MatToolbar`, `MatButton`, `MatCard`, `MatIcon` — layout geral

### Backend C# — tratamento de erros
- `GlobalExceptionHandler` (`IExceptionHandler`, nativo do ASP.NET Core): captura todas as exceções não tratadas
- Exceções de domínio tipadas: `InsufficientBalanceException`, `ProductNotFoundException`, `InvoiceClosedException`, etc., mapeadas para o status HTTP correto
- Resposta no padrão `ProblemDetails` (RFC 9457) para todos os erros, incluindo conflitos de concorrência (`DbUpdateConcurrencyException` → 409)

### LINQ utilizado
```csharp
// Listagem ordenada com projeção para DTO
db.Products.OrderBy(p => p.Code)
           .Select(p => new ProductResponse(...))
           .ToListAsync()

// Próxima numeração sequencial
db.Invoices.MaxAsync(n => n.Number)

// Include para eager loading
db.Invoices.Include(n => n.Items).FirstOrDefaultAsync(n => n.Id == id)
```

### Frameworks e bibliotecas
- `Microsoft.Extensions.Http.Resilience` (Polly v8): retry e circuit breaker
- `Npgsql.EntityFrameworkCore.PostgreSQL`: provider PostgreSQL para EF Core
- `Swashbuckle.AspNetCore`: documentação Swagger/OpenAPI
