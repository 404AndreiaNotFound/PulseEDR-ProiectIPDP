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
/// <summary>
    /// Ports considered "normal" — connections on these are NOT flagged
    /// as uncommon. Covers HTTP/HTTPS, DNS, common dev/DB workloads, and
    /// the ephemeral loopback ranges used by browsers/IPC.
    /// </summary>
    public static readonly HashSet<int> CommonPorts = new()
    {
        // Web
        80, 443, 8080, 8443,
        // DNS / DHCP
        53, 67, 68,
        // Mail (rare on a desktop but standard)
        25, 465, 587, 110, 143, 993, 995,
        // SSH / Git
        22,
        // Dev databases (Postgres / Mongo / Redis / MSSQL / MySQL)
        5432, 27017, 6379, 1433, 3306,
        // Local dev servers commonly used (ASP.NET Core / Vite / Node / Webpack)
        3000, 3001, 4000, 4200, 5000, 5001, 5015, 5173, 5432, 7000, 7001, 8000, 9000,
        // Steam, Discord, common gaming clients (best-effort)
        27015, 27017, 27036
    };

    /// <summary>
    /// Address prefixes considered "local/private" — connections whose
    /// remote address falls in these ranges are NOT flagged as "outbound to
    /// public IP". Covers RFC1918 private, loopback, link-local, and IPv6 loopback.
    /// </summary>
    public static readonly string[] LocalAddressPrefixes = new[]
    {
        "127.",          // loopback IPv4
        "10.",           // RFC1918
        "192.168.",      // RFC1918
        "172.16.", "172.17.", "172.18.", "172.19.",
        "172.20.", "172.21.", "172.22.", "172.23.",
        "172.24.", "172.25.", "172.26.", "172.27.",
        "172.28.", "172.29.", "172.30.", "172.31.", // RFC1918 (172.16-31)
        "169.254.",      // link-local
        "::1",           // IPv6 loopback
        "fe80:",         // IPv6 link-local
        "fc", "fd"       // IPv6 unique-local
    };

/// <summary>
    /// User-writable folders considered "hot zones" — frequently abused by
    /// social-engineering malware to drop droppers and second-stage payloads.
    /// Temp is intentionally NOT here: it contains legitimate system files
    /// (logs, installers, browser cache) and produces too much noise.
    /// </summary>
    public static readonly string[] WatchedFolders = new[]
    {
        "Downloads",
        "Desktop"
    };

    // Score weights per finding type (sum is clamped to 100 by RiskScore).
    public const int ScoreExeInDownloads          = 30;
    public const int ScoreProcessFromDownloads    = 28;
    public const int ScoreLolBinInteractive       = 18;
    public const int ScoreUncommonRemotePort      = 18;
    public const int ScorePublicIpOutbound        = 10;
    public const int ScoreOutdatedSoftwareDefault = 25;
}