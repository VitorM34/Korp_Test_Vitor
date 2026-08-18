namespace Faturamento.Api.Infrastructure.Http;

public record BaixarSaldoRequest(int Quantidade, string ChaveIdempotencia);
public record ReporSaldoRequest(int Quantidade);
public record ProdutoResponse(Guid Id, string Codigo, string Descricao, int Saldo);

public interface IEstoqueApiClient
{
    Task<ProdutoResponse> ObterProdutoAsync(Guid produtoId, CancellationToken ct = default);
    Task BaixarSaldoAsync(Guid produtoId, int quantidade, string chaveIdempotencia, CancellationToken ct = default);
    Task ReporSaldoAsync(Guid produtoId, int quantidade, CancellationToken ct = default);
}

public class EstoqueApiClient(HttpClient httpClient, ILogger<EstoqueApiClient> logger) : IEstoqueApiClient
{
    public async Task<ProdutoResponse> ObterProdutoAsync(Guid produtoId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"api/produtos/{produtoId}", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProdutoResponse>(ct)
            ?? throw new InvalidOperationException("Resposta inválida do serviço de estoque.");
    }

    public async Task BaixarSaldoAsync(Guid produtoId, int quantidade, string chaveIdempotencia, CancellationToken ct = default)
    {
        var body = new BaixarSaldoRequest(quantidade, chaveIdempotencia);
        var response = await httpClient.PostAsJsonAsync($"api/produtos/{produtoId}/baixar-saldo", body, ct);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Falha ao baixar saldo do produto {Id}: {Status} - {Erro}", produtoId, response.StatusCode, erro);
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task ReporSaldoAsync(Guid produtoId, int quantidade, CancellationToken ct = default)
    {
        var body = new ReporSaldoRequest(quantidade);
        var response = await httpClient.PostAsJsonAsync($"api/produtos/{produtoId}/repor-saldo", body, ct);

        if (!response.IsSuccessStatusCode)
            logger.LogError("Falha ao repor saldo do produto {Id} durante compensação", produtoId);
    }
}
