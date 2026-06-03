using ECommerce.Application.Abstractions;
using ECommerce.Application.Exceptions;
using MediatR;

namespace ECommerce.Application.Orders.Handlers;

public sealed class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(command.Id, command.BuyerId, includeItems: false, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        order.Delete();
        try
        {
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateConcurrencyException")
        {
            throw new NotFoundException("Order not found or has been modified.");
        }

        return Unit.Value;
    }
}
