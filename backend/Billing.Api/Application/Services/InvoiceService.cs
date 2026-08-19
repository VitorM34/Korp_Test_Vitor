using Billing.Api.Application.DTOs;
using Billing.Api.Domain.Entities;
using Billing.Api.Domain.Exceptions;
using Billing.Api.Infrastructure;
using Billing.Api.Infrastructure.Http;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api.Application.Services;

public class InvoiceService(
    BillingDbContext db,
    IInventoryApiClient inventoryClient,
    ILogger<InvoiceService> logger)
{
    public async Task<List<InvoiceResponse>> ListAsync(CancellationToken ct = default) =>
        await db.Invoices
            .AsNoTracking()
            .Include(n => n.Items)
            .OrderByDescending(n => n.Number)
            .Select(n => MapToResponse(n))
            .ToListAsync(ct);

    public async Task<InvoiceResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await db.Invoices
            .AsNoTracking()
            .Include(n => n.Items)
            .FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new InvoiceNotFoundException(id);

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> CreateAsync(CancellationToken ct = default)
    {
        var nextNumber = await db.Invoices.AnyAsync(ct)
            ? await db.Invoices.MaxAsync(n => n.Number, ct) + 1
            : 1;

        var invoice = Invoice.Create(nextNumber);
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Nota fiscal criada: {Id}, numeração {Num}", invoice.Id, invoice.Number);
        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> AddItemAsync(Guid invoiceId, AddItemRequest request, CancellationToken ct = default)
    {
        var invoice = await db.Invoices
            .Include(n => n.Items)
            .FirstOrDefaultAsync(n => n.Id == invoiceId, ct)
            ?? throw new InvoiceNotFoundException(invoiceId);

        invoice.AddItem(request.ProductId, request.ProductDescription, request.Quantity);

        await db.SaveChangesAsync(ct);
        return MapToResponse(invoice);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await db.Invoices
            .FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new InvoiceNotFoundException(id);

        if (invoice.Status == Domain.Enums.InvoiceStatus.Closed)
            throw new InvoiceClosedException(id);

        db.Invoices.Remove(invoice);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Nota fiscal {Id} excluída.", id);
    }

    public async Task<InvoiceResponse> RemoveItemAsync(Guid invoiceId, Guid itemId, CancellationToken ct = default)
    {
        var invoice = await db.Invoices
            .Include(n => n.Items)
            .FirstOrDefaultAsync(n => n.Id == invoiceId, ct)
            ?? throw new InvoiceNotFoundException(invoiceId);

        invoice.RemoveItem(itemId);
        await db.SaveChangesAsync(ct);
        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> UpdateItemAsync(Guid invoiceId, Guid itemId, UpdateItemRequest request, CancellationToken ct = default)
    {
        var invoice = await db.Invoices
            .Include(n => n.Items)
            .FirstOrDefaultAsync(n => n.Id == invoiceId, ct)
            ?? throw new InvoiceNotFoundException(invoiceId);

        invoice.UpdateItemQuantity(itemId, request.Quantity);
        await db.SaveChangesAsync(ct);
        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> PrintAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await db.Invoices
            .Include(n => n.Items)
            .FirstOrDefaultAsync(n => n.Id == id, ct)
            ?? throw new InvoiceNotFoundException(id);

        if (invoice.Status == Domain.Enums.InvoiceStatus.Closed)
            throw new InvoiceClosedException(id);

        var decreasedItems = new List<(Guid ProductId, int Quantity)>();

        try
        {
            foreach (var item in invoice.Items)
            {
                var idempotencyKey = $"{invoice.Id}-{item.ProductId}";
                await inventoryClient.DecreaseBalanceAsync(item.ProductId, item.Quantity, idempotencyKey, ct);
                decreasedItems.Add((item.ProductId, item.Quantity));
                logger.LogInformation("Saldo baixado para produto {ProductId}, qtd {Qtd}", item.ProductId, item.Quantity);
            }

            invoice.Close();
            db.InvoicePrintLogs.Add(new InvoicePrintLog(invoice.Id));
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Nota fiscal {Id} impressa com sucesso", id);
        }
        catch (Exception ex) when (decreasedItems.Count > 0)
        {
            logger.LogWarning(ex, "Falha na impressão, iniciando compensação de {Count} item(s)", decreasedItems.Count);
            // Compensação roda com CancellationToken.None: precisa terminar mesmo se o cliente cancelar a requisição,
            // senão o saldo fica baixado sem a nota ter sido efetivamente impressa.
            await CompensateBalancesAsync(decreasedItems);
            throw;
        }

        return MapToResponse(invoice);
    }

    public async Task<List<PrintLogResponse>> ListPrintLogAsync(CancellationToken ct = default) =>
        await db.InvoicePrintLogs
            .AsNoTracking()
            .OrderByDescending(l => l.PrintedDate)
            .Join(db.Invoices.AsNoTracking(), l => l.InvoiceId, n => n.Id, (l, n) => new PrintLogResponse(l.Id, n.Id, n.Number, l.PrintedDate))
            .ToListAsync(ct);

    public async Task<PrintLogResponse> LogReprintAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await db.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == invoiceId, ct)
            ?? throw new InvoiceNotFoundException(invoiceId);

        if (invoice.Status != Domain.Enums.InvoiceStatus.Closed)
            throw new InvoiceNotClosedException(invoiceId);

        var log = new InvoicePrintLog(invoiceId);
        db.InvoicePrintLogs.Add(log);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Reimpressão registrada para nota fiscal {Id}", invoiceId);

        return new PrintLogResponse(log.Id, invoice.Id, invoice.Number, log.PrintedDate);
    }

    private async Task CompensateBalancesAsync(List<(Guid ProductId, int Quantity)> items)
    {
        foreach (var (productId, quantity) in items)
        {
            try
            {
                await inventoryClient.ReplenishBalanceAsync(productId, quantity);
                logger.LogInformation("Compensação realizada para produto {ProductId}", productId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha crítica na compensação do produto {ProductId}", productId);
            }
        }
    }

    private static InvoiceResponse MapToResponse(Invoice invoice) =>
        new(invoice.Id, invoice.Number, invoice.Status.ToString(), invoice.CreatedDate, invoice.PrintedDate,
            invoice.Items.Select(i => new InvoiceItemResponse(i.Id, i.ProductId, i.ProductDescription, i.Quantity)).ToList());
}
