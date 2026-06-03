using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Orders;

namespace ECommerce.Tests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Create_WhenItemsAreEmpty_ShouldThrowDomainException()
    {
        var buyerId = Guid.NewGuid();

        var exception = Assert.Throws<DomainException>(() => Order.Create(buyerId, []));

        Assert.Equal("Order must contain at least one product.", exception.Message);
    }

    [Fact]
    public void Create_WhenValid_ShouldStartAsInitiated()
    {
        var order = CreateOrder();

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(OrderStatus.Initiated, order.Status);
        Assert.Single(order.Items);
        Assert.Null(order.DeletedAtUtc);
    }

    [Fact]
    public void UpdateItems_WhenOrderIsInitiated_ShouldReplaceItems()
    {
        var order = CreateOrder();
        var replacement = new[]
        {
            CreateItem(quantity: 2, unitPrice: 50)
        };

        order.UpdateItems(replacement);

        var item = Assert.Single(order.Items);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(50, item.UnitPrice);
    }

    [Fact]
    public void UpdateItems_WhenOrderIsProcessed_ShouldThrowDomainException()
    {
        var order = CreateOrder();
        order.Process();

        var exception = Assert.Throws<DomainException>(() => order.UpdateItems([CreateItem()]));

        Assert.Equal("Only orders with status Initiated can be updated.", exception.Message);
    }

    [Fact]
    public void Cancel_WhenOrderIsInitiated_ShouldSetCancelledAndKeepOrderQueryable()
    {
        var order = CreateOrder();

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Null(order.DeletedAtUtc);
    }

    [Fact]
    public void Cancel_WhenOrderIsShipped_ShouldThrowDomainException()
    {
        var order = CreateOrder();
        order.Process();
        order.Send();

        var exception = Assert.Throws<DomainException>(order.Cancel);

        Assert.Equal("Only orders with status Initiated or Processed can be cancelled.", exception.Message);
    }

    [Fact]
    public void Send_WhenOrderIsProcessed_ShouldSetShipped()
    {
        var order = CreateOrder();
        order.Process();

        order.Send();

        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public void Send_WhenOrderIsInitiated_ShouldThrowDomainException()
    {
        var order = CreateOrder();

        var exception = Assert.Throws<DomainException>(order.Send);

        Assert.Equal("Only orders with status Processed can be shipped.", exception.Message);
    }

    [Fact]
    public void Delete_WhenOrderExists_ShouldSetDeletedAt()
    {
        var order = CreateOrder();

        order.Delete();

        Assert.NotNull(order.DeletedAtUtc);
    }

    private static Order CreateOrder()
    {
        return Order.Create(Guid.NewGuid(), [CreateItem()]);
    }

    private static OrderItem CreateItem(int quantity = 1, decimal unitPrice = 10)
    {
        return new OrderItem(
            id: Guid.NewGuid(),
            productId: Guid.NewGuid(),
            quantity: quantity,
            unitPrice: unitPrice);
    }
}
