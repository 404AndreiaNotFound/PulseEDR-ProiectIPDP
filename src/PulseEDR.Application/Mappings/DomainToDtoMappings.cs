using PulseEDR.Application.Dtos;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Application.Mappings;

/// <summary>
/// Extension methods that convert Domain entities into Application DTOs.
/// Kept as pure functions for easy unit testing.
/// </summary>
public static class DomainToDtoMappings
{
    public static RiskScoreDto? ToDto(this RiskScore? score) =>
        score is null ? null : new RiskScoreDto(score.Value, score.Severity.ToString());

    public static EvidenceDto ToDto(this Evidence ev) =>
        new(ev.DetectorName, ev.Description,
            new Dictionary<string, string>(ev.Facts));

    public static AlertDto ToDto(this Alert a) =>
        new(a.Id, a.ScanResultId,
            a.Category.ToString(), a.Severity.ToString(),
            a.Score, a.Title, a.Description, a.Remediation,
            a.Evidence.ToDto(), a.CveId, a.CreatedAt);

    public static ProcessInfoDto ToDto(this ProcessInfo p) =>
        new(p.Pid, p.Name, p.ExecutablePath, p.CommandLine, p.ParentPid, p.StartTime);

    public static NetworkConnectionDto ToDto(this NetworkConnection c) =>
        new(c.LocalAddress, c.LocalPort, c.RemoteAddress, c.RemotePort,
            c.Protocol, c.State, c.OwnerPid);

    public static InstalledSoftwareDto ToDto(this InstalledSoftware s) =>
        new(s.Name, s.Version, s.Publisher, s.InstallLocation, s.InstallDate);

    public static RecentFileDto ToDto(this RecentFile f) =>
        new(f.FullPath, f.FileName, f.Extension, f.SizeBytes, f.LastModified, f.Sha256);

    public static CveDto ToDto(this CveEntry c) =>
        new(c.Id, c.CveId, c.ProductName,
            c.MaxAffectedVersionExclusive, c.FixedVersion,
            c.Severity.ToString(), c.CvssScore,
            c.Description, c.Remediation, c.ReferenceUrl, c.PublishedAt);

    public static ScanSummaryDto ToSummaryDto(this ScanResult s) =>
        new(s.Id, s.MachineName, s.OsVersion, s.Status.ToString(),
            s.RiskScore.ToDto(), s.Alerts.Count, s.StartedAt, s.CompletedAt);

    public static ScanDetailDto ToDetailDto(this ScanResult s) =>
        new(s.Id, s.MachineName, s.OsVersion, s.Status.ToString(),
            s.RiskScore.ToDto(),
            s.StartedAt, s.CompletedAt,
            s.Processes.Select(p => p.ToDto()).ToList(),
            s.Connections.Select(c => c.ToDto()).ToList(),
            s.Software.Select(sw => sw.ToDto()).ToList(),
            s.RecentFiles.Select(f => f.ToDto()).ToList(),
            s.Alerts.Select(a => a.ToDto()).ToList());
}