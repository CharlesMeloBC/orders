using Orders.Domain.Orders;

namespace Orders.Application.Orders.Dtos;

public sealed record OrderResponse(
    Guid Id,
    Guid BuyerId,
    string BuyerName,
    OrderStatus Status,
    DateTime CreatedAtUtc,
    List<OrderItemResponse> Items,
    decimal Total
);

public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);
