namespace Billing.Api.Domain.Entities;

// Registro de auditoria de cada emissão/reimpressão de uma nota fiscal.
// O primeiro registro é criado automaticamente quando a nota é emitida (InvoiceService.PrintAsync);
// registros seguintes são criados a cada reimpressão feita pelo usuário na tela de detalhes.
public class InvoicePrintLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid InvoiceId { get; private set; }
    public DateTime PrintedDate { get; private set; } = DateTime.UtcNow;

    private InvoicePrintLog() { }

    internal InvoicePrintLog(Guid invoiceId)
    {
        InvoiceId = invoiceId;
    }
}
