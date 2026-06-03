using ECommerce.Application.Abstractions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using MediatR;

namespace ECommerce.Application.Orders.Handlers;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderResponse>
{
    private readonly IBuyerRepository _buyerRepository;
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IBuyerRepository buyerRepository, IOrderRepository orderRepository)
    {
        _buyerRepository = buyerRepository;
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(command.Id, includeItems: true, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        order.Cancel();
        await _orderRepository.SaveChangesAsync(cancellationToken);

        var persistedOrder = await _orderRepository.GetByIdAsync(order.Id, includeItems: true, cancellationToken);
        if (persistedOrder is null)
        {
            throw new NotFoundException("Order not found after cancellation.");
        }

        var buyer = await _buyerRepository.GetByIdAsync(persistedOrder.BuyerId, cancellationToken);
        if (buyer is null)
        {
            throw new NotFoundException("Buyer not found for this order.");
        }

        return OrderResponseMapper.Map(persistedOrder, buyer);
    }
}
