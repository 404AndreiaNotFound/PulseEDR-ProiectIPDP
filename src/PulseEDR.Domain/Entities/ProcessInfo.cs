using PulseEDR.Domain.Common;

namespace PulseEDR.Domain.Entities;

/// <summary>
/// A snapshot of a running process captured by the agent during a scan.
/// </summary>
public class ProcessInfo : Entity
{
    public Guid ScanResultId { get; private set; }
    public int Pid { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ExecutablePath { get; private set; }
    public string? CommandLine { get; private set; }
    public int? ParentPid { get; private set; }
    public DateTime StartTime { get; private set; }

    private ProcessInfo() { }

    public ProcessInfo(int pid, string name, string? executablePath,
                      string? commandLine, int? parentPid, DateTime startTime)
    {
        Pid = pid;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ExecutablePath = executablePath;
        CommandLine = commandLine;
        ParentPid = parentPid;
        StartTime = startTime;
    }
}