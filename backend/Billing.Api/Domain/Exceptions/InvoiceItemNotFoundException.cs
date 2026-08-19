namespace Billing.Api.Domain.Exceptions;

public class InvoiceItemNotFoundException : Exception
{
    public Guid InvoiceId { get; }
    public Guid ItemId { get; }

    public InvoiceItemNotFoundException()
    {
    }

    public InvoiceItemNotFoundException(string message) : base(message)
    {
    }

    public InvoiceItemNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InvoiceItemNotFoundException(Guid invoiceId, Guid itemId)
        : base($"Item '{itemId}' não encontrado na nota fiscal '{invoiceId}'.")
    {
        InvoiceId = invoiceId;
        ItemId = itemId;
    }
}
