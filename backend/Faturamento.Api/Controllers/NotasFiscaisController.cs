using Faturamento.Api.Application.DTOs;
using Faturamento.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Faturamento.Api.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
public class NotasFiscaisController(NotaFiscalService notaFiscalService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await notaFiscalService.ListarAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id) =>
        Ok(await notaFiscalService.ObterPorIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Criar()
    {
        var nota = await notaFiscalService.CriarAsync();
        return CreatedAtAction(nameof(ObterPorId), new { id = nota.Id }, nota);
    }

    [HttpPost("{id:guid}/itens")]
    public async Task<IActionResult> AdicionarItem(Guid id, [FromBody] AdicionarItemRequest request) =>
        Ok(await notaFiscalService.AdicionarItemAsync(id, request));

    [HttpPost("{id:guid}/imprimir")]
    public async Task<IActionResult> Imprimir(Guid id, CancellationToken ct) =>
        Ok(await notaFiscalService.ImprimirAsync(id, ct));
}
