using ECommerce.Application.Abstractions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using MediatR;

namespace ECommerce.Application.Orders.Handlers;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse>
{
    private readonly IBuyerRepository _buyerRepository;
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IBuyerRepository buyerRepository, IOrderRepository orderRepository)
    {
        _buyerRepository = buyerRepository;
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(query.Id, includeItems: true, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        var buyer = await _buyerRepository.GetByIdAsync(order.BuyerId, cancellationToken);
        if (buyer is null)
        {
            throw new NotFoundException("Buyer not found for this order.");
        }

        return OrderResponseMapper.Map(order, buyer);
    }
}
