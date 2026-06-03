using MediatR;

namespace ECommerce.Application.Orders;

public sealed record DeleteOrderCommand(Guid BuyerId, Guid Id) : IRequest<Unit>;
