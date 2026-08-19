namespace Inventory.Api.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Balance { get; private set; }
    public uint XMin { get; private set; }

    private Product() { }

    public static Product Create(string code, string description, int balance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        ArgumentOutOfRangeException.ThrowIfNegative(balance, nameof(balance));

        return new Product { Code = code.Trim(), Description = description.Trim(), Balance = balance };
    }

    public void DecreaseBalance(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity, nameof(quantity));
        if (Balance < quantity)
            throw new Exceptions.InsufficientBalanceException(Id, Balance, quantity);

        Balance -= quantity;
    }

    public void ReplenishBalance(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity, nameof(quantity));

        Balance += quantity;
    }
}
