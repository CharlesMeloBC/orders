using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Orders;

namespace Orders.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrdersService, OrdersService>();
        return services;
    }
}
