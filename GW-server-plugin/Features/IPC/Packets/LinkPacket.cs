using GW_server_plugin.Enums;

namespace GW_server_plugin.Features.IPC.Packets;

/// <summary>
/// Packet sent when linking an user steam to discord. 
/// </summary>
public class LinkPacket: CommunicationPacket
{
    /// <inheritdoc />
    public override PacketType Type { get; set; } = PacketType.Link;


    /// <summary>
    /// SteamID of the one that requested a link.
    /// </summary>
    public ulong SteamID { get; set; }
    
    /// <summary>
    /// One time code associated with the person.
    /// </summary>
    public int OneTimeCode { get; set; }


    /// <inheritdoc />
    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger.LogWarning("Tried to process an outgoing only Link packet.");
        return null;
    }
}