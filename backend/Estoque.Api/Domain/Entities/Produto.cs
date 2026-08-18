namespace Estoque.Api.Domain.Entities;

public class Produto
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Codigo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public int Saldo { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private Produto() { }

    public static Produto Criar(string codigo, string descricao, int saldo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Código é obrigatório.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição é obrigatória.");
        if (saldo < 0)
            throw new ArgumentException("Saldo não pode ser negativo.");

        return new Produto { Codigo = codigo.Trim(), Descricao = descricao.Trim(), Saldo = saldo };
    }

    public void BaixarSaldo(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.");
        if (Saldo < quantidade)
            throw new Exceptions.SaldoInsuficienteException(Id, Saldo, quantidade);

        Saldo -= quantidade;
    }

    public void ReporSaldo(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero.");

        Saldo += quantidade;
    }
}
