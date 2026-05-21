using PulseEDR.Domain.Entities;

namespace PulseEDR.Domain.Abstractions;

/// <summary>
/// Repository specialization for ScanResult aggregate.
/// Exposes queries that are specific to scans (with includes for child entities).
/// </summary>
public interface IScanResultRepository : IRepository<ScanResult>
{
    /// <summary>
    /// Loads a scan with all child collections (processes, connections,
    /// software, files, alerts) eagerly populated.
    /// </summary>
    Task<ScanResult?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns the most recent N scans (without child details, for listing UI).
    /// </summary>
    Task<IReadOnlyList<ScanResult>> GetRecentAsync(int take, CancellationToken ct = default);

    /// <summary>
    /// Returns the latest completed scan, if any. Used by Dashboard.
    /// </summary>
    Task<ScanResult?> GetLatestCompletedAsync(CancellationToken ct = default);
}