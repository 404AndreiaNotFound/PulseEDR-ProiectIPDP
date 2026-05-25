using Microsoft.Extensions.Logging;
using PulseEDR.Application.Detection;
using PulseEDR.Application.Dtos;
using PulseEDR.Application.Mappings;
using PulseEDR.Application.Services.Abstractions;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Application.Services.Implementations;

/// <summary>
/// Constructs a deterministic, realistic threat chain (no random fluctuations)
/// and runs it through the real detection pipeline + risk calculator.
///
/// The scan is a fully-formed ScanResult with the exact telemetry needed for
/// each detector to fire — what would happen if a user actually downloaded
/// a malicious .exe, launched it, and it phoned home.
/// </summary>
public class DemoScenarioService : IDemoScenarioService
{
    private readonly IDetectorFactory _detectorFactory;
    private readonly IRiskCalculator _calculator;
    private readonly IScanResultRepository _scanRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DemoScenarioService> _log;

    public DemoScenarioService(
        IDetectorFactory detectorFactory,
        IRiskCalculator calculator,
        IScanResultRepository scanRepo,
        IUnitOfWork uow,
        ILogger<DemoScenarioService> log)
    {
        _detectorFactory = detectorFactory;
        _calculator = calculator;
        _scanRepo = scanRepo;
        _uow = uow;
        _log = log;
    }

    public async Task<ScanDetailDto> RunThreatChainAsync(CancellationToken ct = default)
    {
        _log.LogInformation("DemoScenario: building canonical threat chain...");

        var scan = BuildThreatChainScan();
        scan.Start();

        // Run the SAME detector pipeline used by real scans — no shortcuts.
        foreach (var detector in _detectorFactory.GetAll())
        {
            var alerts = await detector.AnalyzeAsync(scan, ct);
            foreach (var alert in alerts) scan.AddAlert(alert);
            _log.LogInformation(
                "DemoScenario: {Detector} produced {Count} alerts.",
                detector.Name, alerts.Count);
        }

        var score = _calculator.Calculate(scan.Alerts);
        scan.Complete(score);

        _log.LogInformation(
            "DemoScenario complete. {Alerts} alerts, score={Score} ({Severity}).",
            scan.Alerts.Count, score.Value, score.Severity);

        await _scanRepo.AddAsync(scan, ct);
        await _uow.SaveChangesAsync(ct);

        return scan.ToDetailDto();
    }

    /// <summary>
    /// Hand-crafted ScanResult containing the canonical "infected user" pattern.
    /// Each piece of data is designed to trip a specific detector.
    /// </summary>
    private static ScanResult BuildThreatChainScan()
    {
        var scan = new ScanResult(
            machineName: "DEMO-HOST",
            osVersion: "Windows 11 (Demo)");

        // --- 1. Recent file: a fake "invoice viewer" dropped in Downloads ---
        // (Trips UnsafeDownloadDetector)
        scan.AddRecentFile(new RecentFile(
            fullPath: "C:\\Users\\Demo\\Downloads\\invoice_viewer.exe",
            fileName: "invoice_viewer.exe",
            extension: ".exe",
            sizeBytes: 1457280,
            lastModified: DateTime.UtcNow.AddMinutes(-5),
            sha256: "demo-hash-001"));

        // --- 2. PowerShell script on Desktop (another risky location + LOLBin link) ---
        scan.AddRecentFile(new RecentFile(
            fullPath: "C:\\Users\\Demo\\Desktop\\update.ps1",
            fileName: "update.ps1",
            extension: ".ps1",
            sizeBytes: 4096,
            lastModified: DateTime.UtcNow.AddMinutes(-2),
            sha256: "demo-hash-002"));

        // --- 3. Process: invoice_viewer.exe running from Downloads ---
        // (Trips SuspiciousProcessDetector — hot-zone rule)
        scan.AddProcess(new ProcessInfo(
            pid: 6606,
            name: "invoice_viewer.exe",
            executablePath: "C:\\Users\\Demo\\Downloads\\invoice_viewer.exe",
            commandLine: "C:\\Users\\Demo\\Downloads\\invoice_viewer.exe /open",
            parentPid: 4100,
            startTime: DateTime.UtcNow.AddMinutes(-4)));

        // --- 4. Process: PowerShell launched (LOLBin) ---
        // (Trips SuspiciousProcessDetector — LOLBin rule)
        scan.AddProcess(new ProcessInfo(
            pid: 7821,
            name: "powershell.exe",
            executablePath: "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
            commandLine: "powershell.exe -ExecutionPolicy Bypass -File update.ps1",
            parentPid: 6606,
            startTime: DateTime.UtcNow.AddMinutes(-3)));

        // --- 5. Outbound connection to public IP on uncommon port ---
        // (Trips RiskyConnectionDetector — public IP + uncommon port = HIGH)
        scan.AddConnection(new NetworkConnection(
            localAddress: "192.168.1.50",
            localPort: 52341,
            remoteAddress: "185.234.219.55",  // public, EU-based, suspicious
            remotePort: 4444,                  // Metasploit default reverse shell
            protocol: "TCP",
            state: "ESTABLISHED",
            ownerPid: 6606));

        // --- 6. Outdated Chrome (matches CVE-2024-0519 in the local catalog) ---
        // (Trips OutdatedSoftwareDetector + CVE matching)
        scan.AddSoftware(new InstalledSoftware(
            name: "Google Chrome",
            version: "120.0.0.0",
            publisher: "Google LLC",
            installLocation: "C:\\Program Files\\Google\\Chrome",
            installDate: DateTime.UtcNow.AddMonths(-6)));

        // --- 7. Outdated 7-Zip (matches CVE-2024-11477) ---
        scan.AddSoftware(new InstalledSoftware(
            name: "7-Zip",
            version: "22.01",
            publisher: "Igor Pavlov",
            installLocation: "C:\\Program Files\\7-Zip",
            installDate: DateTime.UtcNow.AddYears(-1)));

        return scan;
    }
}