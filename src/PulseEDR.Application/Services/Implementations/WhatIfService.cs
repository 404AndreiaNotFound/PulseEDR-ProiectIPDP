using PulseEDR.Application.Dtos;
using PulseEDR.Application.Mappings;
using PulseEDR.Application.Services.Abstractions;
using PulseEDR.Domain.Abstractions;

namespace PulseEDR.Application.Services.Implementations;

/// <summary>
/// What-if simulator: "what would my score be if I dismissed alerts X, Y, Z?"
///
/// This is a key differentiator for PulseEDR — it lets users explore the impact
/// of remediations before committing. Powered by recomputing the RiskScore on
/// the remaining (non-dismissed) alerts.
/// </summary>
public class WhatIfService : IWhatIfService
{
    private readonly IScanResultRepository _scans;
    private readonly IRiskCalculator _calculator;

    public WhatIfService(IScanResultRepository scans, IRiskCalculator calculator)
    {
        _scans = scans;
        _calculator = calculator;
    }

    public async Task<WhatIfResultDto> SimulateAsync(
        Guid scanId,
        IReadOnlyCollection<Guid> dismissedAlertIds,
        CancellationToken ct = default)
    {
        var scan = await _scans.GetWithDetailsAsync(scanId, ct)
            ?? throw new InvalidOperationException(
                $"Scan {scanId} not found.");

        var originalScore = scan.RiskScore?.Value ?? 0;
        var originalSeverity = scan.RiskScore?.Severity.ToString() ?? "None";

        // Recompute the score on the alerts that would remain.
        var dismissedSet = new HashSet<Guid>(dismissedAlertIds);
        var remaining = scan.Alerts.Where(a => !dismissedSet.Contains(a.Id)).ToList();

        var projected = _calculator.Calculate(remaining);

        return new WhatIfResultDto(
            ScanId: scanId,
            OriginalScore: new RiskScoreDto(originalScore, originalSeverity),
            ProjectedScore: projected.ToDto()!,
            Delta: originalScore - projected.Value);
    }
}