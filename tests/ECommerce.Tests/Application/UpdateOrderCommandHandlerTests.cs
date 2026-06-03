using ECommerce.Application.Abstractions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Orders;
using ECommerce.Application.Orders.Handlers;
using ECommerce.Domain.Buyers;
using ECommerce.Domain.Exceptions;
using ECommerce.Domain.Orders;
using ECommerce.Domain.Products;

namespace ECommerce.Tests.Application;

public sealed class UpdateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderIsInitiated_ShouldReplaceItemsAndReturnUpdatedOrder()
    {
        var buyer = new Buyer(Guid.NewGuid(), "Charles");
        var order = Order.Create(buyer.Id, [CreateItem(quantity: 1, unitPrice: 10)]);
        var buyerRepository = new FakeBuyerRepository(buyer);
        var productRepository = new FakeProductRepository();
        var orderRepository = new FakeOrderRepository(order);
        var handler = new UpdateOrderCommandHandler(buyerRepository, productRepository, orderRepository);
        var request = new UpdateOrderRequest(
        [
            new UpdateOrderProductRequest("Produto B", 50, 2)
        ]);

        var response = await handler.Handle(new UpdateOrderCommand(buyer.Id, order.Id, request), CancellationToken.None);

        Assert.Equal(order.Id, response.Id);
        Assert.Equal("Initiated", response.Status);
        Assert.Equal(100, response.Total);
        Assert.Single(response.Items);
        Assert.True(orderRepository.RemoveItemsByOrderIdWasCalled);
        Assert.Single(orderRepository.AddedItems);
        Assert.Single(productRepository.Products);
        Assert.True(orderRepository.SaveChangesWasCalled);
    }

    [Fact]
    public async Task Handle_WhenOrderIsProcessed_ShouldThrowDomainException()
    {
        var buyer = new Buyer(Guid.NewGuid(), "Charles");
        var order = Order.Create(buyer.Id, [CreateItem()]);
        order.Process();
        var handler = new UpdateOrderCommandHandler(
            new FakeBuyerRepository(buyer),
            new FakeProductRepository(),
            new FakeOrderRepository(order));
        var request = new UpdateOrderRequest(
        [
            new UpdateOrderProductRequest("Produto B", 50, 2)
        ]);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new UpdateOrderCommand(buyer.Id, order.Id, request), CancellationToken.None));

        Assert.Equal("Only orders with status Initiated can be updated.", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenProductsAreEmpty_ShouldThrowValidationException()
    {
        var buyer = new Buyer(Guid.NewGuid(), "Charles");
        var order = Order.Create(buyer.Id, [CreateItem()]);
        var handler = new UpdateOrderCommandHandler(
            new FakeBuyerRepository(buyer),
            new FakeProductRepository(),
            new FakeOrderRepository(order));
        var request = new UpdateOrderRequest([]);

        var exception = await Assert.ThrowsAsync<ECommerce.Application.Exceptions.ValidationException>(() =>
            handler.Handle(new UpdateOrderCommand(buyer.Id, order.Id, request), CancellationToken.None));

        Assert.True(exception.Errors.ContainsKey("products"));
    }

    private static OrderItem CreateItem(int quantity = 1, decimal unitPrice = 10)
    {
        return new OrderItem(Guid.NewGuid(), Guid.NewGuid(), quantity, unitPrice);
    }

    private sealed class FakeBuyerRepository : IBuyerRepository
    {
        private readonly Buyer _buyer;

        public FakeBuyerRepository(Buyer buyer)
        {
            _buyer = buyer;
        }

        public Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(id == _buyer.Id ? _buyer : null);
        }

        public void Add(Buyer buyer)
        {
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public List<Product> Products { get; } = [];

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Products.FirstOrDefault(x => x.Id == id));
        }

        public void Add(Product product)
        {
            Products.Add(product);
        }
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        private readonly Order _order;

        public FakeOrderRepository(Order order)
        {
            _order = order;
        }

        public bool RemoveItemsByOrderIdWasCalled { get; private set; }
        public bool SaveChangesWasCalled { get; private set; }
        public List<OrderItem> AddedItems { get; } = [];

        public Task<Order?> GetByIdAsync(Guid id, Guid buyerId, bool includeItems, CancellationToken cancellationToken)
        {
            var result = id == _order.Id && buyerId == _order.BuyerId ? _order : null;
            return Task.FromResult(result);
        }

        public Task<List<Order>> ListAsync(Guid buyerId, OrderStatus? status, CancellationToken cancellationToken)
        {
            var result = _order.BuyerId == buyerId && (status is null || _order.Status == status)
                ? new List<Order> { _order }
                : [];

            return Task.FromResult(result);
        }

        public void Add(Order order)
        {
        }

        public void AddItems(IEnumerable<OrderItem> items)
        {
            AddedItems.AddRange(items);
        }

        public Task RemoveItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        {
            RemoveItemsByOrderIdWasCalled = orderId == _order.Id;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesWasCalled = true;
            return Task.CompletedTask;
        }
    }
}
