using Billing.Api.Domain.Enums;
using Billing.Api.Domain.Exceptions;

namespace Billing.Api.Domain.Entities;

public class Invoice
{
    private readonly List<InvoiceItem> _items = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public int Number { get; private set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Open;
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
    public DateTime? PrintedDate { get; private set; }
    public uint XMin { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    private Invoice() { }

    public static Invoice Create(int number) => new() { Number = number };

    public void AddItem(Guid productId, string productDescription, int quantity)
    {
        if (Status == InvoiceStatus.Closed)
            throw new InvoiceClosedException(Id);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity, nameof(quantity));
        ArgumentException.ThrowIfNullOrWhiteSpace(productDescription, nameof(productDescription));

        _items.Add(new InvoiceItem(Id, productId, productDescription.Trim(), quantity));
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status == InvoiceStatus.Closed)
            throw new InvoiceClosedException(Id);

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvoiceItemNotFoundException(Id, itemId);

        _items.Remove(item);
    }

    public void UpdateItemQuantity(Guid itemId, int newQuantity)
    {
        if (Status == InvoiceStatus.Closed)
            throw new InvoiceClosedException(Id);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newQuantity, nameof(newQuantity));

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvoiceItemNotFoundException(Id, itemId);

        item.UpdateQuantity(newQuantity);
    }

    public void Close()
    {
        if (Status == InvoiceStatus.Closed)
            throw new InvoiceClosedException(Id);

        if (_items.Count == 0)
            throw new InvoiceWithoutItemsException(Id);

        Status = InvoiceStatus.Closed;
        PrintedDate = DateTime.UtcNow;
    }
}
