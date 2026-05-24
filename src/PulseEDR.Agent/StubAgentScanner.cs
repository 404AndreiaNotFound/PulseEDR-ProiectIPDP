using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Agent;

/// <summary>
/// Temporary in-memory agent that returns a deterministic sample scan.
/// Used until the real Windows collectors are wired (planned Day 4).
///
/// IMPORTANT: this is intentionally simple — it lets the API and detectors
/// be exercised end-to-end before the OS-specific collection code exists.
/// </summary>
public class StubAgentScanner : IAgentScanner
{
    public Task<ScanResult> PerformScanAsync(CancellationToken ct = default)
    {
        var scan = new ScanResult(
            machineName: Environment.MachineName,
            osVersion: Environment.OSVersion.ToString());

        // Sample processes: one safe, one in Downloads as a risky example.
        scan.AddProcess(new ProcessInfo(
            pid: 1234,
            name: "explorer.exe",
            executablePath: "C:\\Windows\\explorer.exe",
            commandLine: null,
            parentPid: 1000,
            startTime: DateTime.UtcNow.AddHours(-2)));

        scan.AddProcess(new ProcessInfo(
            pid: 5678,
            name: "invoice_viewer.exe",
            executablePath: "C:\\Users\\Demo\\Downloads\\invoice_viewer.exe",
            commandLine: "C:\\Users\\Demo\\Downloads\\invoice_viewer.exe --open",
            parentPid: 1234,
            startTime: DateTime.UtcNow.AddMinutes(-3)));

        // Sample connections: one normal (Google HTTPS), one suspicious (uncommon port + public IP).
        scan.AddConnection(new NetworkConnection(
            localAddress: "192.168.1.50",
            localPort: 51234,
            remoteAddress: "142.250.200.78",
            remotePort: 443,
            protocol: "TCP",
            state: "ESTABLISHED",
            ownerPid: 1234));

        scan.AddConnection(new NetworkConnection(
            localAddress: "192.168.1.50",
            localPort: 51301,
            remoteAddress: "185.234.219.55",
            remotePort: 4444,
            protocol: "TCP",
            state: "ESTABLISHED",
            ownerPid: 5678));

        // Sample installed software (with CVE-vulnerable versions).
        scan.AddSoftware(new InstalledSoftware(
            name: "Google Chrome",
            version: "120.0.0.0",
            publisher: "Google LLC",
            installLocation: "C:\\Program Files\\Google\\Chrome",
            installDate: DateTime.UtcNow.AddMonths(-6)));

        scan.AddSoftware(new InstalledSoftware(
            name: "7-Zip",
            version: "23.01",
            publisher: "Igor Pavlov",
            installLocation: "C:\\Program Files\\7-Zip",
            installDate: DateTime.UtcNow.AddYears(-1)));

        // Sample recent files in hot zones.
        scan.AddRecentFile(new RecentFile(
            fullPath: "C:\\Users\\Demo\\Downloads\\invoice_viewer.exe",
            fileName: "invoice_viewer.exe",
            extension: ".exe",
            sizeBytes: 1234567,
            lastModified: DateTime.UtcNow.AddMinutes(-4),
            sha256: "a1b2c3d4e5f6"));

        scan.AddRecentFile(new RecentFile(
            fullPath: "C:\\Users\\Demo\\Desktop\\setup.ps1",
            fileName: "setup.ps1",
            extension: ".ps1",
            sizeBytes: 2048,
            lastModified: DateTime.UtcNow.AddHours(-1),
            sha256: null));

        return Task.FromResult(scan);
    }
}