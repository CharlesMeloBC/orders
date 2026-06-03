using Microsoft.EntityFrameworkCore;
using ECommerce.Application.Abstractions;
using ECommerce.Domain.Products;

namespace ECommerce.Infrastructure.Persistence.Repositories;

internal sealed class EfProductRepository : IProductRepository
{
    private readonly OrdersDbContext _dbContext;

    public EfProductRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Add(Product product)
    {
        _dbContext.Products.Add(product);
    }
}
