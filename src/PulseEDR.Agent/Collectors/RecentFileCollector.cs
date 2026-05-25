using PulseEDR.Domain.Entities;

namespace PulseEDR.Agent.Collectors;

/// <summary>
/// Enumerates files in known "hot zones" (Downloads, Desktop) that were
/// touched recently. We do NOT compute SHA-256 by default — it's expensive
/// for large files and not required by the detectors.
/// </summary>
public static class RecentFileCollector
{
    private static readonly TimeSpan RecentWindow = TimeSpan.FromDays(7);
    private const int MaxFilesPerFolder = 200;

    public static IEnumerable<RecentFile> Collect()
    {
        foreach (var folder in EnumerateHotZones())
        {
            if (!Directory.Exists(folder)) continue;

            DirectoryInfo dir;
            try { dir = new DirectoryInfo(folder); }
            catch { continue; }

            FileInfo[] files;
            try
            {
                files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                continue;
            }

            var cutoff = DateTime.UtcNow - RecentWindow;
            var recentFiles = files
                .Where(f =>
                {
                    try { return f.LastWriteTimeUtc >= cutoff; }
                    catch { return false; }
                })
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .Take(MaxFilesPerFolder);

            foreach (var f in recentFiles)
            {
                yield return new RecentFile(
                    fullPath: f.FullName,
                    fileName: f.Name,
                    extension: string.IsNullOrEmpty(f.Extension) ? "(none)" : f.Extension,
                    sizeBytes: f.Length,
                    lastModified: f.LastWriteTimeUtc,
                    sha256: null);  // expensive — skipped
            }
        }
    }

    private static IEnumerable<string> EnumerateHotZones()
    {
        // %USERPROFILE%\Downloads
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(userProfile))
            yield return Path.Combine(userProfile, "Downloads");

        // Desktop
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (!string.IsNullOrEmpty(desktop))
            yield return desktop;

        // %TEMP%
        var temp = Path.GetTempPath();
        if (!string.IsNullOrEmpty(temp))
            yield return temp;
    }
}