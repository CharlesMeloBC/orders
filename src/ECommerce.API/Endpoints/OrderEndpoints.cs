using ECommerce.Application.DTOs;
using ECommerce.Application.Orders;
using MediatR;

namespace ECommerce.API.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders").WithTags("Orders");

        group.MapPost("", async (CreateOrderRequest request, ISender sender, CancellationToken ct) =>
        {
            var response = await sender.Send(new CreateOrderCommand(request), ct);
            return Results.Created($"/api/v1/orders/{response.Id}", response);
        });

        group.MapGet("", async (string? status, Guid? buyerId, ISender sender, CancellationToken ct) =>
        {
            var response = await sender.Send(new GetOrdersQuery(status, buyerId), ct);
            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var response = await sender.Send(new GetOrderByIdQuery(id), ct);
            return Results.Ok(response);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateOrderRequest request, ISender sender, CancellationToken ct) =>
        {
            var response = await sender.Send(new UpdateOrderCommand(id, request), ct);
            return Results.Ok(response);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var response = await sender.Send(new CancelOrderCommand(id), ct);
            return Results.Ok(response);
        });

        return app;
    }
}
