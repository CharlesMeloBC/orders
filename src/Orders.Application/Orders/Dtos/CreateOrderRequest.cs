namespace Orders.Application.Orders.Dtos;

public sealed record CreateOrderRequest(
    Guid BuyerId,
    string BuyerName,
    List<CreateOrderItemRequest> Items
);

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);
