using ECommerce.Application.Abstractions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Orders;
using MediatR;

namespace ECommerce.Application.Orders.Handlers;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderResponse>>
{
    private readonly IBuyerRepository _buyerRepository;
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IBuyerRepository buyerRepository, IOrderRepository orderRepository)
    {
        _buyerRepository = buyerRepository;
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderResponse>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        OrderStatus? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<OrderStatus>(query.Status, ignoreCase: true, out var parsed))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["status"] = [$"Invalid status. Allowed values: {string.Join(", ", Enum.GetNames<OrderStatus>())}"]
                });
            }

            status = parsed;
        }

        var orders = await _orderRepository.ListAsync(status, query.BuyerId, cancellationToken);
        if (orders.Count == 0)
        {
            return [];
        }

        var result = new List<OrderResponse>(orders.Count);
        foreach (var order in orders)
        {
            var buyer = await _buyerRepository.GetByIdAsync(order.BuyerId, cancellationToken);
            if (buyer is null)
            {
                throw new NotFoundException("Buyer not found for this order.");
            }

            result.Add(OrderResponseMapper.Map(order, buyer));
        }

        return result;
    }
}
