using Estoque.Api.Application.DTOs;
using Estoque.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController(ProdutoService produtoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() =>
        Ok(await produtoService.ListarAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id) =>
        Ok(await produtoService.ObterPorIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarProdutoRequest request)
    {
        var produto = await produtoService.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
    }

    [HttpPost("{id:guid}/baixar-saldo")]
    public async Task<IActionResult> BaixarSaldo(Guid id, [FromBody] BaixarSaldoRequest request) =>
        Ok(await produtoService.BaixarSaldoAsync(id, request));

    [HttpPost("{id:guid}/repor-saldo")]
    public async Task<IActionResult> ReporSaldo(Guid id, [FromBody] ReporSaldoRequest request) =>
        Ok(await produtoService.ReporSaldoAsync(id, request));
}
