using GW_server_plugin.Enums;
using Newtonsoft.Json;

namespace GW_server_plugin.Features.IPC.Packets;

/// <summary>
/// Abstract class to reporesent a Communication Packet
/// </summary>
[JsonConverter(typeof(PacketTypeConverter))]
public abstract class CommunicationPacket
{
    public PacketType Type { get; set; }

    public abstract CommunicationPacket? Process();
}