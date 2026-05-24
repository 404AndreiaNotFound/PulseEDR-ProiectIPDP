using System.Net;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Application.Detection.Detectors;

/// <summary>
/// Flags outbound TCP connections that look suspicious. Tuned to avoid
/// false positives on a typical developer machine:
///   - Local-only traffic (loopback, RFC1918, link-local, IPv6 loopback)
///     is skipped entirely.
///   - Connections on well-known ports (HTTPS, DNS, SSH, dev DBs) are skipped.
///   - Only public IP + uncommon port produces a High alert.
/// </summary>
public class RiskyConnectionDetector : IDetector
{
    public string Name => nameof(RiskyConnectionDetector);

    public Task<IReadOnlyList<Alert>> AnalyzeAsync(
        ScanResult scan, CancellationToken ct = default)
    {
        var alerts = new List<Alert>();

        foreach (var conn in scan.Connections)
        {
            if (!string.Equals(conn.Protocol, "TCP", StringComparison.OrdinalIgnoreCase))
                continue;

            if (IsLocalOrPrivate(conn.RemoteAddress))
                continue;

            var uncommonPort = !DetectionConstants.CommonPorts.Contains(conn.RemotePort);

            if (uncommonPort)
            {
                alerts.Add(BuildAlert(conn,
                    severity: Severity.High,
                    score: DetectionConstants.ScoreUncommonRemotePort
                         + DetectionConstants.ScorePublicIpOutbound,
                    title: "Outbound to public IP on uncommon port: " + conn.RemoteAddress + ":" + conn.RemotePort,
                    reason: "Combination of public IP and uncommon port is frequently used by malware C2 channels."));
            }
            else
            {
                alerts.Add(BuildAlert(conn,
                    severity: Severity.Low,
                    score: DetectionConstants.ScorePublicIpOutbound,
                    title: "Outbound to public IP: " + conn.RemoteAddress,
                    reason: "Public IP destination. Cross-check with expected behavior."));
            }
        }

        return Task.FromResult<IReadOnlyList<Alert>>(alerts);
    }

    private static bool IsLocalOrPrivate(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return true;

        if (address == "::1") return true;
        if (address.StartsWith("fe80:", StringComparison.OrdinalIgnoreCase)) return true;
        if (address.StartsWith("fc", StringComparison.OrdinalIgnoreCase)) return true;
        if (address.StartsWith("fd", StringComparison.OrdinalIgnoreCase)) return true;

        if (!IPAddress.TryParse(address, out var ip)) return true;
        var bytes = ip.GetAddressBytes();
        if (bytes.Length != 4) return false;

        if (bytes[0] == 127) return true;
        if (bytes[0] == 10) return true;
        if (bytes[0] == 192 && bytes[1] == 168) return true;
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
        if (bytes[0] == 169 && bytes[1] == 254) return true;
        if (bytes[0] >= 224) return true;

        return false;
    }

    private static Alert BuildAlert(
        NetworkConnection conn, Severity severity, int score, string title, string reason)
    {
        var ev = new Evidence(
            DetectorName: nameof(RiskyConnectionDetector),
            Description: reason,
            Facts: new Dictionary<string, string>
            {
                ["LocalEndpoint"] = conn.LocalAddress + ":" + conn.LocalPort,
                ["RemoteEndpoint"] = conn.RemoteAddress + ":" + conn.RemotePort,
                ["Protocol"] = conn.Protocol,
                ["State"] = conn.State,
                ["OwnerPid"] = conn.OwnerPid?.ToString() ?? "(unknown)"
            });

        return new Alert(
            category: AlertCategory.RiskyConnection,
            severity: severity,
            score: score,
            title: title,
            description: reason,
            remediation: "Identify the owning process. Block the destination in the host firewall if it is not legitimate.",
            evidence: ev);
    }
}