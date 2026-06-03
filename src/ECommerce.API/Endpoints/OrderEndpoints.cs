using ECommerce.Application.DTOs;
using ECommerce.Application.Orders;
using ECommerce.Domain.Orders;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ECommerce.API.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders").WithTags("Orders").RequireAuthorization();

        group.MapPost("", async (CreateOrderRequest request, ClaimsPrincipal user, ISender sender, ILoggerFactory loggerFactory, HttpContext httpContext, CancellationToken ct) =>
        {
            var logger = CreateLogger(loggerFactory);

            if (!TryGetBuyer(user, out var buyerId, out var buyerName))
            {
                logger.LogWarning("Orders create unauthorized. TraceId={TraceId}", httpContext.TraceIdentifier);
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Orders create started. BuyerId={BuyerId} Items={ItemsCount} TraceId={TraceId}",
                buyerId,
                request.Products?.Count ?? 0,
                httpContext.TraceIdentifier);

            var response = await sender.Send(new CreateOrderCommand(buyerId, buyerName, request), ct);

            logger.LogInformation(
                "Orders create completed. BuyerId={BuyerId} OrderId={OrderId} Status={Status} TraceId={TraceId}",
                buyerId,
                response.Id,
                response.Status,
                httpContext.TraceIdentifier);

            return Results.Created($"/api/v1/orders/{response.Id}", response);
        });

        group.MapGet("", async (OrderStatus? status, ClaimsPrincipal user, ISender sender, ILoggerFactory loggerFactory, HttpContext httpContext, CancellationToken ct) =>
        {
            var logger = CreateLogger(loggerFactory);

            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                logger.LogWarning("Orders list unauthorized. TraceId={TraceId}", httpContext.TraceIdentifier);
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Orders list started. BuyerId={BuyerId} Status={Status} TraceId={TraceId}",
                buyerId,
                status?.ToString(),
                httpContext.TraceIdentifier);

            var response = await sender.Send(new GetOrdersQuery(buyerId, status), ct);

            logger.LogInformation(
                "Orders list completed. BuyerId={BuyerId} Count={Count} TraceId={TraceId}",
                buyerId,
                response.Count,
                httpContext.TraceIdentifier);

            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, ISender sender, ILoggerFactory loggerFactory, HttpContext httpContext, CancellationToken ct) =>
        {
            var logger = CreateLogger(loggerFactory);

            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                logger.LogWarning("Orders get by id unauthorized. OrderId={OrderId} TraceId={TraceId}", id, httpContext.TraceIdentifier);
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Orders get by id started. BuyerId={BuyerId} OrderId={OrderId} TraceId={TraceId}",
                buyerId,
                id,
                httpContext.TraceIdentifier);

            var response = await sender.Send(new GetOrderByIdQuery(buyerId, id), ct);

            logger.LogInformation(
                "Orders get by id completed. BuyerId={BuyerId} OrderId={OrderId} Status={Status} TraceId={TraceId}",
                buyerId,
                id,
                response.Status,
                httpContext.TraceIdentifier);

            return Results.Ok(response);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateOrderRequest request, ClaimsPrincipal user, ISender sender, ILoggerFactory loggerFactory, HttpContext httpContext, CancellationToken ct) =>
        {
            var logger = CreateLogger(loggerFactory);

            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                logger.LogWarning("Orders update unauthorized. OrderId={OrderId} TraceId={TraceId}", id, httpContext.TraceIdentifier);
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Orders update started. BuyerId={BuyerId} OrderId={OrderId} Items={ItemsCount} TraceId={TraceId}",
                buyerId,
                id,
                request.Products?.Count ?? 0,
                httpContext.TraceIdentifier);

            var response = await sender.Send(new UpdateOrderCommand(buyerId, id, request), ct);

            logger.LogInformation(
                "Orders update completed. BuyerId={BuyerId} OrderId={OrderId} Status={Status} TraceId={TraceId}",
                buyerId,
                id,
                response.Status,
                httpContext.TraceIdentifier);

            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/process", async (Guid id, ClaimsPrincipal user, ISender sender, ILoggerFactory loggerFactory, HttpContext httpContext, CancellationToken ct) =>
        {
            var logger = CreateLogger(loggerFactory);

            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                logger.LogWarning("Orders process unauthorized. OrderId={OrderId} TraceId={TraceId}", id, httpContext.TraceIdentifier);
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Orders process started. BuyerId={BuyerId} OrderId={OrderId} TraceId={TraceId}",
                buyerId,
                id,
                httpContext.TraceIdentifier);

            var response = await sender.Send(new ProcessOrderCommand(buyerId, id), ct);

            logger.LogInformation(
                "Orders process completed. BuyerId={BuyerId} OrderId={OrderId} Status={Status} TraceId={TraceId}",
                buyerId,
                id,
                response.Status,
                httpContext.TraceIdentifier);

            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/ship", async (Guid id, ClaimsPrincipal user, ISender sender, ILoggerFactory loggerFactory, HttpContext httpContext, CancellationToken ct) =>
        {
            var logger = CreateLogger(loggerFactory);

            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                logger.LogWarning("Orders ship unauthorized. OrderId={OrderId} TraceId={TraceId}", id, httpContext.TraceIdentifier);
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Orders ship started. BuyerId={BuyerId} OrderId={OrderId} TraceId={TraceId}",
                buyerId,
                id,
                httpContext.TraceIdentifier);

            var response = await sender.Send(new ShipOrderCommand(buyerId, id), ct);

            logger.LogInformation(
                "Orders ship completed. BuyerId={BuyerId} OrderId={OrderId} Status={Status} TraceId={TraceId}",
                buyerId,
                id,
                response.Status,
                httpContext.TraceIdentifier);

            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, ClaimsPrincipal user, ISender sender, ILoggerFactory loggerFactory, HttpContext httpContext, CancellationToken ct) =>
        {
            var logger = CreateLogger(loggerFactory);

            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                logger.LogWarning("Orders cancel unauthorized. OrderId={OrderId} TraceId={TraceId}", id, httpContext.TraceIdentifier);
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Orders cancel started. BuyerId={BuyerId} OrderId={OrderId} TraceId={TraceId}",
                buyerId,
                id,
                httpContext.TraceIdentifier);

            var response = await sender.Send(new CancelOrderCommand(buyerId, id), ct);

            logger.LogInformation(
                "Orders cancel completed. BuyerId={BuyerId} OrderId={OrderId} Status={Status} TraceId={TraceId}",
                buyerId,
                id,
                response.Status,
                httpContext.TraceIdentifier);

            return Results.Ok(response);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, ISender sender, ILoggerFactory loggerFactory, HttpContext httpContext, CancellationToken ct) =>
        {
            var logger = CreateLogger(loggerFactory);

            if (!TryGetBuyer(user, out var buyerId, out _))
            {
                logger.LogWarning("Orders delete unauthorized. OrderId={OrderId} TraceId={TraceId}", id, httpContext.TraceIdentifier);
                return Results.Unauthorized();
            }

            logger.LogInformation(
                "Orders delete started. BuyerId={BuyerId} OrderId={OrderId} TraceId={TraceId}",
                buyerId,
                id,
                httpContext.TraceIdentifier);

            await sender.Send(new DeleteOrderCommand(buyerId, id), ct);

            logger.LogInformation(
                "Orders delete completed. BuyerId={BuyerId} OrderId={OrderId} TraceId={TraceId}",
                buyerId,
                id,
                httpContext.TraceIdentifier);

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

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger("ECommerce.API.Endpoints.OrderEndpoints");
    }
}
