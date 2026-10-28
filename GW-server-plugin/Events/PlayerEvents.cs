using System;
using NuclearOption.Networking;

namespace GW_server_plugin.Events;

/// <summary>
///     Declares player-related events.
/// </summary>
public static class PlayerEvents
{
    /// <summary>
    /// Event handler for when a player joins a faction
    /// </summary>
    public static event Action<Player, FactionHQ>? PlayerJoinedFaction;

    internal static void OnPlayerJoinFaction(Player p, FactionHQ f)
    {
        PlayerJoinedFaction?.Invoke(p, f);
    }

}