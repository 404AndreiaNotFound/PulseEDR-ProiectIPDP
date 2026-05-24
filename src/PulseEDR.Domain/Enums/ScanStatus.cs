namespace PulseEDR.Domain.Enums;

/// <summary>
/// Lifecycle states for a scan execution.
/// </summary>
public enum ScanStatus
{
    Pending,
    Running,
    Completed,
    Failed
}