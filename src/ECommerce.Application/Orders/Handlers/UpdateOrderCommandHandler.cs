using ECommerce.Application.Abstractions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Orders;
using ECommerce.Domain.Products;
using MediatR;

namespace ECommerce.Application.Orders.Handlers;

public sealed class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderResponse>
{
    private readonly IBuyerRepository _buyerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderCommandHandler(
        IBuyerRepository buyerRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        _buyerRepository = buyerRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        ValidateUpdate(command.Request);

        var order = await _orderRepository.GetByIdAsync(command.Id, command.BuyerId, includeItems: true, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        var items = new List<OrderItem>(command.Request.Products.Count);
        foreach (var productRequest in command.Request.Products)
        {
            var product = new Product(Guid.NewGuid(), productRequest.Name, productRequest.Price);
            _productRepository.Add(product);

            items.Add(new OrderItem(
                id: Guid.NewGuid(),
                productId: product.Id,
                quantity: productRequest.Quantity,
                unitPrice: productRequest.Price));
        }

        order.UpdateItems(items);
        try
        {
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateConcurrencyException")
        {
            throw new NotFoundException("Order not found or has been modified.");
        }

        var persistedOrder = await _orderRepository.GetByIdAsync(order.Id, command.BuyerId, includeItems: true, cancellationToken);
        if (persistedOrder is null)
        {
            throw new NotFoundException("Order not found after update.");
        }

        var buyer = await _buyerRepository.GetByIdAsync(command.BuyerId, cancellationToken);
        if (buyer is null)
        {
            throw new NotFoundException("Buyer not found for this order.");
        }

        return OrderResponseMapper.Map(persistedOrder, buyer);
    }

    private static void ValidateUpdate(UpdateOrderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

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
