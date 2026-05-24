using PulseEDR.Application.Detection.Detectors;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;

namespace PulseEDR.UnitTests;

public class UnsafeDownloadDetectorTests
{
    private readonly UnsafeDownloadDetector _detector = new();

    private static string Combine(params string[] parts) =>
        string.Join(Path.DirectorySeparatorChar.ToString(), parts);

    private static ScanResult MakeScan(List<RecentFile> files)
    {
        var scan = new ScanResult("TEST-PC", "Windows 11");
        foreach (var f in files) scan.AddRecentFile(f);
        return scan;
    }

    [Fact]
    public async Task ExeInDownloads_ShouldReturnHighAlert()
    {
        var path = Combine("C:", "Users", "test", "Downloads", "malware.exe");
        var scan = MakeScan(new List<RecentFile>
        {
            new RecentFile(
                fullPath: path,
                fileName: "malware.exe",
                extension: ".exe",
                sizeBytes: 1024,
                lastModified: DateTime.UtcNow,
                sha256: null)
        });

        var alerts = await _detector.AnalyzeAsync(scan);

        Assert.Single(alerts);
        Assert.Equal(Severity.High, alerts[0].Severity);
    }

    [Fact]
    public async Task TxtInDownloads_ShouldReturnNoAlerts()
    {
        var path = Combine("C:", "Users", "test", "Downloads", "readme.txt");
        var scan = MakeScan(new List<RecentFile>
        {
            new RecentFile(
                fullPath: path,
                fileName: "readme.txt",
                extension: ".txt",
                sizeBytes: 512,
                lastModified: DateTime.UtcNow,
                sha256: null)
        });

        var alerts = await _detector.AnalyzeAsync(scan);

        Assert.Empty(alerts);
    }

    [Fact]
    public async Task ExeInSystem32_ShouldReturnNoAlerts()
    {
        var path = Combine("C:", "Windows", "System32", "notepad.exe");
        var scan = MakeScan(new List<RecentFile>
        {
            new RecentFile(
                fullPath: path,
                fileName: "notepad.exe",
                extension: ".exe",
                sizeBytes: 1024,
                lastModified: DateTime.UtcNow,
                sha256: null)
        });

        var alerts = await _detector.AnalyzeAsync(scan);

        Assert.Empty(alerts);
    }
}