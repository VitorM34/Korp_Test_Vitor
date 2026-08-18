namespace Faturamento.Api.Domain.Exceptions;

public class NotaFiscalNaoEncontradaException(Guid id)
    : Exception($"Nota fiscal com Id '{id}' não encontrada.")
{
    public Guid NotaFiscalId { get; } = id;
}
