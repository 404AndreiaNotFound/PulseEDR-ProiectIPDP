using Microsoft.Extensions.DependencyInjection;

namespace PulseEDR.Application.DependencyInjection;

/// <summary>
/// Registration entry-point for the Application layer.
/// Services concretes are added here in Day 3 (after implementations are written).
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Concrete services (ScanService, AlertService, etc.) will be wired here
        // once implementations are added in Day 3.
        return services;
    }
}