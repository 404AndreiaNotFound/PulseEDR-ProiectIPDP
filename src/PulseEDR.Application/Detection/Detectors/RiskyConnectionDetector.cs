using System.Net;
using PulseEDR.Domain.Abstractions;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.Application.Detection.Detectors;

/// <summary>
/// Flags outbound TCP connections that look suspicious:
///  - remote port is not in the common set (80/443/53/22/...)
///  - remote IP is public (not RFC1918 / loopback).
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

            var uncommonPort = !DetectionConstants.CommonPorts.Contains(conn.RemotePort);
            var publicIp = IsPublicIp(conn.RemoteAddress);

            if (uncommonPort && publicIp)
            {
                alerts.Add(BuildAlert(conn,
                    severity: Severity.High,
                    score: DetectionConstants.ScoreUncommonRemotePort
                         + DetectionConstants.ScorePublicIpOutbound,
                    title: $"Outbound to public IP on uncommon port: {conn.RemoteAddress}:{conn.RemotePort}",
                    reason: "Combination of public IP and uncommon port is " +
                            "frequently used by malware C2 channels."));
            }
            else if (uncommonPort)
            {
                alerts.Add(BuildAlert(conn,
                    severity: Severity.Medium,
                    score: DetectionConstants.ScoreUncommonRemotePort,
                    title: $"Outbound on uncommon port: {conn.RemotePort}",
                    reason: "Connection to a non-standard port. Verify the owning process."));
            }
            else if (publicIp)
            {
                alerts.Add(BuildAlert(conn,
                    severity: Severity.Low,
                    score: DetectionConstants.ScorePublicIpOutbound,
                    title: $"Outbound to public IP: {conn.RemoteAddress}",
                    reason: "Public IP destination. Cross-check with expected behavior."));
            }
        }

        return Task.FromResult<IReadOnlyList<Alert>>(alerts);
    }

    private static Alert BuildAlert(
        NetworkConnection conn, Severity severity, int score, string title, string reason)
    {
        var ev = new Evidence(
            DetectorName: nameof(RiskyConnectionDetector),
            Description: reason,
            Facts: new Dictionary<string, string>
            {
                ["LocalEndpoint"] = $"{conn.LocalAddress}:{conn.LocalPort}",
                ["RemoteEndpoint"] = $"{conn.RemoteAddress}:{conn.RemotePort}",
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
            remediation: "Identify the owning process. Block the destination " +
                         "in the host firewall if it is not legitimate.",
            evidence: ev);
    }

    private static bool IsPublicIp(string address)
    {
        if (!IPAddress.TryParse(address, out var ip))
            return false;

        var bytes = ip.GetAddressBytes();
        if (bytes.Length != 4) return false;  // IPv6 — out of scope for prototype

        // Loopback
        if (bytes[0] == 127) return false;
        // RFC1918 private ranges
        if (bytes[0] == 10) return false;
        if (bytes[0] == 192 && bytes[1] == 168) return false;
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false;
        // Link-local
        if (bytes[0] == 169 && bytes[1] == 254) return false;
        // Multicast (224.0.0.0/4) and reserved
        if (bytes[0] >= 224) return false;

        return true;
    }
}