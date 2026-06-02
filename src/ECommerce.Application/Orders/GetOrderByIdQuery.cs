using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Orders;

public sealed record GetOrderByIdQuery(Guid Id) : IRequest<OrderResponse>;
