namespace Billing.Api.Domain.Entities;

public class InvoiceItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid InvoiceId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductDescription { get; private set; } = string.Empty;
    public int Quantity { get; private set; }

    private InvoiceItem() { }

    internal InvoiceItem(Guid invoiceId, Guid productId, string productDescription, int quantity)
    {
        InvoiceId = invoiceId;
        ProductId = productId;
        ProductDescription = productDescription;
        Quantity = quantity;
    }

    internal void UpdateQuantity(int newQuantity)
    {
        Quantity = newQuantity;
    }
}
