namespace PulseEDR.Domain.Enums;

/// <summary>
/// CVSS-aligned severity levels for alerts and CVEs.
/// </summary>
public enum Severity
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}