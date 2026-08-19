using System.Net;
using Billing.Api.Domain.Exceptions;

namespace Billing.Api.Infrastructure.Http;

public record DecreaseBalanceRequest(int Quantity, string IdempotencyKey);
public record ReplenishBalanceRequest(int Quantity);
public record ProductResponse(Guid Id, string Code, string Description, int Balance);

public interface IInventoryApiClient
{
    Task<ProductResponse> GetProductAsync(Guid productId, CancellationToken ct = default);
    Task DecreaseBalanceAsync(Guid productId, int quantity, string idempotencyKey, CancellationToken ct = default);
    Task ReplenishBalanceAsync(Guid productId, int quantity, CancellationToken ct = default);
}

public class InventoryApiClient(HttpClient httpClient, ILogger<InventoryApiClient> logger) : IInventoryApiClient
{
    public async Task<ProductResponse> GetProductAsync(Guid productId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"api/products/{productId}", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductResponse>(ct)
            ?? throw new InvalidOperationException("Resposta inválida do serviço de estoque.");
    }

    public async Task DecreaseBalanceAsync(Guid productId, int quantity, string idempotencyKey, CancellationToken ct = default)
    {
        var body = new DecreaseBalanceRequest(quantity, idempotencyKey);
        var response = await httpClient.PostAsJsonAsync($"api/products/{productId}/decrease-balance", body, ct);

        if (response.IsSuccessStatusCode)
            return;

        var erro = await response.Content.ReadAsStringAsync(ct);
        logger.LogWarning("Falha ao baixar saldo do produto {Id}: {Status} - {Erro}", productId, response.StatusCode, erro);

        // Erros de negócio do estoque (produto inexistente, saldo insuficiente) não são falha de
        // conectividade: precisam virar exceções específicas para não serem confundidos com o
        // serviço de estoque estar fora do ar (que continua caindo no EnsureSuccessStatusCode abaixo).
        switch (response.StatusCode)
        {
            case HttpStatusCode.NotFound:
                throw new InventoryProductNotFoundException(productId);
            case HttpStatusCode.UnprocessableEntity:
                throw new InsufficientStockException(productId, $"Saldo insuficiente no estoque para o produto '{productId}'.");
            default:
                response.EnsureSuccessStatusCode();
                break;
        }
    }

    public async Task ReplenishBalanceAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        var body = new ReplenishBalanceRequest(quantity);
        var response = await httpClient.PostAsJsonAsync($"api/products/{productId}/replenish-balance", body, ct);

        if (!response.IsSuccessStatusCode)
            logger.LogError("Falha ao repor saldo do produto {Id} durante compensação", productId);
    }
}
