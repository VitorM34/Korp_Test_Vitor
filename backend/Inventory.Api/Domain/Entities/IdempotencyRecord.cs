namespace Inventory.Api.Domain.Entities;

public class IdempotencyRecord
{
    public string Key { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public DateTime ProcessedDate { get; private set; } = DateTime.UtcNow;

    private IdempotencyRecord() { }

    public static IdempotencyRecord Create(string key, Guid productId) =>
        new() { Key = key, ProductId = productId };
}
