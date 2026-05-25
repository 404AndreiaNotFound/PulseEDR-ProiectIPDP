using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PulseEDR.Api.Endpoints;

/// <summary>
/// Authentication endpoints (login → JWT bearer token).
///
/// For the academic demo this validates a single hardcoded user from
/// configuration. In production we would use ASP.NET Identity + hashed
/// passwords stored in the database (with PBKDF2 / Argon2).
/// </summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .WithOpenApi();

        group.MapPost("/login", (LoginRequest request, IConfiguration cfg) =>
        {
            var demoUser = cfg.GetSection("DemoUser").Get<DemoUser>()
                ?? throw new InvalidOperationException("DemoUser config missing.");

            if (!string.Equals(request.Username, demoUser.Username, StringComparison.Ordinal) ||
                !string.Equals(request.Password, demoUser.Password, StringComparison.Ordinal))
            {
                return Results.Unauthorized();
            }

            var token = GenerateToken(request.Username, cfg);
            return Results.Ok(new LoginResponse(token, "Bearer", 3600));
        })
        .WithName("Login")
        .WithSummary("Exchanges credentials for a JWT bearer token (valid 60 min).")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static string GenerateToken(string username, IConfiguration cfg)
    {
        var jwt = cfg.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt config missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(ClaimTypes.Role, "User")
        };

        var jwtToken = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwt.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    // ----- Records / config shape -----
    public sealed record LoginRequest(string Username, string Password);
    public sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresInSeconds);

    private sealed class DemoUser
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    private sealed class JwtSettings
    {
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public string SigningKey { get; set; } = "";
        public int ExpirationMinutes { get; set; } = 60;
    }
}