using ECommerce.Domain.Orders;

namespace ECommerce.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, Guid buyerId, bool includeItems, CancellationToken cancellationToken);
    Task<List<Order>> ListAsync(Guid buyerId, OrderStatus? status, CancellationToken cancellationToken);
    void Add(Order order);
    void AddItems(IEnumerable<OrderItem> items);
    Task RemoveItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
