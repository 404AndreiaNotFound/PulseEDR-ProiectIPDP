using PulseEDR.Domain.Common;
using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Domain.Entities;

/// <summary>
/// Aggregate root for a complete scan: collected data + alerts + risk score.
/// Owns child collections; external code adds children via the methods below.
/// </summary>
public class ScanResult : Entity, IAuditable
{
    private readonly List<ProcessInfo> _processes = new();
    private readonly List<NetworkConnection> _connections = new();
    private readonly List<InstalledSoftware> _software = new();
    private readonly List<RecentFile> _recentFiles = new();
    private readonly List<Alert> _alerts = new();

    public string MachineName { get; private set; } = string.Empty;
    public string OsVersion { get; private set; } = string.Empty;
    public ScanStatus Status { get; private set; }
    public RiskScore? RiskScore { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<ProcessInfo>       Processes   => _processes.AsReadOnly();
    public IReadOnlyCollection<NetworkConnection> Connections => _connections.AsReadOnly();
    public IReadOnlyCollection<InstalledSoftware> Software    => _software.AsReadOnly();
    public IReadOnlyCollection<RecentFile>        RecentFiles => _recentFiles.AsReadOnly();
    public IReadOnlyCollection<Alert>             Alerts      => _alerts.AsReadOnly();

    private ScanResult() { }

    public ScanResult(string machineName, string osVersion)
    {
        MachineName = machineName;
        OsVersion = osVersion;
        Status = ScanStatus.Pending;
        StartedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void Start() => Status = ScanStatus.Running;

    public void AddProcess(ProcessInfo p)           => _processes.Add(p);
    public void AddConnection(NetworkConnection c)  => _connections.Add(c);
    public void AddSoftware(InstalledSoftware s)    => _software.Add(s);
    public void AddRecentFile(RecentFile f)         => _recentFiles.Add(f);

    public void AddAlert(Alert alert)
    {
        alert.AttachToScan(Id);
        _alerts.Add(alert);
    }

    public void Complete(RiskScore score)
    {
        RiskScore = score;
        Status = ScanStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = ScanStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}