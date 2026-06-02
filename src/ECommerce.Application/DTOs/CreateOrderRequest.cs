namespace ECommerce.Application.DTOs;

public sealed record CreateOrderRequest(
    string BuyerName,
    List<CreateOrderProductRequest> Products
);

public sealed record CreateOrderProductRequest(
    string Name,
    decimal Price,
    int Quantity
);
