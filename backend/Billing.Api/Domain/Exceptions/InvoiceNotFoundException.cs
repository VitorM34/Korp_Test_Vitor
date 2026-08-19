namespace Billing.Api.Domain.Exceptions;

public class InvoiceNotFoundException : Exception
{
    public Guid InvoiceId { get; }

    public InvoiceNotFoundException()
    {
    }

    public InvoiceNotFoundException(string message) : base(message)
    {
    }

    public InvoiceNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InvoiceNotFoundException(Guid id)
        : base($"Nota fiscal com Id '{id}' não encontrada.")
    {
        InvoiceId = id;
    }
}
