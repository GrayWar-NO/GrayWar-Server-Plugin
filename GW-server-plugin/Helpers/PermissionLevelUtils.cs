using Com.Graywar.NoServerManager.Proto;
using NuclearOption.Networking;

namespace GW_server_plugin.Helpers;

/// <summary>
/// Utilites for handling permission levels.
/// </summary>
public static class PermissionLevelUtils
{
    /// <summary>
    ///     Transforms a proto-defined permission level to an internal shockfront permission level.
    /// </summary>
    /// <param name="permissionLevel"></param>
    /// <returns></returns>
    public static NuclearOption.ModdingApi.PermissionLevel ProtoToShockfrontPermissionLevel(
        PermissionLevel permissionLevel)
    {
        switch (permissionLevel)
        {
            case PermissionLevel.Owner:
                return (NuclearOption.ModdingApi.PermissionLevel)0xFE;
            case PermissionLevel.Admin:
                return NuclearOption.ModdingApi.PermissionLevel.Admin;
            case PermissionLevel.Moderator:
                return NuclearOption.ModdingApi.PermissionLevel.Moderator;
            case PermissionLevel.Everyone:
            default:
                return NuclearOption.ModdingApi.PermissionLevel.Player;
        }
    }

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