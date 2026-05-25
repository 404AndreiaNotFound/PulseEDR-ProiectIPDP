using PulseEDR.Domain.Entities;

namespace PulseEDR.Domain.Abstractions;

/// <summary>
/// Contract for the local agent that collects host telemetry.
/// Implementations live in the Agent project (Windows-specific code).
/// </summary>
public interface IAgentScanner
{
    /// <summary>
    /// Performs a full local scan (processes + connections + software + recent files)
    /// and returns a populated ScanResult (without alerts/score yet).
    /// </summary>
    Task<ScanResult> PerformScanAsync(CancellationToken ct = default);
}