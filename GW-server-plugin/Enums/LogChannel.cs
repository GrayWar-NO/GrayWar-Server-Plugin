namespace GW_server_plugin.Enums;

/// <summary>
/// Enumerates logs channels that are possible to send through the socket
/// </summary>
public enum LogChannel
{
    /// <summary>
    /// Info logging (mission change, performance, etc...)
    /// </summary>
    Info,
    /// <summary>
    /// Chat messages
    /// </summary>
    Chat,
    /// <summary>
    /// Teamkill logs
    /// </summary>
    Teamkill,
    /// <summary>
    /// Damage by or to player.
    /// </summary>
    Damage,
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