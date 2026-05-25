using Microsoft.EntityFrameworkCore;
using PulseEDR.Application.Dtos;
using PulseEDR.Application.Mappings;
using PulseEDR.Application.Services.Abstractions;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Enums;
using PulseEDR.Infrastructure.Persistence;

namespace PulseEDR.Application.Services.Implementations;

/// <summary>
/// Builds the Dashboard summary view shown on the WPF home screen.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IScanResultRepository _scans;
    private readonly PulseEdrDbContext _db;

    public DashboardService(IScanResultRepository scans, PulseEdrDbContext db)
    {
        _scans = scans;
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var latest = await _scans.GetLatestCompletedAsync(ct);

        var totalScans = await _db.Scans.CountAsync(ct);
        var totalAlerts = await _db.Alerts.CountAsync(ct);
        var critical = await _db.Alerts.CountAsync(a => a.Severity == Severity.Critical, ct);
        var knownVulns = await _db.Alerts.CountAsync(
            a => a.Category == AlertCategory.OutdatedSoftware ||
                 a.Category == AlertCategory.KnownVulnerability, ct);

        return new DashboardSummaryDto(
            CurrentRiskScore: latest?.RiskScore.ToDto(),
            TotalScans: totalScans,
            TotalAlerts: totalAlerts,
            CriticalAlerts: critical,
            KnownVulnerabilities: knownVulns,
            LastScanAt: latest?.CompletedAt);
    }
}