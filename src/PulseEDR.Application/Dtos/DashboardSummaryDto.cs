namespace PulseEDR.Application.Dtos;

/// <summary>
/// High-level summary for the WPF Dashboard view.
/// Mirrors the legacy prototype's DashboardSummary but uses typed RiskScore.
/// </summary>
public sealed record DashboardSummaryDto(
    RiskScoreDto? CurrentRiskScore,
    int TotalScans,
    int TotalAlerts,
    int CriticalAlerts,
    int KnownVulnerabilities,
    DateTime? LastScanAt);