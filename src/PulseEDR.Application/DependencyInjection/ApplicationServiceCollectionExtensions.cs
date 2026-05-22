using Microsoft.Extensions.DependencyInjection;
using PulseEDR.Application.Detection;
using PulseEDR.Application.Detection.Detectors;
using PulseEDR.Application.Services.Abstractions;
using PulseEDR.Application.Services.Implementations;
using PulseEDR.Domain.Abstractions;

namespace PulseEDR.Application.DependencyInjection;

/// <summary>
/// Registration entry-point for the Application layer.
/// Registers:
///   - all IDetector implementations (Strategy pattern)
///   - DetectorFactory (Factory pattern)
///   - RiskCalculator (Single Responsibility)
///   - all concrete application services
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // --- Detectors (Strategy implementations) ---
        services.AddScoped<IDetector, UnsafeDownloadDetector>();
        services.AddScoped<IDetector, SuspiciousProcessDetector>();
        services.AddScoped<IDetector, RiskyConnectionDetector>();
        services.AddScoped<IDetector, OutdatedSoftwareDetector>();

        // --- Factory + Calculator ---
        services.AddScoped<IDetectorFactory, DetectorFactory>();
        services.AddScoped<IRiskCalculator, RiskCalculator>();

        // --- Application Services ---
        services.AddScoped<IScanService, ScanService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<ICveService, CveService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IWhatIfService, WhatIfService>();

        return services;
    }
}