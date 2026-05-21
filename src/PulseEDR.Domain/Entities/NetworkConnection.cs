using PulseEDR.Domain.Common;

namespace PulseEDR.Domain.Entities;

/// <summary>
/// A snapshot of an active TCP/UDP connection at scan time.
/// </summary>
public class NetworkConnection : Entity
{
    public Guid ScanResultId { get; private set; }
    public string LocalAddress { get; private set; } = string.Empty;
    public int LocalPort { get; private set; }
    public string RemoteAddress { get; private set; } = string.Empty;
    public int RemotePort { get; private set; }
    public string Protocol { get; private set; } = "TCP";
    public string State { get; private set; } = string.Empty;
    public int? OwnerPid { get; private set; }

    private NetworkConnection() { }

    public NetworkConnection(string localAddress, int localPort,
                             string remoteAddress, int remotePort,
                             string protocol, string state, int? ownerPid)
    {
        LocalAddress = localAddress;
        LocalPort = localPort;
        RemoteAddress = remoteAddress;
        RemotePort = remotePort;
        Protocol = protocol;
        State = state;
        OwnerPid = ownerPid;
    }
}