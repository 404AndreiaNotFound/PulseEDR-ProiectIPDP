using Microsoft.Win32;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Agent.Collectors;

/// <summary>
/// Reads installed software entries from the Windows registry's
/// Uninstall keys (both 32-bit and 64-bit views, per-machine and per-user).
/// </summary>
public static class InstalledSoftwareCollector
{
    private static readonly string[] HiveRoots = new[]
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

    public static IEnumerable<InstalledSoftware> Collect()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (baseKey, rootPath) in EnumerateRoots())
        {
            using var root = baseKey.OpenSubKey(rootPath);
            if (root is null) continue;

            foreach (var subKeyName in root.GetSubKeyNames())
            {
                using var sub = root.OpenSubKey(subKeyName);
                if (sub is null) continue;

                var displayName = sub.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(displayName)) continue;

                var version = sub.GetValue("DisplayVersion") as string ?? "0.0.0";
                var publisher = sub.GetValue("Publisher") as string;
                var location = sub.GetValue("InstallLocation") as string;
                var installDateRaw = sub.GetValue("InstallDate") as string;

                DateTime? installDate = null;
                if (!string.IsNullOrEmpty(installDateRaw) &&
                    DateTime.TryParseExact(installDateRaw, "yyyyMMdd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var parsed))
                {
                    installDate = parsed;
                }

                var dedupKey = $"{displayName}|{version}";
                if (!seen.Add(dedupKey)) continue;

                yield return new InstalledSoftware(
                    name: displayName,
                    version: version,
                    publisher: publisher,
                    installLocation: location,
                    installDate: installDate);
            }
        }
    }

    private static IEnumerable<(RegistryKey BaseKey, string Path)> EnumerateRoots()
    {
        foreach (var path in HiveRoots)
        {
            yield return (Registry.LocalMachine, path);
            yield return (Registry.CurrentUser, path);
        }
    }
}