using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Orders;

public sealed record GetOrdersQuery(string? Status, Guid? BuyerId) : IRequest<List<OrderResponse>>;
