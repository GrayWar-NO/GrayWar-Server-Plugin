namespace GW_server_plugin.Enums;

/// <summary>
/// Enumerates logs channels that are possible to send through the socket
/// </summary>
public enum LogChannel
{
    /// <summary>
    /// Players joining and leaving.
    /// </summary>
    JoinLeave,
    /// <summary>
    /// Players joining a faction.
    /// </summary>
    FactionJoin,
    /// <summary>
    /// Missions starting and stopping, and general mission-related logging.
    /// </summary>
    MissionStatus,
    /// <summary>
    /// Start and end of a player's sortie.
    /// </summary>
    SortieStatus,
    /// <summary>
    /// Chat messages
    /// </summary>
    Chat,
    /// <summary>
    /// Teamkill logs
    /// </summary>
    Teamkill,
    /// <summary>
    /// Player killed someone.
    /// </summary>
    Kill,
    /// <summary>
    /// Log when a player is kicked.
    /// </summary>
    Kick,
    /// <summary>
    /// Log when a player is banned.
    /// </summary>
    Ban,
    /// <summary>
    /// Log when a player is warned.
    /// </summary>
    Warn
}