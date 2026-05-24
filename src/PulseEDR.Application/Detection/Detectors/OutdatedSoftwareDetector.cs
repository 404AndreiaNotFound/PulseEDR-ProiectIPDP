using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Application.Detection.Detectors;

/// <summary>
/// Matches installed software against the CVE catalog by product name and version.
/// A software item is "vulnerable" when its version is strictly less than
/// CveEntry.MaxAffectedVersionExclusive.
/// </summary>
public class OutdatedSoftwareDetector : IDetector
{
    private readonly ICveRepository _cves;

    public string Name => nameof(OutdatedSoftwareDetector);

    public OutdatedSoftwareDetector(ICveRepository cves) => _cves = cves;

    public async Task<IReadOnlyList<Alert>> AnalyzeAsync(
        ScanResult scan, CancellationToken ct = default)
    {
        var alerts = new List<Alert>();

        foreach (var software in scan.Software)
        {
            var matchingCves = await _cves.FindByProductAsync(software.Name, ct);
            if (matchingCves.Count == 0) continue;

            foreach (var cve in matchingCves)
            {
                if (!IsVersionAffected(software.Version, cve.MaxAffectedVersionExclusive))
                    continue;

                alerts.Add(BuildAlert(software, cve));
            }
        }

        return alerts;
    }

    private static Alert BuildAlert(InstalledSoftware sw, CveEntry cve)
    {
        var ev = new Evidence(
            DetectorName: nameof(OutdatedSoftwareDetector),
            Description: $"Installed {sw.Name} {sw.Version} is affected by {cve.CveId}.",
            Facts: new Dictionary<string, string>
            {
                ["Product"] = sw.Name,
                ["InstalledVersion"] = sw.Version,
                ["MaxAffectedExclusive"] = cve.MaxAffectedVersionExclusive,
                ["FixedVersion"] = cve.FixedVersion,
                ["CvssScore"] = cve.CvssScore.ToString("F1"),
                ["CveDescription"] = cve.Description
            });

        // Use CVE's own severity to weight the alert (Critical = 30, High = 25, etc.).
        var score = cve.Severity switch
        {
            Severity.Critical => 30,
            Severity.High     => 25,
            Severity.Medium   => 15,
            Severity.Low      => 8,
            _                 => DetectionConstants.ScoreOutdatedSoftwareDefault
        };

        return new Alert(
            category: AlertCategory.OutdatedSoftware,
            severity: cve.Severity,
            score: score,
            title: $"{sw.Name} {sw.Version} is affected by {cve.CveId}",
            description: cve.Description,
            remediation: cve.Remediation +
                         $" Recommended version: {cve.FixedVersion} or later.",
            evidence: ev,
            cveId: cve.CveId);
    }

    /// <summary>
    /// Compares installed version to "max affected version (exclusive)".
    /// Returns true if installed &lt; maxExclusive.
    /// Falls back to string comparison if Version.TryParse fails.
    /// </summary>
    private static bool IsVersionAffected(string installed, string maxExclusive)
    {
        if (Version.TryParse(installed, out var v1) &&
            Version.TryParse(maxExclusive, out var v2))
        {
            return v1 < v2;
        }

        // Fallback (best effort) — lexicographic ordering for non-numeric versions.
        return string.Compare(installed, maxExclusive,
            StringComparison.OrdinalIgnoreCase) < 0;
    }
}