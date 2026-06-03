using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Orders;

public sealed record GetOrderByIdQuery(Guid BuyerId, Guid Id) : IRequest<OrderResponse>;
