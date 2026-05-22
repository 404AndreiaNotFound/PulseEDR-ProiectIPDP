using PulseEDR.Agent;
using PulseEDR.Api.BackgroundServices;
using PulseEDR.Api.Endpoints;
using PulseEDR.Application.DependencyInjection;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// --- Layer wiring ---
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// --- Agent (stub for now — real Windows collectors come in Day 4) ---
builder.Services.AddScoped<IAgentScanner, StubAgentScanner>();

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PulseEDR API",
        Version = "v1",
        Description = "Light EDR + Security Coach — REST API"
    });
});

// --- Background services ---
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

// --- Health endpoint ---
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "PulseEDR API",
    utcNow = DateTimeOffset.UtcNow
}))
.WithTags("Health")
.WithSummary("Simple liveness check.");

// --- Feature endpoints ---
app.MapScanEndpoints();
app.MapAlertEndpoints();
app.MapCveEndpoints();
app.MapDashboardEndpoints();
app.MapWhatIfEndpoints();
app.MapDemoEndpoints();

app.Run();