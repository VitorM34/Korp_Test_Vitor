namespace Inventory.Api.Domain.Exceptions;

public class InsufficientBalanceException : Exception
{
    public Guid ProductId { get; }
    public int CurrentBalance { get; }
    public int RequestedQuantity { get; }

    public InsufficientBalanceException()
    {
    }

    public InsufficientBalanceException(string message) : base(message)
    {
    }

    public InsufficientBalanceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InsufficientBalanceException(Guid productId, int currentBalance, int requestedQuantity)
        : base($"Produto {productId} possui saldo {currentBalance}, mas foram solicitadas {requestedQuantity} unidades.")
    {
        ProductId = productId;
        CurrentBalance = currentBalance;
        RequestedQuantity = requestedQuantity;
    }
}
