namespace PulseEDR.Application.Detection;

/// <summary>
/// Detection thresholds and lists. These are inspired by the prototype's
/// scoring rules. In production we'd move them to a YAML policy file
/// (planned in Day 5 — policy-as-code feature).
/// </summary>
internal static class DetectionConstants
{
    // Risky extensions for files in Downloads/Desktop.
    public static readonly HashSet<string> RiskyExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".scr", ".js", ".vbs", ".bat", ".cmd", ".ps1", ".msi", ".jar"
    };

    // LOLBins — Living-Off-The-Land Binaries, often abused by attackers.
    public static readonly HashSet<string> LolBinNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "powershell.exe", "cmd.exe", "wmic.exe", "rundll32.exe", "regsvr32.exe",
        "mshta.exe", "cscript.exe", "wscript.exe", "certutil.exe", "bitsadmin.exe"
    };

    // Common safe ports — anything outside is treated as suspicious.
    public static readonly HashSet<int> CommonPorts = new()
    {
        80, 443, 53, 22, 25, 110, 143, 465, 587, 993, 995, 3389
    };

    // Folders that user-downloaded executables tend to land in (highest risk).
    public static readonly string[] WatchedFolders = new[]
    {
        "Downloads", "Desktop", "Temp"
    };

    // Score weights per finding type (sum is clamped to 100 by RiskScore).
    public const int ScoreExeInDownloads          = 30;
    public const int ScoreProcessFromDownloads    = 28;
    public const int ScoreLolBinInteractive       = 18;
    public const int ScoreUncommonRemotePort      = 18;
    public const int ScorePublicIpOutbound        = 10;
    public const int ScoreOutdatedSoftwareDefault = 25;
}