using System.Text;
using System.Text.Json;
using ECommerce.API.Middlewares;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Tests.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenJsonBodyIsInvalid_ShouldReturnFriendlyBadRequest()
    {
        var middleware = new ExceptionHandlingMiddleware();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = _ => throw new JsonException("The JSON value could not be converted to System.Decimal.");

        await middleware.InvokeAsync(context, next);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("Invalid request body", body);
        Assert.Contains("decimal separator", body);
        Assert.DoesNotContain("could not be converted to System.Decimal", body);
    }
}
