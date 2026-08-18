using Faturamento.Api.Domain.Enums;

namespace Faturamento.Api.Domain.Entities;

public class NotaFiscal
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int Numeracao { get; private set; }
    public StatusNota Status { get; private set; } = StatusNota.Aberta;
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
    public DateTime? DataImpressao { get; private set; }
    public List<ItemNotaFiscal> Itens { get; private set; } = [];

    private NotaFiscal() { }

    public static NotaFiscal Criar(int numeracao) => new() { Numeracao = numeracao };

    public void AdicionarItem(Guid produtoId, string descricaoProduto, int quantidade)
    {
        if (Status == StatusNota.Fechada)
            throw new Exceptions.NotaFiscalFechadaException(Id);
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(descricaoProduto))
            throw new ArgumentException("Descrição do produto é obrigatória.");

        Itens.Add(new ItemNotaFiscal(Id, produtoId, descricaoProduto.Trim(), quantidade));
    }

    public void Fechar()
    {
        if (Status == StatusNota.Fechada)
            throw new Exceptions.NotaFiscalFechadaException(Id);

        Status = StatusNota.Fechada;
        DataImpressao = DateTime.UtcNow;
    }
}
