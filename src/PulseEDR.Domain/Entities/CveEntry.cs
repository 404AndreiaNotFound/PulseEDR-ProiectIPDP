using PulseEDR.Domain.Common;
using PulseEDR.Domain.Enums;

namespace PulseEDR.Domain.Entities;

/// <summary>
/// A normalized CVE record imported from a local catalog (cves.json)
/// or NVD feed. Mapped from legacy CveRecord with additional metadata.
/// </summary>
public class CveEntry : Entity, IAuditable
{
    public string CveId { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string MaxAffectedVersionExclusive { get; private set; } = string.Empty;
    public string FixedVersion { get; private set; } = string.Empty;
    public Severity Severity { get; private set; }
    public double CvssScore { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Remediation { get; private set; } = string.Empty;
    public string? ReferenceUrl { get; private set; }
    public DateTime PublishedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private CveEntry() { }

    public CveEntry(string cveId, string productName,
                    string maxAffectedVersionExclusive, string fixedVersion,
                    Severity severity, double cvssScore,
                    string description, string remediation,
                    string? referenceUrl, DateTime publishedAt)
    {
        CveId = cveId;
        ProductName = productName;
        MaxAffectedVersionExclusive = maxAffectedVersionExclusive;
        FixedVersion = fixedVersion;
        Severity = severity;
        CvssScore = cvssScore;
        Description = description;
        Remediation = remediation;
        ReferenceUrl = referenceUrl;
        PublishedAt = publishedAt;
        CreatedAt = DateTime.UtcNow;
    }
}