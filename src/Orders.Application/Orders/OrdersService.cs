using Orders.Application.Abstractions;
using Orders.Application.Orders.Dtos;
using Orders.Domain.Buyers;
using Orders.Domain.Exceptions;
using Orders.Domain.Orders;
using Orders.Domain.Products;

namespace Orders.Application.Orders;

public sealed class OrdersService : IOrdersService
{
    private readonly IBuyerRepository _buyerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    public OrdersService(
        IBuyerRepository buyerRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        _buyerRepository = buyerRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var buyer = await _buyerRepository.GetByIdAsync(request.BuyerId, cancellationToken);
        if (buyer is null)
        {
            buyer = new Buyer(request.BuyerId, request.BuyerName);
            _buyerRepository.Add(buyer);
        }

        var orderItems = new List<OrderItem>(request.Items.Count);
        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                product = new Product(item.ProductId, item.ProductName, item.UnitPrice);
                _productRepository.Add(product);
            }

            orderItems.Add(new OrderItem(
                id: Guid.NewGuid(),
                productId: item.ProductId,
                quantity: item.Quantity,
                unitPrice: item.UnitPrice));
        }

        var order = Order.Create(request.BuyerId, orderItems);
        _orderRepository.Add(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return Map(order, buyer);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, includeItems: true, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var buyer = await _buyerRepository.GetByIdAsync(order.BuyerId, cancellationToken);
        if (buyer is null)
        {
            throw new DomainException("Buyer não encontrado para o pedido.");
        }

        return Map(order, buyer);
    }

    public async Task<List<OrderResponse>> ListAsync(OrderStatus? status, Guid? buyerId, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.ListAsync(status, buyerId, cancellationToken);
        if (orders.Count == 0)
        {
            return [];
        }

        var buyerIds = orders.Select(o => o.BuyerId).Distinct().ToList();
        var buyers = new Dictionary<Guid, Buyer>(buyerIds.Count);
        foreach (var id in buyerIds)
        {
            var buyer = await _buyerRepository.GetByIdAsync(id, cancellationToken);
            if (buyer is not null)
            {
                buyers[id] = buyer;
            }
        }

        var result = new List<OrderResponse>(orders.Count);
        foreach (var order in orders)
        {
            if (!buyers.TryGetValue(order.BuyerId, out var buyer))
            {
                throw new DomainException("Buyer não encontrado para o pedido.");
            }

            result.Add(Map(order, buyer));
        }

        return result;
    }

    public async Task<OrderResponse?> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, includeItems: true, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var orderItems = new List<OrderItem>(request.Items.Count);
        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                product = new Product(item.ProductId, item.ProductName, item.UnitPrice);
                _productRepository.Add(product);
            }

            orderItems.Add(new OrderItem(
                id: Guid.NewGuid(),
                productId: item.ProductId,
                quantity: item.Quantity,
                unitPrice: item.UnitPrice));
        }

        order.UpdateItems(orderItems);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        var buyer = await _buyerRepository.GetByIdAsync(order.BuyerId, cancellationToken);
        if (buyer is null)
        {
            throw new DomainException("Buyer não encontrado para o pedido.");
        }

        return Map(order, buyer);
    }

    public async Task<OrderResponse?> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, includeItems: true, cancellationToken);
        if (order is null)
        {
            return null;
        }

        order.Cancel();
        await _orderRepository.SaveChangesAsync(cancellationToken);

        var buyer = await _buyerRepository.GetByIdAsync(order.BuyerId, cancellationToken);
        if (buyer is null)
        {
            throw new DomainException("Buyer não encontrado para o pedido.");
        }

        return Map(order, buyer);
    }

    private static OrderResponse Map(Order order, Buyer buyer)
    {
        var items = new List<OrderItemResponse>(order.Items.Count);
        decimal total = 0;

        foreach (var item in order.Items)
        {
            var productName = item.Product?.Name ?? string.Empty;
            var lineTotal = item.UnitPrice * item.Quantity;
            total += lineTotal;

            items.Add(new OrderItemResponse(
                ProductId: item.ProductId,
                ProductName: productName,
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,
                LineTotal: lineTotal));
        }

        return new OrderResponse(
            Id: order.Id,
            BuyerId: order.BuyerId,
            BuyerName: buyer.Name,
            Status: order.Status,
            CreatedAtUtc: order.CreatedAtUtc,
            Items: items,
            Total: total);
    }
}
