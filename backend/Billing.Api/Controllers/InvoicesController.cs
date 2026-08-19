using Billing.Api.Application.DTOs;
using Billing.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController(InvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<InvoiceResponse>), StatusCodes.Status200OK)]

    public async Task<ActionResult<List<InvoiceResponse>>> List(CancellationToken ct) =>
        await invoiceService.ListAsync(ct);

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<ActionResult<InvoiceResponse>> GetById(Guid id, CancellationToken ct) =>
        await invoiceService.GetByIdAsync(id, ct);

    [HttpPost]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]

    public async Task<ActionResult<InvoiceResponse>> Create(CancellationToken ct)
    {
        var invoice = await invoiceService.CreateAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]

    public async Task<ActionResult<InvoiceResponse>> AddItem(Guid id, [FromBody] AddItemRequest request, CancellationToken ct) =>
        await invoiceService.AddItemAsync(id, request, ct);

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]

    public async Task<ActionResult<InvoiceResponse>> RemoveItem(Guid id, Guid itemId, CancellationToken ct) =>
        await invoiceService.RemoveItemAsync(id, itemId, ct);

    [HttpPatch("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]

    public async Task<ActionResult<InvoiceResponse>> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateItemRequest request, CancellationToken ct) =>
        await invoiceService.UpdateItemAsync(id, itemId, request, ct);

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]

    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await invoiceService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/print")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]

    public async Task<ActionResult<InvoiceResponse>> Print(Guid id, CancellationToken ct) =>
        await invoiceService.PrintAsync(id, ct);

    [HttpGet("print-log")]
    [ProducesResponseType(typeof(List<PrintLogResponse>), StatusCodes.Status200OK)]

    public async Task<ActionResult<List<PrintLogResponse>>> ListPrintLog(CancellationToken ct) =>
        await invoiceService.ListPrintLogAsync(ct);

    [HttpPost("{id:guid}/reprint-log")]
    [ProducesResponseType(typeof(PrintLogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]

    public async Task<ActionResult<PrintLogResponse>> LogReprint(Guid id, CancellationToken ct)
    {
        var log = await invoiceService.LogReprintAsync(id, ct);
        return StatusCode(StatusCodes.Status201Created, log);
    }
}
