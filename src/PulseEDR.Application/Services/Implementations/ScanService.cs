using Microsoft.Extensions.Logging;
using PulseEDR.Application.Detection;
using PulseEDR.Application.Dtos;
using PulseEDR.Application.Mappings;
using PulseEDR.Application.Services.Abstractions;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Application.Services.Implementations;

/// <summary>
/// Orchestrates the full scan lifecycle:
///   1. Agent collects host telemetry → ScanResult.
///   2. Every IDetector analyzes the scan and produces Alerts.
///   3. RiskCalculator aggregates the alerts into a final RiskScore.
///   4. Everything is persisted via the repository + unit of work.
///
/// Heavily uses Dependency Inversion: ScanService depends on abstractions
/// (IAgentScanner, IDetectorFactory, IRiskCalculator, IScanResultRepository),
/// not on concrete EF Core / Windows code.
/// </summary>
public class ScanService : IScanService
{
    private readonly IAgentScanner _agent;
    private readonly IDetectorFactory _detectorFactory;
    private readonly IRiskCalculator _calculator;
    private readonly IScanResultRepository _scanRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ScanService> _log;

    public ScanService(
        IAgentScanner agent,
        IDetectorFactory detectorFactory,
        IRiskCalculator calculator,
        IScanResultRepository scanRepo,
        IUnitOfWork uow,
        ILogger<ScanService> log)
    {
        _agent = agent;
        _detectorFactory = detectorFactory;
        _calculator = calculator;
        _scanRepo = scanRepo;
        _uow = uow;
        _log = log;
    }

    public async Task<ScanDetailDto> RunScanAsync(CancellationToken ct = default)
    {
        _log.LogInformation("Starting scan...");

        // Step 1 — Agent collects host telemetry.
        var scan = await _agent.PerformScanAsync(ct);
        scan.Start();

        _log.LogInformation(
            "Agent collected: {Procs} processes, {Conns} connections, " +
            "{Sw} software entries, {Files} recent files",
            scan.Processes.Count, scan.Connections.Count,
            scan.Software.Count, scan.RecentFiles.Count);

        // Step 2 — Run all detectors (Strategy pattern in action).
        var detectors = _detectorFactory.GetAll();
        foreach (var detector in detectors)
        {
            var alerts = await detector.AnalyzeAsync(scan, ct);
            _log.LogInformation(
                "Detector {Name} produced {Count} alerts",
                detector.Name, alerts.Count);

            foreach (var alert in alerts)
                scan.AddAlert(alert);
        }

        // Step 3 — Compute the final risk score.
        var score = _calculator.Calculate(scan.Alerts);
        scan.Complete(score);

        _log.LogInformation(
            "Scan complete. Total alerts={Alerts}, Risk score={Score} ({Severity})",
            scan.Alerts.Count, score.Value, score.Severity);

        // Step 4 — Persist.
        await _scanRepo.AddAsync(scan, ct);
        await _uow.SaveChangesAsync(ct);

        return scan.ToDetailDto();
    }

    public async Task<ScanDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var scan = await _scanRepo.GetWithDetailsAsync(id, ct);
        return scan?.ToDetailDto();
    }

    public async Task<PaginatedResult<ScanSummaryDto>> ListRecentAsync(
        int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        // For brevity: we use the existing helper and apply pagination in-memory.
        // In production with many scans we'd add a paginated query in the repo.
        var recent = await _scanRepo.GetRecentAsync(page * pageSize, ct);
        var total = await _scanRepo.CountAsync(ct);

        var items = recent
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.ToSummaryDto())
            .ToList();

        return new PaginatedResult<ScanSummaryDto>(items, total, page, pageSize);
    }

    public async Task<ScanDetailDto?> GetLatestCompletedAsync(CancellationToken ct = default)
    {
        var latest = await _scanRepo.GetLatestCompletedAsync(ct);
        if (latest is null) return null;

        // Load with details (the helper returns a tracking-free shallow scan).
        return await GetByIdAsync(latest.Id, ct);
    }
}