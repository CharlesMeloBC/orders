namespace Orders.Application.Orders.Dtos;

public sealed record UpdateOrderRequest(
    List<UpdateOrderItemRequest> Items
);

public sealed record UpdateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
);
