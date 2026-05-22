using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Application.Detection.Detectors;

/// <summary>
/// Flags executable / scriptable files placed in user-writable hot zones
/// (Downloads, Desktop, Temp). These are the most common entry points
/// for phishing and drive-by downloads.
/// </summary>
public class UnsafeDownloadDetector : IDetector
{
    public string Name => nameof(UnsafeDownloadDetector);

    public Task<IReadOnlyList<Alert>> AnalyzeAsync(
        ScanResult scan, CancellationToken ct = default)
    {
        var alerts = new List<Alert>();

        foreach (var file in scan.RecentFiles)
        {
            if (!DetectionConstants.RiskyExtensions.Contains(file.Extension))
                continue;

            if (!IsInWatchedFolder(file.FullPath))
                continue;

            var evidence = new Evidence(
                DetectorName: Name,
                Description: $"Executable/scriptable file found in user-writable folder.",
                Facts: new Dictionary<string, string>
                {
                    ["FullPath"] = file.FullPath,
                    ["Extension"] = file.Extension,
                    ["SizeBytes"] = file.SizeBytes.ToString(),
                    ["LastModified"] = file.LastModified.ToString("u")
                });

            alerts.Add(new Alert(
                category: AlertCategory.UnsafeDownload,
                severity: Severity.High,
                score: DetectionConstants.ScoreExeInDownloads,
                title: $"Risky file in hot zone: {file.FileName}",
                description: $"A {file.Extension} file was found in a user folder " +
                             "commonly abused by attackers (Downloads/Desktop/Temp).",
                remediation: "Verify the file's origin. If you did not intentionally " +
                             "place it there, delete it and run a full malware scan.",
                evidence: evidence));
        }

        return Task.FromResult<IReadOnlyList<Alert>>(alerts);
    }

    private static bool IsInWatchedFolder(string fullPath) =>
        DetectionConstants.WatchedFolders
            .Any(f => fullPath.Contains(
                Path.DirectorySeparatorChar + f + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase)
            || fullPath.EndsWith(Path.DirectorySeparatorChar + f,
                StringComparison.OrdinalIgnoreCase));
}