namespace Billing.Api.Domain.Exceptions;

public class InvoiceClosedException : Exception
{
    public Guid InvoiceId { get; }

    public InvoiceClosedException()
    {
    }

    public InvoiceClosedException(string message) : base(message)
    {
    }

    public InvoiceClosedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InvoiceClosedException(Guid id)
        : base($"A nota fiscal '{id}' já está fechada e não pode ser modificada.")
    {
        InvoiceId = id;
    }
}
