using GW_server_plugin.Enums;
using Mirage;

namespace GW_server_plugin.Features.IPC.Packets;

/// <summary>
/// Packet type for the output packet for logging chat messages.
/// </summary>
public class ChatLogPacket: CommunicationPacket
{
    /// <inheritdoc />
    public override PacketType Type { get; set; } = PacketType.ChatLog;

    /// <summary>
    ///     Name of the chat the message was sent in
    /// </summary>
    public string ChatName { get; set; } = null!;

    /// <summary>
    ///     Text of the chat message to log.
    /// </summary>
    public string MessageText { get; set; } = null!;

    /// <inheritdoc />
    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger.LogWarning("Tried to process out-only LogEntry type packet.");
        return null;
    }

    
}
