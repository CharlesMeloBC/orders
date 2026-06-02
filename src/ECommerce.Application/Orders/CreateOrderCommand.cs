using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Orders;

public sealed record CreateOrderCommand(CreateOrderRequest Request) : IRequest<OrderResponse>;
