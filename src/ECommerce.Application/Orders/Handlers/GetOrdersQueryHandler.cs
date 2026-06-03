using ECommerce.Application.Abstractions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
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
        var orders = await _orderRepository.ListAsync(query.BuyerId, query.Status, cancellationToken);
        if (orders.Count == 0)
        {
            return [];
        }

        var buyer = await _buyerRepository.GetByIdAsync(query.BuyerId, cancellationToken);
        if (buyer is null)
        {
            throw new NotFoundException("Buyer not found for these orders.");
        }

        var result = new List<OrderResponse>(orders.Count);
        foreach (var order in orders)
        {
            result.Add(OrderResponseMapper.Map(order, buyer));
        }

        return result;
    }
}
