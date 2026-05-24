using System.Diagnostics;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Agent.Collectors;

/// <summary>
/// Collects a snapshot of running processes on the local machine.
/// Best-effort: some processes (System, secure, elevated) deny access to
/// MainModule / StartTime; we skip them silently.
/// </summary>
public static class ProcessCollector
{
    public static IEnumerable<ProcessInfo> Collect()
    {
        foreach (var p in Process.GetProcesses())
        {
            ProcessInfo? snapshot = null;

            try
            {
                string? exePath = null;
                DateTime startTime = DateTime.UtcNow;

                try { exePath = p.MainModule?.FileName; }
                catch { /* access denied — keep null */ }

                try { startTime = p.StartTime.ToUniversalTime(); }
                catch { /* access denied — keep default */ }

                snapshot = new ProcessInfo(
                    pid: p.Id,
                    name: p.ProcessName + ".exe",
                    executablePath: exePath,
                    commandLine: null,    // command line requires WMI; skipped for now
                    parentPid: null,      // parent pid also requires WMI; skipped
                    startTime: startTime);
            }
            catch
            {
                // ignore process — has died between enumeration and read
            }
            finally
            {
                p.Dispose();
            }

            if (snapshot is not null)
                yield return snapshot;
        }
    }
}