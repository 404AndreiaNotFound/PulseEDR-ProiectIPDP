using PulseEDR.Application.Services.Abstractions;

namespace PulseEDR.Api.BackgroundServices;

/// <summary>
/// Runs once at API startup to import the local CVE catalog (cves.json) into
/// the database. In production this would also schedule periodic imports from
/// the real NVD feed; for the demo we only do the bootstrap import.
///
/// Pattern: BackgroundService (long-running hosted service).
/// </summary>
public class CveImporterHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CveImporterHostedService> _log;

    public CveImporterHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<CveImporterHostedService> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("CVE importer: starting bootstrap import...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var cveService = scope.ServiceProvider.GetRequiredService<ICveService>();

            var imported = await cveService.ImportFromLocalCatalogAsync(stoppingToken);
            _log.LogInformation(
                "CVE importer: bootstrap complete. {Count} new entries persisted.",
                imported);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CVE importer: bootstrap import failed.");
        }
    }
}