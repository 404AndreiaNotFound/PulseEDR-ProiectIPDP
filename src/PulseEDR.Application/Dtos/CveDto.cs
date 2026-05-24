namespace PulseEDR.Application.Dtos;

/// <summary>
/// CVE entry returned by the catalog API.
/// </summary>
public sealed record CveDto(
    Guid Id,
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