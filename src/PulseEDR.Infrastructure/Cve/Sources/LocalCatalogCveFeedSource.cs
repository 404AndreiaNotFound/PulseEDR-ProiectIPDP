using System.Text.Json;
using Microsoft.Extensions.Logging;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;

namespace PulseEDR.Infrastructure.Cve.Sources;

/// <summary>
/// Reads CVE entries from a JSON file shipped with the API (Data/cves.json).
/// Used for bootstrap + offline demos.
/// </summary>
public class LocalCatalogCveFeedSource : ICveFeedSource
{
    private readonly ILogger<LocalCatalogCveFeedSource> _log;
    private readonly string _filePath;

    public string SourceName => "LocalCatalog";

    public LocalCatalogCveFeedSource(ILogger<LocalCatalogCveFeedSource> log)
    {
        _log = log;
        _filePath = Path.Combine(AppContext.BaseDirectory, "Data", "cves.json");
    }

    public async Task<IReadOnlyList<CveEntry>> FetchAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath))
        {
            _log.LogWarning("Local CVE catalog not found at {Path}", _filePath);
            return Array.Empty<CveEntry>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath, ct);
            var dtos = JsonSerializer.Deserialize<List<LocalCveDto>>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dtos is null) return Array.Empty<CveEntry>();

            return dtos.Select(d => new CveEntry(
                cveId: d.CveId,
                productName: d.ProductName,
                maxAffectedVersionExclusive: d.MaxAffectedVersionExclusive,
                fixedVersion: d.FixedVersion,
                severity: Enum.Parse<Severity>(d.Severity, ignoreCase: true),
                cvssScore: d.CvssScore,
                description: d.Description,
                remediation: d.Remediation,
                referenceUrl: d.ReferenceUrl,
                publishedAt: d.PublishedAt)).ToList();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to read local CVE catalog.");
            return Array.Empty<CveEntry>();
        }
    }

    /// <summary>
    /// Local shape of cves.json rows.
    /// </summary>
    private sealed record LocalCveDto(
        string CveId,
        string ProductName,
        string MaxAffectedVersionExclusive,
        string FixedVersion,
        string Severity,
        double CvssScore,
        string Description,
        string Remediation,
        string? ReferenceUrl,
        DateTime PublishedAt);
}