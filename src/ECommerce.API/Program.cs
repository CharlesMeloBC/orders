using ECommerce.API.Endpoints;
using ECommerce.API.Middlewares;
using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

LoadDotEnvIfPresent();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

await ApplyDatabaseMigrationsAsync(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthEndpoints();
app.MapOrderEndpoints();

app.Run();

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    const int maxAttempts = 60;
    var delay = TimeSpan.FromSeconds(2);

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseMigration");

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            logger.LogInformation("Applying database migrations. Attempt {Attempt}/{MaxAttempts}.", attempt, maxAttempts);

            var strategy = db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () => await db.Database.MigrateAsync(app.Lifetime.ApplicationStopping));

            logger.LogInformation("Database migrations applied successfully.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                ex,
                "Database migration failed. Waiting {DelaySeconds} seconds before retrying.",
                delay.TotalSeconds);

            await Task.Delay(delay, app.Lifetime.ApplicationStopping);
        }
    }
}

static void LoadDotEnvIfPresent()
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
