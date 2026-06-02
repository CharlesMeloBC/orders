using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Products;

namespace ECommerce.Domain.Orders;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    public OrderItem(Guid id, Guid productId, int quantity, decimal unitPrice)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("OrderItem Id não pode ser vazio.");
        }

        if (productId == Guid.Empty)
        {
            throw new DomainException("OrderItem ProductId não pode ser vazio.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("OrderItem Quantity deve ser maior que zero.");
        }

        if (unitPrice <= 0)
        {
            throw new DomainException("OrderItem UnitPrice deve ser maior que zero.");
        }

        Id = id;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    internal void SetOrderId(Guid orderId)
    {
        OrderId = orderId;
    }
}
