using ECommerce.Domain.Orders;

namespace ECommerce.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, Guid buyerId, bool includeItems, CancellationToken cancellationToken);
    Task<List<Order>> ListAsync(Guid buyerId, OrderStatus? status, CancellationToken cancellationToken);
    void Add(Order order);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
