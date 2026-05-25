using PulseEDR.Domain.Common;

namespace PulseEDR.Domain.Entities;

/// <summary>
/// A piece of software found on the host (registry Uninstall keys).
/// Matched against CveEntry by OutdatedSoftwareDetector.
/// </summary>
public class InstalledSoftware : Entity
{
    public Guid ScanResultId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string? Publisher { get; private set; }
    public string? InstallLocation { get; private set; }
    public DateTime? InstallDate { get; private set; }

    private InstalledSoftware() { }

    public InstalledSoftware(string name, string version, string? publisher,
                             string? installLocation, DateTime? installDate)
    {
        Name = name;
        Version = version;
        Publisher = publisher;
        InstallLocation = installLocation;
        InstallDate = installDate;
    }
}