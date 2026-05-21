using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Infrastructure.Persistence;
using PulseEDR.Infrastructure.Persistence.Repositories;

namespace PulseEDR.Infrastructure.DependencyInjection;

/// <summary>
/// One-stop registration for everything in the Infrastructure layer.
/// API/Desktop projects just call services.AddInfrastructure(configuration).
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Missing connection string 'DefaultConnection'.");

        services.AddDbContext<PulseEdrDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(PulseEdrDbContext).Assembly.FullName);
            }));

        services.AddScoped<IScanResultRepository, ScanResultRepository>();
        services.AddScoped<ICveRepository, CveRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}