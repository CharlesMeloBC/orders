using Microsoft.EntityFrameworkCore;
using ECommerce.Application.Abstractions;
using ECommerce.Domain.Buyers;

namespace ECommerce.Infrastructure.Persistence.Repositories;

internal sealed class EfBuyerRepository : IBuyerRepository
{
    private readonly OrdersDbContext _dbContext;

    public EfBuyerRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Buyers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Add(Buyer buyer)
    {
        _dbContext.Buyers.Add(buyer);
    }
}
