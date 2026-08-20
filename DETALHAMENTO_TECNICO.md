# Detalhamento técnico

Este documento responde, item a item, ao que o documento de especificação do
teste técnico pede na seção "detalhamento técnico". Para instruções de como
rodar o projeto, ver o [README](README.md).

---

## 1. Ciclos de vida do Angular utilizados

- **`ngOnInit`** — usado nos componentes de tela (produtos, lista de notas,
  detalhe de nota) para carregar os dados iniciais via `HttpClient` assim que
  o componente é montado.
  Exemplo: `frontend/notas-fiscais-app/src/app/features/invoices/invoice-detail.component.ts`, linha 85.

- **`DestroyRef.onDestroy`** — usado no lugar do tradicional `ngOnDestroy`.
  É a forma recomendada desde o Angular 16 para registrar lógica de limpeza
  sem precisar implementar uma interface de ciclo de vida. No projeto, limpa
  o estado compartilhado de breadcrumbs (`pageContext`) quando o usuário sai
  da tela de detalhe da nota:

```ts
ngOnInit(): void {
  this.destroyRef.onDestroy(() => this.pageContext.clear());
  ...
}
```

  Arquivo: `invoice-detail.component.ts`, linha 85.

---

## 2. Uso da biblioteca RxJS

Sim, RxJS é usado nos fluxos que são assincronos por natureza: chamadas HTTP
e eventos de navegação de rota. Para estado local simples, o projeto usa
Signals nativos do Angular (`signal`/`computed`) em vez de RxJS.

**Fluxo de impressão de nota** (`invoice-detail.component.ts`, método `print()`):

```ts
ref.afterClosed().pipe(
  filter(confirmed => !!confirmed),
  tap(() => { this.printing.set(true); this.printError.set(''); }),
  switchMap(() => this.invoiceService.print(this.invoice()!.id).pipe(
    tap(() => this.printing.set(false)),
    catchError(err => { /* trata erro */ return EMPTY; })
  ))
).subscribe(updatedInvoice => { /* atualiza estado */ });
```

- `filter`: interrompe o fluxo se o usuário cancelar o diálogo de confirmação.
- `tap`: liga/desliga o indicador de carregamento como efeito colateral, sem
  alterar o valor emitido.
- `switchMap` (e não `mergeMap`, propositalmente): se o usuário clicar duas
  vezes seguidas em "imprimir", cancela a chamada anterior e considera só a
  mais recente — evita duas requisições de impressão concorrentes para a
  mesma nota.
- `catchError` + `EMPTY`: captura o erro HTTP sem quebrar o observable.

**Interceptor HTTP funcional** (`core/interceptors/error.interceptor.ts`):
usa `catchError`/`throwError` para tratar erros de toda a aplicação em um
único lugar, mapeando os status HTTP 0, 400, 404, 409, 422 e 503 para
mensagens amigáveis exibidas em `MatSnackBar`.

**Conversão de Observable em Signal** (`app.component.ts`):

```ts
private readonly url = toSignal(
  this.router.events.pipe(
    filter((e): e is NavigationEnd => e instanceof NavigationEnd),
    map(e => e.urlAfterRedirects),
    startWith(this.router.url)
  ),
  { initialValue: this.router.url }
);
```

Usa `toSignal` para transformar os eventos de navegação do `Router` (um
Observable) em um Signal: `filter` seleciona apenas o evento de fim de
navegação (`NavigationEnd`), `map` extrai a URL, e `startWith` garante um
valor inicial antes da primeira navegação. Isso alimenta os breadcrumbs da
aplicação sem gerenciamento manual de `subscribe`/`unsubscribe`.

Como as chamadas via `HttpClient` completam sozinhas (não são streams
infinitos), não há necessidade de unsubscribe manual nelas; o `DestroyRef`
cobre a limpeza do estado compartilhado que sobrevive entre navegações.

---

## 3. Outras bibliotecas utilizadas e finalidade

| Biblioteca | Finalidade |
|---|---|
| `@angular/material` + `@angular/cdk` | Componentes de UI prontos e acessíveis (ver item 4) |
| `material-icons` | Conjunto de ícones, usado via `<span class="material-icons">` |
| `rxjs` | Fluxos assíncronos (HTTP, eventos de rota) — ver item 2 |
| `zone.js` | Detecção de mudanças do Angular (padrão do CLI) |
| `tslib` | Helpers de compilação TypeScript |

No backend (C#): ver item 6.

---

## 4. Bibliotecas de componentes visuais

**Angular Material 18 + Angular CDK**, usados nos componentes de tela:

- `MatFormField`, `MatInput`, `MatSelect` — formulários (Reactive Forms) de
  cadastro de produto e de itens da nota
- `MatButton`, `MatIcon` — ações e ícones
- `MatProgressSpinner` — indicador de carregamento durante a impressão da nota
- `MatSnackBar` — feedback de sucesso/erro
- `MatDialog` — confirmação antes de ações destrutivas e antes de imprimir
- `MatDivider` — separadores visuais
- `MatRipple` (Angular CDK) — feedback tátil de clique na navegação lateral

As listagens de produtos e de notas fiscais **não** usam `MatTable`: são
implementadas com o control flow nativo do Angular (`@for`) e layout próprio
em CSS, o que deu mais liberdade de estilização para o layout do dashboard.
O cabeçalho/menu lateral também é um layout customizado (CSS próprio), não
`MatToolbar`/`MatCard`.

---

## 5. Gerenciamento de dependências no Golang

Não se aplica: o backend foi implementado em **C#/.NET**, não em Golang (o
desafio permitia optar entre as duas linguagens — justificativa da escolha
no item 6). No backend em C#, o gerenciamento de dependências é feito via
**NuGet**, com cada microsserviço mantendo seu próprio arquivo `.csproj`
(`backend/Billing.Api/Billing.Api.csproj`, `backend/Inventory.Api/Inventory.Api.csproj`).

No frontend (Angular), o gerenciamento de dependências é feito via **npm**,
com `package.json`/`package-lock.json`.

---

## 6. Frameworks utilizados (Golang ou C#)

Optei por **C#/.NET 10**. Principais frameworks/bibliotecas:

- **ASP.NET Core Web API** (minimal hosting, sem `Startup.cs`) — framework
  web dos dois microsserviços, `Inventory.Api` e `Billing.Api`.
- **Entity Framework Core 10** com provider `Npgsql.EntityFrameworkCore.PostgreSQL`
  — ORM para persistência em PostgreSQL, um banco por microsserviço
  (*database-per-service*).
- **`Microsoft.Extensions.Http.Resilience`** (Polly v8) — retry e circuit
  breaker na comunicação HTTP entre `Billing.Api` e `Inventory.Api`
  (detalhes no item 7 e na seção de diferenciais).
- **Swashbuckle.AspNetCore** — documentação Swagger/OpenAPI.

### Por que C# e não Go

O desafio permitia as duas linguagens. Go tem vantagens reais para
microsserviços — binário nativo sem runtime pesado, startup quase
instantâneo, footprint de memória menor, concorrência leve com goroutines.
Já trabalhei com Go em projetos pequenos e reconheço essas vantagens para
esse cenário.

Optei por C# por ter mais profundidade e produtividade na linguagem e no
ecossistema .NET (Entity Framework Core, Polly, recursos modernos como
*primary constructors* e pattern matching), o que permitiu, no tempo
disponível, entregar concorrência otimista, idempotência e compensação de
falha implementados de forma mais sólida do que se tivesse gasto parte do
tempo se adaptando a uma linguagem que domino menos. Foi uma escolha
pragmática de produtividade, não uma limitação de Go para o caso de uso —
inclusive o .NET vem reduzindo a diferença histórica de startup/footprint
com Native AOT (não utilizado neste projeto).

### Por que microsserviços e não monolito

Além de ser requisito obrigatório do desafio, Estoque e Faturamento são
contextos de negócio diferentes, com regras e ciclos de vida próprios —
separá-los permite desenvolver, testar, escalar e implantar cada um de
forma independente. O ponto mais relevante para este desafio é o
**isolamento de falha**: se o serviço de Estoque cair, o serviço de
Faturamento continua no ar e consegue dar um retorno claro ao usuário, em
vez de a aplicação inteira cair junto (ver cenário de falha simulado no
README). Em um monolito, um problema no módulo de Estoque rodaria no mesmo
processo do Faturamento e poderia derrubá-lo junto.

---

## 7. Tratamento de erros e exceções no backend

- **Exceções de domínio customizadas** para cada regra de negócio que pode
  falhar — `InvoiceNotFoundException`, `InvoiceClosedException`,
  `InvoiceWithoutItemsException`, `InsufficientStockException`, `ProductNotFoundException`,
  entre outras (`backend/Billing.Api/Domain/Exceptions/` e equivalente em
  `Inventory.Api`). A exceção é lançada diretamente de dentro da entidade de
  domínio quando a regra é violada — a regra de negócio vive na entidade, não
  espalhada por controller/service:

```csharp
// backend/Billing.Api/Domain/Entities/Invoice.cs, linhas 57-67
public void Close()
{
    if (Status == InvoiceStatus.Closed)
        throw new InvoiceClosedException(Id);
    if (_items.Count == 0)
        throw new InvoiceWithoutItemsException(Id);
    Status = InvoiceStatus.Closed;
    PrintedDate = DateTime.UtcNow;
}
```

- **`GlobalExceptionHandler`** (`backend/Billing.Api/Middleware/GlobalExceptionHandler.cs`
  e equivalente em `Inventory.Api`) implementa a interface nativa do
  ASP.NET Core `IExceptionHandler`, registrada uma vez no `Program.cs`
  (`AddExceptionHandler<GlobalExceptionHandler>()`). Centraliza a captura de
  todas as exceções não tratadas e mapeia cada tipo para o status HTTP
  correto, respondendo no padrão **`ProblemDetails`** (RFC 9457):

```csharp
InvoiceNotFoundException e     => 404 "Nota fiscal não encontrada"
InvoiceClosedException e       => 409 "Nota fiscal fechada"
InsufficientStockException e   => 422 "Saldo insuficiente"
DbUpdateConcurrencyException   => 409 "Conflito de concorrência"
BrokenCircuitException /
TimeoutRejectedException /
HttpRequestException           => 503 "Serviço de estoque indisponível"
_                               => 500 "Erro interno"
```

  Quando o Polly detecta o circuito aberto ou timeout na chamada entre
  `Billing.Api` e `Inventory.Api`, a exceção resultante (`BrokenCircuitException`/
  `TimeoutRejectedException`) é capturada especificamente e traduzida em um
  503 com mensagem clara para o usuário, em vez de um 500 genérico.

---

## 8. Uso de LINQ (C#)

```csharp
// Numeração sequencial da nota — backend/Billing.Api/Application/Services/InvoiceService.cs, CreateAsync
var nextNumber = await db.Invoices.AnyAsync(ct)
    ? await db.Invoices.MaxAsync(n => n.Number, ct) + 1
    : 1;
```

```csharp
// Listagem projetada em DTO, só leitura — ProductService.cs, ListAsync
await db.Products.AsNoTracking().OrderBy(p => p.Code)
    .Select(p => new ProductResponse(p.Id, p.Code, p.Description, p.Balance))
    .ToListAsync(ct);
```

`Select` projeta a entidade direto em DTO, sem trazer campos a mais do
banco. `AsNoTracking()` evita que o EF Core rastreie as entidades para
update em consultas somente-leitura, reduzindo uso de memória.

```csharp
// Join explícito — InvoiceService.cs, ListPrintLogAsync
await db.InvoicePrintLogs.AsNoTracking().OrderByDescending(l => l.PrintedDate)
    .Join(db.Invoices.AsNoTracking(), l => l.InvoiceId, n => n.Id,
          (l, n) => new PrintLogResponse(l.Id, n.Id, n.Number, l.PrintedDate))
    .ToListAsync(ct);
```

Também usados: `Include` (eager loading, ex.:
`db.Invoices.Include(n => n.Items).FirstOrDefaultAsync(n => n.Id == id)`) e
`FirstOrDefaultAsync`.

---

## 9. Requisitos opcionais implementados

O documento de especificação lista três diferenciais opcionais: tratamento
de concorrência, uso de Inteligência Artificial, e idempotência. Foram
implementados **dois dos três**: concorrência e idempotência. IA **não**
foi implementada — ver ideias de integração possível no item 10.

### Concorrência otimista

Uso a coluna de sistema `xmin` do PostgreSQL (mantida automaticamente pelo
Postgres, muda a cada `UPDATE` na linha) mapeada como *concurrency token* no
EF Core, tanto na entidade `Invoice` (`BillingDbContext.cs`, linhas 24-28)
quanto na entidade `Product` (`InventoryDbContext.cs`, linhas 22-26):

```csharp
entity.Property(n => n.XMin)
    .HasColumnType("xid")
    .HasColumnName("xmin")
    .IsConcurrencyToken()
    .ValueGeneratedOnAddOrUpdate();
```

Se dois usuários carregam o mesmo produto e tentam atualizar o saldo ao
mesmo tempo, o segundo a salvar recebe `DbUpdateConcurrencyException`
(capturada e traduzida em 409), evitando que ambos debitem o mesmo saldo
acreditando ter exclusividade.

### Idempotência

Cada baixa de saldo carrega uma chave de idempotência, montada como
`{invoiceId}-{productId}` (`InvoiceService.cs`, linha 114). Antes de
debitar, `Inventory.Api` (`ProductService.cs`, `DecreaseBalanceAsync`)
verifica se já existe um `IdempotencyRecord` com aquela chave; se existir,
devolve o resultado já processado em vez de debitar novamente. Isso importa
porque o Polly, no lado do `Billing.Api`, faz retry automático — se a baixa
tiver sucesso mas a resposta se perder na rede, o retry reenviaria a mesma
chamada, e sem essa trava o saldo seria debitado duas vezes para o mesmo
pedido lógico. A corrida entre duas requisições simultâneas com a mesma
chave é tratada capturando a violação de chave única (`Key` é *primary key*
da tabela `IdempotencyRecords`) e retornando o estado já processado pela
outra.

### Compensação de falha (aprofundamento do requisito obrigatório de tratamento de falha)

Não é um dos três opcionais — é como aprofundei o requisito **obrigatório**
de tratamento de falha entre os microsserviços, indo além de um retry
simples. Se uma nota tem vários itens e a baixa de estoque falha depois de
já terem sido debitados alguns, o `catch` em `InvoiceService.PrintAsync`
percorre os itens já debitados e devolve o saldo de cada um:

```csharp
catch (Exception ex) when (decreasedItems.Count > 0)
{
    await CompensateBalancesAsync(decreasedItems);
    throw;
}
```

É um padrão inspirado em Saga, em versão simples e local (sem orquestrador
dedicado) — evita ficar com saldo debitado sem a nota ter sido
efetivamente fechada.

---

## 10. Possíveis evoluções futuras

Não implementadas neste projeto, mas caminhos naturais dado mais tempo:

- **Testes automatizados**: hoje não há suíte de testes (nem xUnit no
  backend, nem specs no frontend). Os services já são desenhados com
  interfaces (`IInventoryApiClient`) justamente para permitir testes de
  unidade com mocks/fakes sem refatoração.
- **Mensageria assíncrona**: trocar a baixa de estoque (hoje síncrona via
  HTTP) por uma fila (RabbitMQ/Kafka) — ganha desacoplamento e resiliência,
  ao custo de consistência imediata.
- **Observabilidade**: OpenTelemetry para tracing distribuído entre os dois
  serviços e logging estruturado com correlação de request ID.
- **API Gateway**: hoje o frontend fala direto com os dois serviços via
  Nginx como proxy reverso; um gateway (ex.: YARP, nativo do ecossistema
  .NET) centralizaria autenticação, rate limiting e roteamento.
- **Autenticação/autorização**: o sistema não tem login hoje; JWT + políticas
  de autorização por perfil seriam o próximo passo.
- **Cache**: Redis para leitura de produtos (que muda com pouca frequência
  relativa), reduzindo carga no banco em cenários de alto volume.
- **IA** (diferencial opcional não implementado): pontos de integração
  possíveis incluem sugestão automática de descrição de produto a partir do
  código, ou um assistente que resume o histórico de notas fiscais de um
  período.
- **Saga orquestrado**: evoluir a compensação local atual (item 9) para um
  Saga com máquina de estados explícita, ou coreografia via eventos em fila.
