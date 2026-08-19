using Inventory.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Middleware;

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
            ProductNotFoundException e => (StatusCodes.Status404NotFound, "Produto não encontrado", e.Message),
            InsufficientBalanceException e => (StatusCodes.Status422UnprocessableEntity, "Saldo insuficiente", e.Message),
            ArgumentException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflito de concorrência",
                "O saldo do produto foi alterado por outra operação simultânea. Tente novamente."),
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
