using ECommerce.Application.DTOs;
using ECommerce.Application.Orders;
using ECommerce.Domain.Orders;
using MediatR;
using System.Security.Claims;

namespace ECommerce.API.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders").WithTags("Orders").RequireAuthorization();

        group.MapPost("", async (CreateOrderRequest request, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            if (!TryGetBuyer(user, out var buyerId, out var buyerName))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new CreateOrderCommand(buyerId, buyerName, request), ct);
            return Results.Created($"/api/v1/orders/{response.Id}", response);
        });

        group.MapGet("", async (OrderStatus? status, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new GetOrdersQuery(buyerId, status), ct);
            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new GetOrderByIdQuery(buyerId, id), ct);
            return Results.Ok(response);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateOrderRequest request, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new UpdateOrderCommand(buyerId, id, request), ct);
            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/process", async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new ProcessOrderCommand(buyerId, id), ct);
            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/ship", async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new ShipOrderCommand(buyerId, id), ct);
            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new CancelOrderCommand(buyerId, id), ct);
            return Results.Ok(response);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                return Results.Unauthorized();
            }

            await sender.Send(new DeleteOrderCommand(buyerId, id), ct);
            return Results.NoContent();
        });

        return app;
    }

    private static bool TryGetBuyer(ClaimsPrincipal user, out Guid buyerId, out string buyerName)
    {
        buyerId = Guid.Empty;
        buyerName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        var rawBuyerId =
            user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("buyerId")
            ?? user.FindFirstValue("sub");

        return Guid.TryParse(rawBuyerId, out buyerId);
    }
}
