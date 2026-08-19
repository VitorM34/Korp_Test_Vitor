namespace Billing.Api.Domain.Exceptions;

public class InventoryProductNotFoundException : Exception
{
    public Guid ProductId { get; }

    public InventoryProductNotFoundException()
    {
    }

    public InventoryProductNotFoundException(string message) : base(message)
    {
    }

    public InventoryProductNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InventoryProductNotFoundException(Guid productId)
        : base($"Produto '{productId}' não encontrado no estoque.")
    {
        ProductId = productId;
    }
}
