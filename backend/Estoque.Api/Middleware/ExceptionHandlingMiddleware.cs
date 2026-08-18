using Estoque.Api.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Estoque.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exceção não tratada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            ProdutoNaoEncontradoException e => (HttpStatusCode.NotFound, "Produto não encontrado", e.Message),
            SaldoInsuficienteException e => (HttpStatusCode.UnprocessableEntity, "Saldo insuficiente", e.Message),
            ArgumentException e => (HttpStatusCode.BadRequest, "Requisição inválida", e.Message),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "Conflito de concorrência",
                "O saldo do produto foi alterado por outra operação simultânea. Tente novamente."),
            _ => (HttpStatusCode.InternalServerError, "Erro interno", "Ocorreu um erro inesperado.")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail,
            instance = context.Request.Path.Value
        });
    }
}
