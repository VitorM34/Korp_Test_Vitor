namespace Estoque.Api.Application.DTOs;

public record CriarProdutoRequest(string Codigo, string Descricao, int Saldo);

public record BaixarSaldoRequest(int Quantidade, string ChaveIdempotencia);

public record ReporSaldoRequest(int Quantidade);

public record ProdutoResponse(Guid Id, string Codigo, string Descricao, int Saldo);
