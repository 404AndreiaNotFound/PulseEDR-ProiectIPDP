using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;

namespace PulseEDR.Infrastructure.Cve.Sources;

/// <summary>
/// Fetches recent CVEs from the National Vulnerability Database (NVD) REST API.
/// Documentation: https://nvd.nist.gov/developers/vulnerabilities
///
/// Pattern: Adapter. Normalizes the NVD shape into our internal CveEntry model.
/// Network errors are swallowed (logged) so the importer keeps working with
/// the local catalog even when NVD is offline.
/// </summary>
public class NvdCveFeedSource : ICveFeedSource
{
    private const string EndpointBase = "https://services.nvd.nist.gov/rest/json/cves/2.0";
    private readonly HttpClient _http;
    private readonly ILogger<NvdCveFeedSource> _log;

    public string SourceName => "NVD";

    public NvdCveFeedSource(HttpClient http, ILogger<NvdCveFeedSource> log)
    {
        _http = http;
        _log = log;
    }

    public async Task<IReadOnlyList<CveEntry>> FetchAsync(CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ss.fff");
        var until = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        var url = $"{EndpointBase}?pubStartDate={since}&pubEndDate={until}&resultsPerPage=20";

        try
        {
            _log.LogInformation("NVD adapter: fetching recent CVEs from {Url}", url);

            var response = await _http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _log.LogWarning("NVD adapter: non-success status {Status}. Skipping import.", (int)response.StatusCode);
                return Array.Empty<CveEntry>();
            }

            var nvdResponse = await response.Content.ReadFromJsonAsync<NvdResponse>(cancellationToken: ct);

            if (nvdResponse?.Vulnerabilities is null || nvdResponse.Vulnerabilities.Count == 0)
            {
                _log.LogInformation("NVD adapter: no new CVEs returned.");
                return Array.Empty<CveEntry>();
            }

            var entries = nvdResponse.Vulnerabilities
                .Select(MapToInternal)
                .Where(e => e is not null)
                .Select(e => e!)
                .ToList();

            _log.LogInformation("NVD adapter: mapped {Count} entries.", entries.Count);
            return entries;
        }
        catch (TaskCanceledException)
        {
            _log.LogWarning("NVD adapter: request timed out.");
            return Array.Empty<CveEntry>();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "NVD adapter: unexpected error during fetch.");
            return Array.Empty<CveEntry>();
        }
    }

    private static CveEntry? MapToInternal(NvdVulnerabilityWrapper wrapper)
    {
        var v = wrapper.Cve;
        if (v is null || string.IsNullOrEmpty(v.Id)) return null;

        var desc = v.Descriptions?.FirstOrDefault(d => d.Lang == "en")?.Value ?? "(no description)";

        var metric = v.Metrics?.CvssMetricV31?.FirstOrDefault()?.CvssData;
        var cvssScore = metric?.BaseScore ?? 0d;
        var severity = MapSeverity(metric?.BaseSeverity);

        var productName = v.Configurations?
            .FirstOrDefault()?.Nodes?
            .FirstOrDefault()?.CpeMatch?
            .FirstOrDefault()?.Criteria ?? "Unknown";

        var publishedAt = v.Published ?? DateTime.UtcNow;

        return new CveEntry(
            cveId: v.Id,
            productName: ExtractProductFromCpe(productName),
            maxAffectedVersionExclusive: "0.0.0",
            fixedVersion: "(see NVD)",
            severity: severity,
            cvssScore: cvssScore,
            description: desc,
            remediation: "Refer to the NVD entry for vendor-recommended remediation.",
            referenceUrl: $"https://nvd.nist.gov/vuln/detail/{v.Id}",
            publishedAt: publishedAt);
    }

    private static string ExtractProductFromCpe(string cpe)
    {
        if (string.IsNullOrEmpty(cpe) || !cpe.StartsWith("cpe:")) return cpe;
        var parts = cpe.Split(':');
        if (parts.Length < 5) return cpe;
        return parts[4].Replace("_", " ");
    }

    private static Severity MapSeverity(string? nvdSeverity) =>
        nvdSeverity?.ToUpperInvariant() switch
        {
            "CRITICAL" => Severity.Critical,
            "HIGH"     => Severity.High,
            "MEDIUM"   => Severity.Medium,
            "LOW"      => Severity.Low,
            _          => Severity.None
        };

    private sealed class NvdResponse
    {
        [JsonPropertyName("vulnerabilities")]
        public List<NvdVulnerabilityWrapper>? Vulnerabilities { get; set; }
    }

    private sealed class NvdVulnerabilityWrapper
    {
        [JsonPropertyName("cve")]
        public NvdCve? Cve { get; set; }
    }

    private sealed class NvdCve
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("published")]
        public DateTime? Published { get; set; }

        [JsonPropertyName("descriptions")]
        public List<NvdDescription>? Descriptions { get; set; }

        [JsonPropertyName("metrics")]
        public NvdMetrics? Metrics { get; set; }

        [JsonPropertyName("configurations")]
        public List<NvdConfiguration>? Configurations { get; set; }
    }

    private sealed class NvdDescription
    {
        [JsonPropertyName("lang")] public string Lang { get; set; } = "";
        [JsonPropertyName("value")] public string Value { get; set; } = "";
    }

    private sealed class NvdMetrics
    {
        [JsonPropertyName("cvssMetricV31")]
        public List<NvdCvssMetric>? CvssMetricV31 { get; set; }
    }

    private sealed class NvdCvssMetric
    {
        [JsonPropertyName("cvssData")]
        public NvdCvssData? CvssData { get; set; }
    }

    private sealed class NvdCvssData
    {
        [JsonPropertyName("baseScore")] public double BaseScore { get; set; }
        [JsonPropertyName("baseSeverity")] public string? BaseSeverity { get; set; }
    }

    private sealed class NvdConfiguration
    {
        [JsonPropertyName("nodes")] public List<NvdNode>? Nodes { get; set; }
    }

    private sealed class NvdNode
    {
        [JsonPropertyName("cpeMatch")] public List<NvdCpeMatch>? CpeMatch { get; set; }
    }

    private sealed class NvdCpeMatch
    {
        [JsonPropertyName("criteria")] public string? Criteria { get; set; }
    }
}