namespace Billing.Api.Domain.Exceptions;

public class InvoiceNotClosedException : Exception
{
    public Guid InvoiceId { get; }

    public InvoiceNotClosedException()
    {
    }

    public InvoiceNotClosedException(string message) : base(message)
    {
    }

    public InvoiceNotClosedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InvoiceNotClosedException(Guid invoiceId)
        : base($"A nota fiscal '{invoiceId}' ainda não foi emitida e não pode ser reimpressa.")
    {
        InvoiceId = invoiceId;
    }
}
