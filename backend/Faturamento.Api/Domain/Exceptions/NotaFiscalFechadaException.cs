namespace Faturamento.Api.Domain.Exceptions;

public class NotaFiscalFechadaException(Guid id)
    : Exception($"A nota fiscal '{id}' já está fechada e não pode ser modificada.")
{
    public Guid NotaFiscalId { get; } = id;
}
