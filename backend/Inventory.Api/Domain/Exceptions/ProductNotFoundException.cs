namespace Inventory.Api.Domain.Exceptions;

public class ProductNotFoundException : Exception
{
    public Guid ProductId { get; }

    public ProductNotFoundException()
    {
    }

    public ProductNotFoundException(string message) : base(message)
    {
    }

    public ProductNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ProductNotFoundException(Guid id)
        : base($"Produto com Id '{id}' não encontrado.")
    {
        ProductId = id;
    }
}
