using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions;
using Orders.Domain.Orders;

namespace Orders.Infrastructure.Persistence.Repositories;

internal sealed class EfOrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _dbContext;

    public EfOrderRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id, bool includeItems, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = _dbContext.Orders;

        if (includeItems)
        {
            query = query
                .Include(x => x.Items)
                .ThenInclude(x => x.Product);
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Order>> ListAsync(OrderStatus? status, Guid? buyerId, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = _dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product);

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
        }

        if (buyerId is not null)
        {
            query = query.Where(x => x.BuyerId == buyerId);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public void Add(Order order)
    {
        _dbContext.Orders.Add(order);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
