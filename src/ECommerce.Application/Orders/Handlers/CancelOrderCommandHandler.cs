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
        var order = await _orderRepository.GetByIdAsync(command.Id, command.BuyerId, includeItems: true, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        order.Cancel();
        try
        {
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateConcurrencyException")
        {
            throw new NotFoundException("Order not found or has been modified.");
        }

        var buyer = await _buyerRepository.GetByIdAsync(command.BuyerId, cancellationToken);
        if (buyer is null)
        {
            throw new NotFoundException("Buyer not found for this order.");
        }

        return OrderResponseMapper.Map(order, buyer);
    }
}
