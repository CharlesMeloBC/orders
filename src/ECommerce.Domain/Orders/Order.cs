using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();

    private Order()
    {
    }

    private Order(Guid id, Guid buyerId)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Order Id não pode ser vazio.");
        }

        if (buyerId == Guid.Empty)
        {
            throw new DomainException("Order BuyerId não pode ser vazio.");
        }

        Id = id;
        BuyerId = buyerId;
        Status = OrderStatus.Initiated;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BuyerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;

    public static Order Create(Guid buyerId, IEnumerable<OrderItem> items)
    {
        var order = new Order(Guid.NewGuid(), buyerId);
        order.ReplaceItems(items);
        return order;
    }

    public void UpdateItems(IEnumerable<OrderItem> items)
    {
        if (Status != OrderStatus.Initiated)
        {
            throw new DomainException("Only orders with status Initiated can be updated.");
        }

        ReplaceItems(items);
    }

    public void Process()
    {
        if (Status != OrderStatus.Initiated)
        {
            throw new DomainException("Only orders with status Initiated can be processed.");
        }

        Status = OrderStatus.Processed;
    }

    public void Cancel()
    {
        if (Status is not (OrderStatus.Initiated or OrderStatus.Processed))
        {
            throw new DomainException("Only orders with status Initiated or Processed can be cancelled.");
        }

        Status = OrderStatus.Cancelled;
    }

    public void Send()
    {
        if (Status != OrderStatus.Processed)
        {
            throw new DomainException("Only orders with status Processed can be shipped.");
        }

        Status = OrderStatus.Shipped;
    }

    private void ReplaceItems(IEnumerable<OrderItem> items)
    {
        if (items is null)
        {
            throw new DomainException("Order items are required.");
        }

        var newItems = items.ToList();
        if (newItems.Count == 0)
        {
            throw new DomainException("Order must contain at least one product.");
        }

        _items.Clear();
        foreach (var item in newItems)
        {
            item.SetOrderId(Id);
            _items.Add(item);
        }
    }
}
