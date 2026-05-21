using PulseEDR.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (DbContext + Repositories + UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Minimal health endpoint (temporary)
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "PulseEDR API",
    utcNow = DateTimeOffset.UtcNow
}));

app.Run();