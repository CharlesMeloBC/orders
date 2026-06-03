using ECommerce.Application.DTOs;
using ECommerce.Domain.Buyers;
using ECommerce.Domain.Orders;

namespace ECommerce.Application.Orders;

internal static class OrderResponseMapper
{
    private static readonly TimeSpan AppOffset = TimeSpan.FromHours(-3);

    public static OrderResponse Map(Order order, Buyer buyer)
    {
        var items = new List<OrderItemResponse>(order.Items.Count);
        decimal total = 0;

        foreach (var item in order.Items)
        {
            var productName = item.Product?.Name ?? string.Empty;
            var lineTotal = item.UnitPrice * item.Quantity;
            total += lineTotal;

            items.Add(new OrderItemResponse(
                ProductName: productName,
                UnitPrice: item.UnitPrice,
                Quantity: item.Quantity,
                LineTotal: lineTotal));
        }

        return new OrderResponse(
            Id: order.Id,
            BuyerId: order.BuyerId,
            BuyerName: buyer.Name,
            Status: order.Status.ToString(),
            CreatedAt: ApplyOffset(order.CreatedAtUtc),
            UpdatedAt: ApplyOffset(order.UpdatedAtUtc),
            DeletedAt: order.DeletedAtUtc is null ? null : ApplyOffset(order.DeletedAtUtc.Value),
            Items: items,
            Total: total);
    }

    public static CreateOrderResponse MapCreate(Order order, Buyer buyer)
    {
        var items = new List<OrderItemResponse>(order.Items.Count);
        decimal total = 0;

        foreach (var item in order.Items)
        {
            var productName = item.Product?.Name ?? string.Empty;
            var lineTotal = item.UnitPrice * item.Quantity;
            total += lineTotal;

            items.Add(new OrderItemResponse(
                ProductName: productName,
                UnitPrice: item.UnitPrice,
                Quantity: item.Quantity,
                LineTotal: lineTotal));
        }

        return new CreateOrderResponse(
            Id: order.Id,
            BuyerId: order.BuyerId,
            BuyerName: buyer.Name,
            Status: order.Status.ToString(),
            CreatedAt: ApplyOffset(order.CreatedAtUtc),
            Items: items,
            Total: total);
    }

    private static DateTimeOffset ApplyOffset(DateTime utc)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc))
            .ToOffset(AppOffset);
    }
}
