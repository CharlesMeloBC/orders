using Orders.Application.Orders.Dtos;
using Orders.Domain.Orders;

namespace Orders.Application.Orders;

public interface IOrdersService
{
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<OrderResponse>> ListAsync(OrderStatus? status, Guid? buyerId, CancellationToken cancellationToken);
    Task<OrderResponse?> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderResponse?> CancelAsync(Guid id, CancellationToken cancellationToken);
}
