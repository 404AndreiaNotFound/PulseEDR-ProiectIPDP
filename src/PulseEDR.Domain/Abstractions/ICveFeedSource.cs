using PulseEDR.Domain.Entities;

namespace PulseEDR.Domain.Abstractions;

/// <summary>
/// Contract for a CVE feed source. Implementations adapt external formats
/// (local JSON, NVD API, Vulners, etc.) into the internal CveEntry model.
///
/// Pattern: Adapter. Decouples the importer from the specific feed.
/// </summary>
public interface ICveFeedSource
{
    /// <summary>
    /// Human-friendly name of the source (used in logs and metadata).
    /// </summary>
    string SourceName { get; }

    /// <summary>
    /// Fetches available CVE entries from the underlying source.
    /// Returns an empty list if the source is temporarily unavailable
    /// (network error, rate limit) — never throws to keep importer resilient.
    /// </summary>
    Task<IReadOnlyList<CveEntry>> FetchAsync(CancellationToken ct = default);
}