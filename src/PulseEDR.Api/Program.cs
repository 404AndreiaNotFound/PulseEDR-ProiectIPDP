using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PulseEDR.Agent;
using PulseEDR.Api.BackgroundServices;
using PulseEDR.Api.Endpoints;
using PulseEDR.Application.DependencyInjection;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Layer wiring
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Agent (stub for now)
// builder.Services.AddScoped<IAgentScanner, StubAgentScanner>();

// Changed Agent
builder.Services.AddScoped<IAgentScanner, WindowsAgentScanner>();

// JWT Authentication
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var jwtKey      = builder.Configuration["Jwt:SigningKey"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PulseEDR API",
        Version = "v1",
        Description = "Light EDR + Security Coach — REST API"
    });

    // JWT bearer support in Swagger UI ("Authorize" button)
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {your token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// Background services
builder.Services.AddHostedService<CveImporterHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PulseEDR API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Health (public, no auth)
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "PulseEDR API",
    utcNow = DateTimeOffset.UtcNow
}))
.WithTags("Health")
.WithSummary("Simple liveness check.");

// Auth endpoints (public)
app.MapAuthEndpoints();

// Feature endpoints (protected with [Authorize] applied below)
app.MapScanEndpoints();
app.MapAlertEndpoints();
app.MapCveEndpoints();
app.MapDashboardEndpoints();
app.MapWhatIfEndpoints();
app.MapDemoEndpoints();

app.Run();

// Makes Program accessible for WebApplicationFactory in integration tests
public partial class Program { }