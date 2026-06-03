using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/token", (AuthTokenRequest request, IConfiguration configuration, ILoggerFactory loggerFactory, HttpContext httpContext) =>
        {
            var logger = CreateLogger(loggerFactory);

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Document))
            {
                logger.LogWarning("Auth token request invalid. TraceId={TraceId}", httpContext.TraceIdentifier);
                return Results.BadRequest(new { message = "name and document are required." });
            }

            logger.LogInformation(
                "Auth token request started. TraceId={TraceId}",
                httpContext.TraceIdentifier);

            var buyerId = CreateDeterministicGuid(request.Document.Trim());
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtSecret(configuration)));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                claims:
                [
                    new Claim(ClaimTypes.NameIdentifier, buyerId.ToString()),
                    new Claim("buyerId", buyerId.ToString()),
                    new Claim(ClaimTypes.Name, request.Name.Trim()),
                    new Claim("document", request.Document.Trim())
                ],
                expires: expiresAt,
                signingCredentials: credentials);

            logger.LogInformation(
                "Auth token request completed. BuyerId={BuyerId} ExpiresAt={ExpiresAt} TraceId={TraceId}",
                buyerId,
                expiresAt,
                httpContext.TraceIdentifier);

            return Results.Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt
            });
        }).WithTags("Auth");

        return app;
    }

    public static string GetJwtSecret(IConfiguration configuration)
    {
        return configuration["Jwt:Secret"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? "development-secret-key-change-me-32";
    }

    private static Guid CreateDeterministicGuid(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var guidBytes = new byte[16];
        Array.Copy(bytes, guidBytes, 16);
        return new Guid(guidBytes);
    }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger("ECommerce.API.Endpoints.AuthEndpoints");
    }
}

public sealed record AuthTokenRequest(string Name, string Document);
