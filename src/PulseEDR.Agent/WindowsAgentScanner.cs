using Microsoft.Extensions.Logging;
using PulseEDR.Agent.Collectors;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Agent;

/// <summary>
/// Real Windows-based implementation of IAgentScanner.
/// Combines the four collectors (processes, network, software, files) and
/// returns a fully-populated ScanResult ready for the detection pipeline.
///
/// Each collector is invoked under a try/catch so a partial failure
/// (e.g. denied registry hive) still yields a usable scan.
/// </summary>
public class WindowsAgentScanner : IAgentScanner
{
    private readonly ILogger<WindowsAgentScanner> _log;

    public WindowsAgentScanner(ILogger<WindowsAgentScanner> log) => _log = log;

    public Task<ScanResult> PerformScanAsync(CancellationToken ct = default)
    {
        var scan = new ScanResult(
            machineName: Environment.MachineName,
            osVersion: Environment.OSVersion.ToString());

        SafeCollect("processes",
            () => ProcessCollector.Collect(),
            scan.AddProcess);

        SafeCollect("connections",
            () => NetworkConnectionCollector.Collect(),
            scan.AddConnection);

        SafeCollect("software",
            () => InstalledSoftwareCollector.Collect(),
            scan.AddSoftware);

        SafeCollect("recent files",
            () => RecentFileCollector.Collect(),
            scan.AddRecentFile);

        _log.LogInformation(
            "Windows agent scan complete: {P} processes, {C} connections, " +
            "{S} software, {F} files.",
            scan.Processes.Count, scan.Connections.Count,
            scan.Software.Count, scan.RecentFiles.Count);

        return Task.FromResult(scan);
    }

    private void SafeCollect<T>(
        string name, Func<IEnumerable<T>> source, Action<T> sink)
    {
        try
        {
            foreach (var item in source())
                sink(item);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex,
                "Windows agent: collector '{Name}' failed; continuing with partial data.",
                name);
        }
    }
}