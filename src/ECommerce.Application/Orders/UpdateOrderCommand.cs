using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Orders;

public sealed record UpdateOrderCommand(Guid BuyerId, Guid Id, UpdateOrderRequest Request) : IRequest<OrderResponse>;
