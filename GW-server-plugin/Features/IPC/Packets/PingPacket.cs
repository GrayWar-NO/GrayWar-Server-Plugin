namespace GW_server_plugin.Features.IPC.Packets;

/// <summary>
/// Packet type for ping.
/// </summary>
public class PingPacket : CommunicationPacket
{
    /// <summary>
    /// Ping data, to be printed in the logs.
    /// </summary>
    public string Data { get; set; } = null!;

    /// <summary>
    /// Process method for Ping packet.
    /// orders sending of that very same packet
    /// </summary>
    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger.LogDebug($"Ping received with data: {Data}");
        return this;
    }
}