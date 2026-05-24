using PulseEDR.Domain.Entities;

namespace PulseEDR.Domain.Abstractions;

/// <summary>
/// Repository specialization for CveEntry catalog.
/// </summary>
public interface ICveRepository : IRepository<CveEntry>
{
    /// <summary>
    /// Returns all CVE entries that affect a given product name (case-insensitive).
    /// Used by OutdatedSoftwareDetector to match installed software → vulnerabilities.
    /// </summary>
    Task<IReadOnlyList<CveEntry>> FindByProductAsync(string productName, CancellationToken ct = default);

    /// <summary>
    /// Bulk upsert used by the CVE feed importer (background service).
    /// </summary>
    Task UpsertManyAsync(IEnumerable<CveEntry> entries, CancellationToken ct = default);
}