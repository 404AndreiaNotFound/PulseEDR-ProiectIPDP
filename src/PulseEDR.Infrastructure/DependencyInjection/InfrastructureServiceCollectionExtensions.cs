using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Infrastructure.Cve.Sources;
using PulseEDR.Infrastructure.Persistence;
using PulseEDR.Infrastructure.Persistence.Repositories;

namespace PulseEDR.Infrastructure.DependencyInjection;

/// <summary>
/// One-stop registration for the Infrastructure layer.
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

        // --- Repositories + Unit of Work ---
        services.AddScoped<IScanResultRepository, ScanResultRepository>();
        services.AddScoped<ICveRepository, CveRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // --- CVE feed sources (Adapter pattern). Order matters: first wins on merge. ---
        services.AddScoped<ICveFeedSource, LocalCatalogCveFeedSource>();

        // NVD source uses a typed HttpClient with a reasonable timeout.
        services.AddHttpClient<ICveFeedSource, NvdCveFeedSource>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "PulseEDR/1.0");
        });

        return services;
    }
}