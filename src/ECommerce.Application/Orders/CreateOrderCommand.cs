using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Orders;

public sealed record CreateOrderCommand(Guid BuyerId, string BuyerName, CreateOrderRequest Request) : IRequest<CreateOrderResponse>;
