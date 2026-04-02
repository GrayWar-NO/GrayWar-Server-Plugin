using GW_server_plugin.Enums;
using Mirage;

namespace GW_server_plugin.Features.IPC.Packets;

/// <summary>
/// Packet type for the output packet for logging chat messages.
/// </summary>
public class ChatLogPacket: LogEntryPacket
{
    /// <inheritdoc />
    public override LogChannel Channel { get; set; } = LogChannel.Chat;

    /// <inheritdoc />
    public override PacketType Type { get; set; } = PacketType.ChatLog;
    
    /// <summary>
    /// SteamID of the sender of the message
    /// </summary>
    public ulong SteamID { get; set; }

    /// <summary>
    ///     Name of the chat the message was sent in
    /// </summary>
    public string ChatName { get; set; } = null!;
    
}
