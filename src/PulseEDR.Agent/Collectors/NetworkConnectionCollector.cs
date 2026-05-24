using System.Net.NetworkInformation;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Agent.Collectors;

/// <summary>
/// Collects active TCP connections via System.Net.NetworkInformation.
/// IPGlobalProperties does not expose the owning PID — that requires
/// elevated WinAPI (GetExtendedTcpTable). Out of scope for now; OwnerPid is null.
/// </summary>
public static class NetworkConnectionCollector
{
    public static IEnumerable<NetworkConnection> Collect()
    {
        var props = IPGlobalProperties.GetIPGlobalProperties();

        // Active TCP connections
        foreach (var c in props.GetActiveTcpConnections())
        {
            yield return new NetworkConnection(
                localAddress: c.LocalEndPoint.Address.ToString(),
                localPort: c.LocalEndPoint.Port,
                remoteAddress: c.RemoteEndPoint.Address.ToString(),
                remotePort: c.RemoteEndPoint.Port,
                protocol: "TCP",
                state: c.State.ToString(),
                ownerPid: null);
        }
    }
}