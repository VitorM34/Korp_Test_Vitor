using Billing.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Billing.Api.Middleware;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Requisição cancelada pelo cliente: {Path}", context.Request.Path);
            return true;
        }

        if (context.Response.HasStarted)
        {
            logger.LogError(exception, "Exceção não tratada após o início da resposta: {Message}", exception.Message);
            return false;
        }

        logger.LogError(exception, "Exceção não tratada: {Message}", exception.Message);

        var (status, title, detail) = exception switch
        {
            InvoiceNotFoundException e => (StatusCodes.Status404NotFound, "Nota fiscal não encontrada", e.Message),
            InvoiceItemNotFoundException e => (StatusCodes.Status404NotFound, "Item não encontrado", e.Message),
            InvoiceClosedException e => (StatusCodes.Status409Conflict, "Nota fiscal fechada", e.Message),
            InvoiceNotClosedException e => (StatusCodes.Status409Conflict, "Nota fiscal não emitida", e.Message),
            InvoiceWithoutItemsException e => (StatusCodes.Status409Conflict, "Nota fiscal sem itens", e.Message),
            InventoryProductNotFoundException e => (StatusCodes.Status404NotFound, "Produto não encontrado no estoque", e.Message),
            InsufficientStockException e => (StatusCodes.Status422UnprocessableEntity, "Saldo insuficiente", e.Message),
            ArgumentException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflito de concorrência",
                "A nota fiscal foi alterada por outra operação simultânea. Tente novamente."),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflito de numeração",
                "Não foi possível criar a nota fiscal devido a uma colisão de numeração. Tente novamente."),
            // O AddStandardResilienceHandler (Polly) embrulha falhas de conectividade em
            // BrokenCircuitException (circuito aberto) ou TimeoutRejectedException (timeout),
            // então HttpRequestException sozinho nunca captura essas falhas na prática.
            HttpRequestException or BrokenCircuitException or TimeoutRejectedException =>
                (StatusCodes.Status503ServiceUnavailable, "Serviço de estoque indisponível",
                "O serviço de estoque está temporariamente indisponível. Tente novamente em instantes."),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro inesperado.")
        };

        context.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            }
        });
    }
}
