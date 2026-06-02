using ECommerce.Domain.Orders;

namespace ECommerce.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, bool includeItems, CancellationToken cancellationToken);
    Task<List<Order>> ListAsync(OrderStatus? status, Guid? buyerId, CancellationToken cancellationToken);
    void Add(Order order);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
