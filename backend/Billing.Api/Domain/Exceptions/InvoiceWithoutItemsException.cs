namespace Billing.Api.Domain.Exceptions;

public class InvoiceWithoutItemsException : Exception
{
    public Guid InvoiceId { get; }

    public InvoiceWithoutItemsException()
    {
    }

    public InvoiceWithoutItemsException(string message) : base(message)
    {
    }

    public InvoiceWithoutItemsException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InvoiceWithoutItemsException(Guid id)
        : base($"A nota fiscal '{id}' não possui itens e não pode ser fechada.")
    {
        InvoiceId = id;
    }
}
