namespace Estoque.Api.Domain.Exceptions;

public class SaldoInsuficienteException(Guid produtoId, int saldoAtual, int quantidadeSolicitada)
    : Exception($"Produto {produtoId} possui saldo {saldoAtual}, mas foram solicitadas {quantidadeSolicitada} unidades.")
{
    public Guid ProdutoId { get; } = produtoId;
    public int SaldoAtual { get; } = saldoAtual;
    public int QuantidadeSolicitada { get; } = quantidadeSolicitada;
}
