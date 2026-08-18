namespace Faturamento.Api.Domain.Entities;

public class ItemNotaFiscal
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid NotaFiscalId { get; private set; }
    public Guid ProdutoId { get; private set; }
    public string DescricaoProduto { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }

    private ItemNotaFiscal() { }

    internal ItemNotaFiscal(Guid notaFiscalId, Guid produtoId, string descricaoProduto, int quantidade)
    {
        NotaFiscalId = notaFiscalId;
        ProdutoId = produtoId;
        DescricaoProduto = descricaoProduto;
        Quantidade = quantidade;
    }
}
