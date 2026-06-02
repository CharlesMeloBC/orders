using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Abstractions;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Persistence.Repositories;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrdersDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:OrdersDb não foi configurado.");
        }

        services.AddDbContext<OrdersDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IBuyerRepository, EfBuyerRepository>();
        services.AddScoped<IProductRepository, EfProductRepository>();
        services.AddScoped<IOrderRepository, EfOrderRepository>();

        return services;
    }
}
