using PulseEDR.Application.Dtos;

namespace PulseEDR.Application.Services.Abstractions;

/// <summary>
/// Orchestrates the full scan lifecycle: trigger agent collection,
/// run detectors, calculate risk score, persist results.
/// </summary>
public interface IScanService
{
    /// <summary>
    /// Triggers a new scan: agent collects host data, detectors analyze,
    /// risk score is computed, everything is persisted.
    /// </summary>
    Task<ScanDetailDto> RunScanAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single scan by id with all its details.
    /// </summary>
    Task<ScanDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lists recent scans (paginated) for the history view.
    /// </summary>
    Task<PaginatedResult<ScanSummaryDto>> ListRecentAsync(
        int page = 1, int pageSize = 20, CancellationToken ct = default);

    /// <summary>
    /// Returns the latest completed scan, if any. Used by Dashboard.
    /// </summary>
    Task<ScanDetailDto?> GetLatestCompletedAsync(CancellationToken ct = default);
}