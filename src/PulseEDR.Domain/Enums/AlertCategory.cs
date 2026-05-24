namespace PulseEDR.Domain.Enums;

/// <summary>
/// Logical grouping of alerts based on the type of finding.
/// Mapped from the legacy prototype's alert categories
/// (Execution risk, Suspicious execution, Suspicious outbound, Vulnerability exposure).
/// </summary>
public enum AlertCategory
{
    SuspiciousProcess,
    OutdatedSoftware,
    RiskyConnection,
    UnsafeDownload,
    WeakConfiguration,
    KnownVulnerability,
    Other
}