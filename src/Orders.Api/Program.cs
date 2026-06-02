using Microsoft.AspNetCore.Mvc;
using Orders.Application;
using Orders.Application.Orders;
using Orders.Application.Orders.Dtos;
using Orders.Domain.Exceptions;
using Orders.Domain.Orders;
using Orders.Infrastructure;

LoadDotEnvIfPresent();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (DomainException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Regra de negócio violada",
            Detail = ex.Message
        });
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

var v1 = app.MapGroup("/api/v1");
var orders = v1.MapGroup("/orders").WithTags("Orders");

orders.MapPost("", async (CreateOrderRequest request, IOrdersService service, CancellationToken ct) =>
{
    var errors = ValidateCreate(request);
    if (errors.Count != 0)
    {
        return Results.ValidationProblem(errors);
    }

    var created = await service.CreateAsync(request, ct);
    return Results.Created($"/api/v1/orders/{created.Id}", created);
});

orders.MapGet("", async (string? status, Guid? buyerId, IOrdersService service, CancellationToken ct) =>
{
    OrderStatus? parsedStatus = null;
    if (!string.IsNullOrWhiteSpace(status))
    {
        if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var s))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["status"] = [$"Status inválido. Valores aceitos: {string.Join(", ", Enum.GetNames<OrderStatus>())}"]
            });
        }

        parsedStatus = s;
    }

    var result = await service.ListAsync(parsedStatus, buyerId, ct);
    return Results.Ok(result);
});

orders.MapGet("/{id:guid}", async (Guid id, IOrdersService service, CancellationToken ct) =>
{
    var order = await service.GetByIdAsync(id, ct);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

orders.MapPut("/{id:guid}", async (Guid id, UpdateOrderRequest request, IOrdersService service, CancellationToken ct) =>
{
    var errors = ValidateUpdate(request);
    if (errors.Count != 0)
    {
        return Results.ValidationProblem(errors);
    }

    var updated = await service.UpdateAsync(id, request, ct);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

orders.MapDelete("/{id:guid}", async (Guid id, IOrdersService service, CancellationToken ct) =>
{
    var cancelled = await service.CancelAsync(id, ct);
    return cancelled is null ? Results.NotFound() : Results.Ok(cancelled);
});

app.Run();

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

static Dictionary<string, string[]> ValidateCreate(CreateOrderRequest request)
{
    var errors = new Dictionary<string, string[]>();

    if (request.BuyerId == Guid.Empty)
    {
        errors["buyerId"] = ["buyerId é obrigatório."];
    }

    if (string.IsNullOrWhiteSpace(request.BuyerName))
    {
        errors["buyerName"] = ["buyerName é obrigatório."];
    }

    if (request.Items is null || request.Items.Count == 0)
    {
        errors["items"] = ["Pedido deve conter pelo menos um item."];
        return errors;
    }

    for (var i = 0; i < request.Items.Count; i++)
    {
        var item = request.Items[i];

        if (item.ProductId == Guid.Empty)
        {
            errors[$"items[{i}].productId"] = ["productId é obrigatório."];
        }

        if (string.IsNullOrWhiteSpace(item.ProductName))
        {
            errors[$"items[{i}].productName"] = ["productName é obrigatório."];
        }

        if (item.UnitPrice <= 0)
        {
            errors[$"items[{i}].unitPrice"] = ["unitPrice deve ser maior que zero."];
        }

        if (item.Quantity <= 0)
        {
            errors[$"items[{i}].quantity"] = ["quantity deve ser maior que zero."];
        }
    }

    return errors;
}

static Dictionary<string, string[]> ValidateUpdate(UpdateOrderRequest request)
{
    var errors = new Dictionary<string, string[]>();

    if (request.Items is null || request.Items.Count == 0)
    {
        errors["items"] = ["Pedido deve conter pelo menos um item."];
        return errors;
    }

    for (var i = 0; i < request.Items.Count; i++)
    {
        var item = request.Items[i];

        if (item.ProductId == Guid.Empty)
        {
            errors[$"items[{i}].productId"] = ["productId é obrigatório."];
        }

        if (string.IsNullOrWhiteSpace(item.ProductName))
        {
            errors[$"items[{i}].productName"] = ["productName é obrigatório."];
        }

        if (item.UnitPrice <= 0)
        {
            errors[$"items[{i}].unitPrice"] = ["unitPrice deve ser maior que zero."];
        }

        if (item.Quantity <= 0)
        {
            errors[$"items[{i}].quantity"] = ["quantity deve ser maior que zero."];
        }
    }

    return errors;
}
