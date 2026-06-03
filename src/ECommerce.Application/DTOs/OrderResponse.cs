namespace ECommerce.Application.DTOs;

public sealed record OrderResponse(
    Guid Id,
    Guid BuyerId,
    string BuyerName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt,
    List<OrderItemResponse> Items,
    decimal Total
);

public sealed record CreateOrderResponse(
    Guid Id,
    Guid BuyerId,
    string BuyerName,
    string Status,
    DateTimeOffset CreatedAt,
    List<OrderItemResponse> Items,
    decimal Total
);

public sealed record OrderItemResponse(
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);
