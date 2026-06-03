using ECommerce.Application.DTOs;
using ECommerce.Domain.Orders;
using MediatR;

namespace ECommerce.Application.Orders;

public sealed record GetOrdersQuery(Guid BuyerId, OrderStatus? Status) : IRequest<List<OrderResponse>>;
