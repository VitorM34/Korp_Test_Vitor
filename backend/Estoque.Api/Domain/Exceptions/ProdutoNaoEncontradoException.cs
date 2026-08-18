namespace Estoque.Api.Domain.Exceptions;

public class ProdutoNaoEncontradoException(Guid id)
    : Exception($"Produto com Id '{id}' não encontrado.")
{
    public Guid ProdutoId { get; } = id;
}
