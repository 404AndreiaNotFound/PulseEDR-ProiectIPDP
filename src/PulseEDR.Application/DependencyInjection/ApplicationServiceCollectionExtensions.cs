using Microsoft.Extensions.DependencyInjection;
using PulseEDR.Application.Detection;
using PulseEDR.Application.Detection.Detectors;
using PulseEDR.Domain.Abstractions;

namespace PulseEDR.Application.DependencyInjection;

/// <summary>
/// Registration entry-point for the Application layer.
/// Registers:
///   - all IDetector implementations (Strategy pattern)
///   - DetectorFactory (Factory pattern)
///   - RiskCalculator (Single Responsibility)
///   - concrete services (added incrementally as they're implemented)
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // --- Detectors (Strategy implementations) ---
        // Order does NOT matter — DetectorFactory enumerates all of them.
        services.AddScoped<IDetector, UnsafeDownloadDetector>();
        services.AddScoped<IDetector, SuspiciousProcessDetector>();
        services.AddScoped<IDetector, RiskyConnectionDetector>();
        services.AddScoped<IDetector, OutdatedSoftwareDetector>();

        // --- Factory + Calculator ---
        services.AddScoped<IDetectorFactory, DetectorFactory>();
        services.AddScoped<IRiskCalculator, RiskCalculator>();

        // --- Application Services ---
        // Concrete services (ScanService, AlertService, etc.) are added below
        // as they are implemented in Etapa 3.3.

        return services;
    }
}