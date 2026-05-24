using PulseEDR.Application.Detection.Detectors;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;

namespace PulseEDR.UnitTests;

public class UnsafeDownloadDetectorTests
{
    private readonly UnsafeDownloadDetector _detector = new();

    private static ScanResult MakeScan(List<RecentFile> files)
    {
        var scan = new ScanResult("TEST-PC", "Windows 11");
        foreach (var f in files) scan.AddRecentFile(f);
        return scan;
    }

    [Fact]
    public async Task ExeInDownloads_ShouldReturnHighAlert()
    {
        var scan = MakeScan(new List<RecentFile>
        {
            new RecentFile(
                fullPath: @"C:\Users\test\Downloads\malware.exe",
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
        var scan = MakeScan(new List<RecentFile>
        {
            new RecentFile(
                fullPath: @"C:\Users\test\Downloads\readme.txt",
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
        var scan = MakeScan(new List<RecentFile>
        {
            new RecentFile(
                fullPath: @"C:\Windows\System32\notepad.exe",
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