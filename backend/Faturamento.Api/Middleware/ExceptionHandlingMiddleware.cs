using Faturamento.Api.Domain.Exceptions;

namespace Faturamento.Api.Middleware;

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
        var (status, title, detail) = exception switch
        {
            NotaFiscalNaoEncontradaException e => (404, "Nota fiscal não encontrada", e.Message),
            NotaFiscalFechadaException e => (409, "Nota fiscal fechada", e.Message),
            ArgumentException e => (400, "Requisição inválida", e.Message),
            HttpRequestException => (503, "Serviço de estoque indisponível",
                "O serviço de estoque está temporariamente indisponível. Tente novamente em instantes."),
            _ => (500, "Erro interno", "Ocorreu um erro inesperado.")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.com/{status}",
            title,
            status,
            detail,
            instance = context.Request.Path.Value
        });
    }
}
