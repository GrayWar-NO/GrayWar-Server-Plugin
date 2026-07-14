using System;
using Com.Graywar.NoServerManager.Proto;
using NuclearOption.Networking;

namespace GW_server_plugin.Helpers;

/// <summary>
/// Utilites for handling permission levels.
/// </summary>
public static class PermissionLevelUtils
{
    /// <summary>
    ///     Get a player's permission level
    /// </summary>
    /// <param name="player"> Player </param>
    /// <returns> Associated permission level </returns>
    public static PermissionLevel GetPlayerPermissionLevel(Player player)
    {
        if (PluginConfig.Owner!.Value == player.SteamID.ToString())
            return PermissionLevel.Owner;
        
        if (PluginConfig.AdminsList.Contains(player.SteamID.ToString()))
            return PermissionLevel.Admin;
        
        if (PluginConfig.ModeratorsList.Contains(player.SteamID.ToString()))
            return PermissionLevel.Moderator;
        
        return PermissionLevel.Everyone;
    }
}