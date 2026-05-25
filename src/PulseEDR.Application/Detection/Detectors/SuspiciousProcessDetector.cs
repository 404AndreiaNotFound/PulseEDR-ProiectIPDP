using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Application.Detection.Detectors;

/// <summary>
/// Flags processes that fit suspicious patterns:
///  - executable lives in a user-writable hot zone (Downloads/Desktop/Temp)
///  - process is a LOLBin (PowerShell, cmd, rundll32, etc.) running interactively.
/// </summary>
public class SuspiciousProcessDetector : IDetector
{
    public string Name => nameof(SuspiciousProcessDetector);

    public Task<IReadOnlyList<Alert>> AnalyzeAsync(
        ScanResult scan, CancellationToken ct = default)
    {
        var alerts = new List<Alert>();

        foreach (var process in scan.Processes)
        {
            // Rule 1: process launched from a hot-zone folder.
            if (!string.IsNullOrEmpty(process.ExecutablePath) &&
                IsInWatchedFolder(process.ExecutablePath))
            {
                alerts.Add(CreateHotZoneAlert(process));
            }

            // Rule 2: LOLBin executed.
            if (DetectionConstants.LolBinNames.Contains(process.Name))
            {
                alerts.Add(CreateLolBinAlert(process));
            }
        }

        return Task.FromResult<IReadOnlyList<Alert>>(alerts);
    }

    private static Alert CreateHotZoneAlert(ProcessInfo p)
    {
        var ev = new Evidence(
            DetectorName: nameof(SuspiciousProcessDetector),
            Description: "Process started from a user-writable folder.",
            Facts: new Dictionary<string, string>
            {
                ["Pid"] = p.Pid.ToString(),
                ["Name"] = p.Name,
                ["ExecutablePath"] = p.ExecutablePath ?? "(unknown)",
                ["StartTime"] = p.StartTime.ToString("u")
            });

        return new Alert(
            category: AlertCategory.SuspiciousProcess,
            severity: Severity.High,
            score: DetectionConstants.ScoreProcessFromDownloads,
            title: $"Process started from hot zone: {p.Name}",
            description: "A running process is executing from a folder commonly " +
                         "used to drop malware (Downloads/Desktop/Temp).",
            remediation: "Terminate the process via Task Manager and remove the " +
                         "originating file if it is not trusted.",
            evidence: ev);
    }

    private static Alert CreateLolBinAlert(ProcessInfo p)
    {
        var ev = new Evidence(
            DetectorName: nameof(SuspiciousProcessDetector),
            Description: "Living-Off-The-Land Binary (LOLBin) was executed.",
            Facts: new Dictionary<string, string>
            {
                ["Pid"] = p.Pid.ToString(),
                ["Name"] = p.Name,
                ["CommandLine"] = p.CommandLine ?? "(unknown)"
            });

        return new Alert(
            category: AlertCategory.SuspiciousProcess,
            severity: Severity.Medium,
            score: DetectionConstants.ScoreLolBinInteractive,
            title: $"LOLBin in use: {p.Name}",
            description: "A binary that attackers commonly abuse for " +
                         "command execution and lateral movement is running.",
            remediation: "Confirm this is an expected use. If not, investigate the " +
                         "parent process and the command line.",
            evidence: ev);
    }

    private static bool IsInWatchedFolder(string fullPath) =>
        DetectionConstants.WatchedFolders
            .Any(f => fullPath.Contains(
                Path.DirectorySeparatorChar + f + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase));
}