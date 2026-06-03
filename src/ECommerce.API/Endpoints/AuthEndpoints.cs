using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/token", (AuthTokenRequest request, IConfiguration configuration) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Document))
            {
                return Results.BadRequest(new { message = "name and document are required." });
            }

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
}

public sealed record AuthTokenRequest(string Name, string Document);
