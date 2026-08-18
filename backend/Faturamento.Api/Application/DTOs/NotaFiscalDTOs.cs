namespace Faturamento.Api.Application.DTOs;

public record CriarNotaFiscalResponse(Guid Id, int Numeracao, string Status, DateTime DataCriacao);

public record AdicionarItemRequest(Guid ProdutoId, string DescricaoProduto, int Quantidade);

public record ItemNotaFiscalResponse(Guid Id, Guid ProdutoId, string DescricaoProduto, int Quantidade);

public record NotaFiscalResponse(
    Guid Id,
    int Numeracao,
    string Status,
    DateTime DataCriacao,
    DateTime? DataImpressao,
    List<ItemNotaFiscalResponse> Itens);
