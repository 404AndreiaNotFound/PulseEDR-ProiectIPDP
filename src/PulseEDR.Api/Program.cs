using PulseEDR.Api.BackgroundServices;
using PulseEDR.Application.DependencyInjection;
using PulseEDR.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Layer wiring 
builder.Services.AddInfrastructure(builder.Configuration);
// Agent (stub for now — real Windows collectors come in a bit later) 
builder.Services.AddScoped<PulseEDR.Domain.Abstractions.IAgentScanner,
    PulseEDR.Agent.StubAgentScanner>();
builder.Services.AddApplication();

// Swagger / OpenAPI 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Background services 
builder.Services.AddHostedService<CveImporterHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Minimal health endpoint
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "PulseEDR API",
    utcNow = DateTimeOffset.UtcNow
}));

app.Run();