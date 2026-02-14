namespace GW_server_plugin.Enums;

/// <summary>
///     Type for packets enum
/// </summary>
public enum PacketType
{
    /// <summary>
    /// Packet type for a ping packet, that needs to be replied to.
    /// </summary>
    Ping,
    /// <summary>
    /// Packet type for a command received from the peer process
    /// </summary>
    Command,
    /// <summary>
    /// Packet type for a response sent to a Command packet
    /// </summary>
    Response,
    /// <summary>
    /// Packet type for logs that are sent to the peer process.
    /// </summary>
    LogEntry
}