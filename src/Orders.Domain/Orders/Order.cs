using Orders.Domain.Exceptions;

namespace Orders.Domain.Orders;

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
        Status = OrderStatus.Iniciado;
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
        if (Status != OrderStatus.Iniciado)
        {
            throw new DomainException("Apenas pedidos não processados (Iniciado) podem ser alterados.");
        }

        ReplaceItems(items);
    }

    public void Process()
    {
        if (Status != OrderStatus.Iniciado)
        {
            throw new DomainException("Apenas pedidos iniciados podem ser processados.");
        }

        Status = OrderStatus.Processado;
    }

    public void Cancel()
    {
        if (Status is not (OrderStatus.Iniciado or OrderStatus.Processado))
        {
            throw new DomainException("Apenas pedidos iniciados ou processados podem ser cancelados.");
        }

        Status = OrderStatus.Cancelado;
    }

    public void Send()
    {
        if (Status != OrderStatus.Processado)
        {
            throw new DomainException("Apenas pedidos processados podem ser enviados.");
        }

        Status = OrderStatus.Enviado;
    }

    private void ReplaceItems(IEnumerable<OrderItem> items)
    {
        if (items is null)
        {
            throw new DomainException("Itens do pedido são obrigatórios.");
        }

        var newItems = items.ToList();
        if (newItems.Count == 0)
        {
            throw new DomainException("Pedido deve conter pelo menos um produto.");
        }

        _items.Clear();
        foreach (var item in newItems)
        {
            item.SetOrderId(Id);
            _items.Add(item);
        }
    }
}
