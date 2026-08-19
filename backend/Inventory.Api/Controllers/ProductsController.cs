using Inventory.Api.Application.DTOs;
using Inventory.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]

    public async Task<ActionResult<List<ProductResponse>>> List(CancellationToken ct) =>
        await productService.ListAsync(ct);

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken ct) =>
        await productService.GetByIdAsync(id, ct);

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await productService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPost("{id:guid}/decrease-balance")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]

    public async Task<ActionResult<ProductResponse>> DecreaseBalance(Guid id, [FromBody] DecreaseBalanceRequest request, CancellationToken ct) =>
        await productService.DecreaseBalanceAsync(id, request, ct);

    [HttpPost("{id:guid}/replenish-balance")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]

    public async Task<ActionResult<ProductResponse>> ReplenishBalance(Guid id, [FromBody] ReplenishBalanceRequest request, CancellationToken ct) =>
        await productService.ReplenishBalanceAsync(id, request, ct);
}
