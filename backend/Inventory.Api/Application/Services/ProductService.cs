using Inventory.Api.Application.DTOs;
using Inventory.Api.Domain.Entities;
using Inventory.Api.Domain.Exceptions;
using Inventory.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Inventory.Api.Application.Services;

public class ProductService(InventoryDbContext db, ILogger<ProductService> logger)
{
    public async Task<List<ProductResponse>> ListAsync(CancellationToken ct) =>
        await db.Products
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new ProductResponse(p.Id, p.Code, p.Description, p.Balance))
            .ToListAsync(ct);

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct)
            ?? throw new ProductNotFoundException(id);

        return new ProductResponse(product.Id, product.Code, product.Description, product.Balance);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var product = Product.Create(request.Code, request.Description, request.Balance);
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Produto criado: {Id} - {Code}", product.Id, product.Code);
        return new ProductResponse(product.Id, product.Code, product.Description, product.Balance);
    }

    public async Task<ProductResponse> DecreaseBalanceAsync(Guid id, DecreaseBalanceRequest request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.IdempotencyKey, nameof(request.IdempotencyKey));

        // Retries do cliente (Polly, ver AddStandardResilienceHandler no Billing.Api) podem reenviar
        // a mesma requisição após um commit bem-sucedido cuja resposta se perdeu. Sem checar a chave
        // de idempotência, isso baixaria o saldo duas vezes para o mesmo pedido lógico.
        var alreadyProcessed = await db.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Key == request.IdempotencyKey, ct);

        if (alreadyProcessed != null)
        {
            logger.LogInformation("Requisição repetida para a chave de idempotência {Key}; retornando resultado já processado", request.IdempotencyKey);
            return await GetCurrentStateAsync(alreadyProcessed.ProductId, ct);
        }

        var product = await db.Products.FindAsync([id], ct)
            ?? throw new ProductNotFoundException(id);

        product.DecreaseBalance(request.Quantity);
        db.IdempotencyRecords.Add(IdempotencyRecord.Create(request.IdempotencyKey, id));

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueKeyViolation(ex))
        {
            // Corrida concorrente: outra requisição com a mesma chave venceu e já processou.
            // product já está rastreado com a mutação em memória (não persistida) e o SaveChanges
            // falhou/reverteu, então FindAsync devolveria esse estado local incorreto — busca sem tracking.
            logger.LogInformation("Corrida na chave de idempotência {Key}; retornando resultado já processado", request.IdempotencyKey);
            return await GetCurrentStateAsync(id, ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Conflito de concorrência ao baixar saldo do produto {Id}", id);
            throw;
        }

        logger.LogInformation("Saldo baixado: produto {Id}, quantidade {Qtd}, novo saldo {Balance}",
            id, request.Quantity, product.Balance);

        return new ProductResponse(product.Id, product.Code, product.Description, product.Balance);
    }

    private static bool IsUniqueKeyViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private async Task<ProductResponse> GetCurrentStateAsync(Guid id, CancellationToken ct)
    {
        var product = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new ProductNotFoundException(id);

        return new ProductResponse(product.Id, product.Code, product.Description, product.Balance);
    }

    public async Task<ProductResponse> ReplenishBalanceAsync(Guid id, ReplenishBalanceRequest request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct)
            ?? throw new ProductNotFoundException(id);

        product.ReplenishBalance(request.Quantity);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Saldo reposto: produto {Id}, quantidade {Qtd}", id, request.Quantity);
        return new ProductResponse(product.Id, product.Code, product.Description, product.Balance);
    }
}
