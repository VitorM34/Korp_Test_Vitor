using Faturamento.Api.Application.DTOs;
using Faturamento.Api.Domain.Entities;
using Faturamento.Api.Domain.Exceptions;
using Faturamento.Api.Infrastructure;
using Faturamento.Api.Infrastructure.Http;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Application.Services;

public class NotaFiscalService(
    FaturamentoDbContext db,
    IEstoqueApiClient estoqueClient,
    ILogger<NotaFiscalService> logger)
{
    public async Task<List<NotaFiscalResponse>> ListarAsync() =>
        await db.NotasFiscais
            .Include(n => n.Itens)
            .OrderByDescending(n => n.Numeracao)
            .Select(n => MapToResponse(n))
            .ToListAsync();

    public async Task<NotaFiscalResponse> ObterPorIdAsync(Guid id)
    {
        var nota = await db.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id)
            ?? throw new NotaFiscalNaoEncontradaException(id);

        return MapToResponse(nota);
    }

    public async Task<NotaFiscalResponse> CriarAsync()
    {
        var proxNumeracao = await db.NotasFiscais.AnyAsync()
            ? await db.NotasFiscais.MaxAsync(n => n.Numeracao) + 1
            : 1;

        var nota = NotaFiscal.Criar(proxNumeracao);
        db.NotasFiscais.Add(nota);
        await db.SaveChangesAsync();
        logger.LogInformation("Nota fiscal criada: {Id}, numeração {Num}", nota.Id, nota.Numeracao);
        return MapToResponse(nota);
    }

    public async Task<NotaFiscalResponse> AdicionarItemAsync(Guid notaId, AdicionarItemRequest request)
    {
        var nota = await db.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == notaId)
            ?? throw new NotaFiscalNaoEncontradaException(notaId);

        nota.AdicionarItem(request.ProdutoId, request.DescricaoProduto, request.Quantidade);
        await db.SaveChangesAsync();
        return MapToResponse(nota);
    }

    public async Task<NotaFiscalResponse> ImprimirAsync(Guid id, CancellationToken ct = default)
    {
        var nota = await db.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new NotaFiscalNaoEncontradaException(id);

        if (nota.Status == Domain.Enums.StatusNota.Fechada)
            throw new NotaFiscalFechadaException(id);

        if (!nota.Itens.Any())
            throw new ArgumentException("A nota fiscal não possui itens.");

        var itensBaixados = new List<(Guid ProdutoId, int Quantidade)>();

        try
        {
            foreach (var item in nota.Itens)
            {
                var chaveIdempotencia = $"{nota.Id}-{item.ProdutoId}";
                await estoqueClient.BaixarSaldoAsync(item.ProdutoId, item.Quantidade, chaveIdempotencia, ct);
                itensBaixados.Add((item.ProdutoId, item.Quantidade));
                logger.LogInformation("Saldo baixado para produto {ProdutoId}, qtd {Qtd}", item.ProdutoId, item.Quantidade);
            }

            nota.Fechar();
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Nota fiscal {Id} impressa com sucesso", id);
        }
        catch (Exception ex) when (itensBaixados.Count > 0)
        {
            logger.LogWarning(ex, "Falha na impressão, iniciando compensação de {Count} item(s)", itensBaixados.Count);
            await CompensarSaldosAsync(itensBaixados);
            throw;
        }

        return MapToResponse(nota);
    }

    private async Task CompensarSaldosAsync(List<(Guid ProdutoId, int Quantidade)> itens)
    {
        foreach (var (produtoId, quantidade) in itens)
        {
            try
            {
                await estoqueClient.ReporSaldoAsync(produtoId, quantidade);
                logger.LogInformation("Compensação realizada para produto {ProdutoId}", produtoId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha crítica na compensação do produto {ProdutoId}", produtoId);
            }
        }
    }

    private static NotaFiscalResponse MapToResponse(NotaFiscal nota) =>
        new(nota.Id, nota.Numeracao, nota.Status.ToString(), nota.DataCriacao, nota.DataImpressao,
            nota.Itens.Select(i => new ItemNotaFiscalResponse(i.Id, i.ProdutoId, i.DescricaoProduto, i.Quantidade)).ToList());
}
