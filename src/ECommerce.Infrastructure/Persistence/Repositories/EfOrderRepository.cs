using Microsoft.EntityFrameworkCore;
using ECommerce.Application.Abstractions;
using ECommerce.Domain.Orders;

namespace ECommerce.Infrastructure.Persistence.Repositories;

internal sealed class EfOrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _dbContext;

    public EfOrderRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id, Guid buyerId, bool includeItems, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = _dbContext.Orders;

        if (includeItems)
        {
            query = query
                .Include(x => x.Items)
                .ThenInclude(x => x.Product);
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id && x.BuyerId == buyerId, cancellationToken);
    }

    public async Task<List<Order>> ListAsync(Guid buyerId, OrderStatus? status, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = _dbContext.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Where(x => x.BuyerId == buyerId);

        if (status is not null)
        {
            query = query.Where(x => x.Status == status);
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
