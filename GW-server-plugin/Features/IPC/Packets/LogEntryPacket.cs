using GW_server_plugin.Enums;

namespace GW_server_plugin.Features.IPC.Packets;

/// <summary>
/// Packet type for the output packet for logging server data.
/// </summary>
public class LogEntryPacket: CommunicationPacket
{
    /// <summary>
    ///     The channel to log to.
    ///     Channels can be anything, it's up to the reciever to handle channels.
    /// </summary>
    public LogChannel Channel { get; set; }
    /// <summary>
    ///     Text to log.
    /// </summary>
    public string LogText { get; set; } = null!;

    /// <inheritdoc />
    public override CommunicationPacket? Process()
    {
        GwServerPlugin.Logger.LogWarning("Tried to process out-only LogEntry type packet.");
        return null;
    }
}