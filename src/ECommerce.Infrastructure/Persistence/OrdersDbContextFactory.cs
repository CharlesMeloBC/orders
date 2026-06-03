using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Persistence;

public sealed class OrdersDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        LoadDotEnvIfPresent();

        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("OrdersDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:OrdersDb não foi configurado. Defina no appsettings ou via variável de ambiente ConnectionStrings__OrdersDb (ex.: usando .env).");
        }

        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new OrdersDbContext(optionsBuilder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var repositoryRoot = FindRepositoryRoot();
        var apiDirectory = repositoryRoot is not null
            ? Path.Combine(repositoryRoot, "src", "ECommerce.API")
            : Directory.GetCurrentDirectory();

        return new ConfigurationBuilder()
            .SetBasePath(apiDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Orders.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static void LoadDotEnvIfPresent()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var dotEnvPath = Path.Combine(directory.FullName, ".env");
            if (File.Exists(dotEnvPath))
            {
                foreach (var line in File.ReadLines(dotEnvPath))
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                    {
                        continue;
                    }

                    var index = trimmed.IndexOf('=');
                    if (index <= 0)
                    {
                        continue;
                    }

                    var key = trimmed[..index].Trim();
                    var value = trimmed[(index + 1)..].Trim().Trim('"');
                    if (key.Length == 0)
                    {
                        continue;
                    }

                    if (Environment.GetEnvironmentVariable(key) is null)
                    {
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }

                return;
            }

            directory = directory.Parent;
        }
    }
}
