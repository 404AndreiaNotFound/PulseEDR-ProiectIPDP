using PulseEDR.Application.Dtos;

namespace PulseEDR.Application.Services.Abstractions;

/// <summary>
/// Simulates the impact of remediating one or more alerts on the risk score.
/// "What would my score be if I uninstalled X / killed process Y?"
///
/// This is one of PulseEDR's key differentiators (explainability + actionable).
/// </summary>
public interface IWhatIfService
{
    /// <summary>
    /// Recomputes the risk score for the given scan, ignoring the alerts
    /// whose ids are in dismissedAlertIds.
    /// </summary>
    Task<WhatIfResultDto> SimulateAsync(
        Guid scanId,
        IReadOnlyCollection<Guid> dismissedAlertIds,
        CancellationToken ct = default);
}

/// <summary>
/// Result of a what-if simulation: the original and projected risk scores.
/// </summary>
public sealed record WhatIfResultDto(
    Guid ScanId,
    RiskScoreDto OriginalScore,
    RiskScoreDto ProjectedScore,
    int Delta);