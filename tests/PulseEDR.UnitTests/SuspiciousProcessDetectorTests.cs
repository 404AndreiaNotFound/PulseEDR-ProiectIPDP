using PulseEDR.Application.Detection.Detectors;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;

namespace PulseEDR.UnitTests;

public class SuspiciousProcessDetectorTests
{
    private readonly SuspiciousProcessDetector _detector = new();

    private static ScanResult MakeScan(List<ProcessInfo> processes)
    {
        var scan = new ScanResult("TEST-PC", "Windows 11");
        foreach (var p in processes) scan.AddProcess(p);
        return scan;
    }

    [Fact]
    public async Task LolBinPowerShell_ShouldReturnAlert()
    {
        var scan = MakeScan(new List<ProcessInfo>
        {
            new ProcessInfo(
                pid: 1234,
                name: "powershell.exe",
                executablePath: @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                commandLine: null,
                parentPid: null,
                startTime: DateTime.UtcNow)
        });

        var alerts = await _detector.AnalyzeAsync(scan);

        Assert.NotEmpty(alerts);
        Assert.Contains(alerts, a => a.Title.Contains("powershell.exe"));
    }

    [Fact]
    public async Task RegularProcess_ShouldReturnNoAlerts()
    {
        var scan = MakeScan(new List<ProcessInfo>
        {
            new ProcessInfo(
                pid: 5678,
                name: "notepad.exe",
                executablePath: @"C:\Windows\System32\notepad.exe",
                commandLine: null,
                parentPid: null,
                startTime: DateTime.UtcNow)
        });

        var alerts = await _detector.AnalyzeAsync(scan);

        Assert.Empty(alerts);
    }
}