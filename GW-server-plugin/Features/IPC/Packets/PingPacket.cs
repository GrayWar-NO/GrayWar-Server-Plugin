namespace GW_server_plugin.Features.IPC.Packets;

public class PingPacket : CommunicationPacket
{
    public string Data { get; set; }

    /// <summary>
    /// Process method for Ping packet.
    /// orders sending of that very same packet
    /// </summary>
    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger?.LogDebug($"Ping received with data: {Data}");
        return this;
    }
}