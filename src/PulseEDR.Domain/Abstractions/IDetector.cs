using PulseEDR.Domain.Entities;

namespace PulseEDR.Domain.Abstractions;

/// <summary>
/// Strategy contract for risk detectors. Each detector inspects a ScanResult
/// (which contains collected processes, connections, software, files)
/// and returns zero or more Alerts.
///
/// New detectors can be added without modifying existing code — this is the
/// Open/Closed Principle in action.
/// </summary>
public interface IDetector
{
    /// <summary>
    /// Human-friendly name of the detector (used in Evidence and logs).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Inspects the scan and returns alerts. Must NOT mutate the scan;
    /// alerts are attached by the orchestrator via ScanResult.AddAlert().
    /// </summary>
    Task<IReadOnlyList<Alert>> AnalyzeAsync(ScanResult scan, CancellationToken ct = default);
}