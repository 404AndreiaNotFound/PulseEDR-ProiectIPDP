namespace PulseEDR.Application.Dtos;

/// <summary>
/// Risk score value as exposed to the UI.
/// </summary>
public sealed record RiskScoreDto(int Value, string Severity);

/// <summary>
/// Compact scan summary used in listing pages and dashboard.
/// </summary>
public sealed record ScanSummaryDto(
    Guid Id,
    string MachineName,
    string OsVersion,
    string Status,
    RiskScoreDto? RiskScore,
    int AlertCount,
    DateTime StartedAt,
    DateTime? CompletedAt);

/// <summary>
/// Full scan with all collected data and alerts. Used in scan details view
/// and by the what-if simulator.
/// </summary>
public sealed record ScanDetailDto(
    Guid Id,
    string MachineName,
    string OsVersion,
    string Status,
    RiskScoreDto? RiskScore,
    DateTime StartedAt,
    DateTime? CompletedAt,
    IReadOnlyList<ProcessInfoDto> Processes,
    IReadOnlyList<NetworkConnectionDto> Connections,
    IReadOnlyList<InstalledSoftwareDto> Software,
    IReadOnlyList<RecentFileDto> RecentFiles,
    IReadOnlyList<AlertDto> Alerts);