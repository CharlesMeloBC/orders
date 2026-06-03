using ECommerce.Application.DTOs;
using ECommerce.Domain.Buyers;
using ECommerce.Domain.Orders;

namespace ECommerce.Application.Orders;

internal static class OrderResponseMapper
{
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
            CreatedAtUtc: order.CreatedAtUtc,
            Items: items,
            Total: total);
    }
}
