using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Orders;

public sealed record CancelOrderCommand(Guid Id) : IRequest<OrderResponse>;
