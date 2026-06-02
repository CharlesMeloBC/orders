using Orders.Domain.Products;

namespace Orders.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(Product product);
}
