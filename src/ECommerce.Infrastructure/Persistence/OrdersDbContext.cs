using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Buyers;
using ECommerce.Domain.Orders;
using ECommerce.Domain.Products;

namespace ECommerce.Infrastructure.Persistence;

public sealed class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        modelBuilder.Entity<Order>().HasQueryFilter(x => x.DeletedAtUtc == null);
    }
}
