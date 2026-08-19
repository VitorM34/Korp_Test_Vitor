namespace Billing.Api.Domain.Exceptions;

public class InsufficientStockException : Exception
{
    public Guid ProductId { get; }

    public InsufficientStockException()
    {
    }

    public InsufficientStockException(string message) : base(message)
    {
    }

    public InsufficientStockException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InsufficientStockException(Guid productId, string detail)
        : base(detail)
    {
        ProductId = productId;
    }
}
