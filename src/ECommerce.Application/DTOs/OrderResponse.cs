namespace ECommerce.Application.DTOs;

public sealed record OrderResponse(
    Guid Id,
    Guid BuyerId,
    string BuyerName,
    string Status,
    DateTime CreatedAtUtc,
    List<OrderItemResponse> Items,
    decimal Total
);

public sealed record OrderItemResponse(
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);
