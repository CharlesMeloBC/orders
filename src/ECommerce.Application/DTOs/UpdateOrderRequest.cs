namespace ECommerce.Application.DTOs;

public sealed record UpdateOrderRequest(
    List<UpdateOrderProductRequest> Products
);

public sealed record UpdateOrderProductRequest(
    string Name,
    decimal Price,
    int Quantity
);
