using ECommerce.Application.Abstractions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Buyers;
using ECommerce.Domain.Orders;
using ECommerce.Domain.Products;
using MediatR;

namespace ECommerce.Application.Orders.Handlers;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IBuyerRepository _buyerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(
        IBuyerRepository buyerRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        _buyerRepository = buyerRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        ValidateCreate(request);

        var buyer = new Buyer(Guid.NewGuid(), request.BuyerName);
        _buyerRepository.Add(buyer);

        var items = new List<OrderItem>(request.Products.Count);
        foreach (var productRequest in request.Products)
        {
            var product = new Product(Guid.NewGuid(), productRequest.Name, productRequest.Price);
            _productRepository.Add(product);

            items.Add(new OrderItem(
                id: Guid.NewGuid(),
                productId: product.Id,
                quantity: productRequest.Quantity,
                unitPrice: productRequest.Price));
        }

        var order = Order.Create(buyer.Id, items);
        _orderRepository.Add(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        var persistedOrder = await _orderRepository.GetByIdAsync(order.Id, includeItems: true, cancellationToken);
        if (persistedOrder is null)
        {
            throw new NotFoundException("Order not found after creation.");
        }

        return OrderResponseMapper.Map(persistedOrder, buyer);
    }

    private static void ValidateCreate(CreateOrderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.BuyerName))
        {
            errors["buyerName"] = ["buyerName is required."];
        }

        if (request.Products is null || request.Products.Count == 0)
        {
            errors["products"] = ["Order must contain at least one product."];
            throw new ValidationException(errors);
        }

        for (var i = 0; i < request.Products.Count; i++)
        {
            var p = request.Products[i];
            if (string.IsNullOrWhiteSpace(p.Name))
            {
                errors[$"products[{i}].name"] = ["name is required."];
            }

            if (p.Price <= 0)
            {
                errors[$"products[{i}].price"] = ["price must be greater than zero."];
            }

            if (p.Quantity <= 0)
            {
                errors[$"products[{i}].quantity"] = ["quantity must be greater than zero."];
            }
        }

        if (errors.Count != 0)
        {
            throw new ValidationException(errors);
        }
    }
}
