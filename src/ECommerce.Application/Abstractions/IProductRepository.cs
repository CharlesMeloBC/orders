using ECommerce.Domain.Products;

namespace ECommerce.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(Product product);
}
