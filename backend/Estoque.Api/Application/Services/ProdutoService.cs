using Estoque.Api.Application.DTOs;
using Estoque.Api.Domain.Entities;
using Estoque.Api.Domain.Exceptions;
using Estoque.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Application.Services;

public class ProdutoService(EstoqueDbContext db, ILogger<ProdutoService> logger)
{
    public async Task<List<ProdutoResponse>> ListarAsync() =>
        await db.Produtos
            .OrderBy(p => p.Codigo)
            .Select(p => new ProdutoResponse(p.Id, p.Codigo, p.Descricao, p.Saldo))
            .ToListAsync();

    public async Task<ProdutoResponse> ObterPorIdAsync(Guid id)
    {
        var produto = await db.Produtos.FindAsync(id)
            ?? throw new ProdutoNaoEncontradoException(id);

        return new ProdutoResponse(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo);
    }

    public async Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request)
    {
        var produto = Produto.Criar(request.Codigo, request.Descricao, request.Saldo);
        db.Produtos.Add(produto);
        await db.SaveChangesAsync();
        logger.LogInformation("Produto criado: {Id} - {Codigo}", produto.Id, produto.Codigo);
        return new ProdutoResponse(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo);
    }

    public async Task<ProdutoResponse> BaixarSaldoAsync(Guid id, BaixarSaldoRequest request)
    {
        var produto = await db.Produtos.FindAsync(id)
            ?? throw new ProdutoNaoEncontradoException(id);

        produto.BaixarSaldo(request.Quantidade);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Conflito de concorrência ao baixar saldo do produto {Id}", id);
            throw;
        }

        logger.LogInformation("Saldo baixado: produto {Id}, quantidade {Qtd}, novo saldo {Saldo}",
            id, request.Quantidade, produto.Saldo);

        return new ProdutoResponse(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo);
    }

    public async Task<ProdutoResponse> ReporSaldoAsync(Guid id, ReporSaldoRequest request)
    {
        var produto = await db.Produtos.FindAsync(id)
            ?? throw new ProdutoNaoEncontradoException(id);

        produto.ReporSaldo(request.Quantidade);
        await db.SaveChangesAsync();
        logger.LogInformation("Saldo reposto: produto {Id}, quantidade {Qtd}", id, request.Quantidade);
        return new ProdutoResponse(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo);
    }
}
