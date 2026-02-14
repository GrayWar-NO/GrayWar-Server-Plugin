using System;
using GW_server_plugin.Enums;
using GW_server_plugin.Features;
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

        /// <summary>
    ///     Converts a string to a <see cref="PermissionLevel" />.
    /// </summary>
    /// <param name="permissionLevel">The string to convert.</param>
    /// <returns>The converted <see cref="PermissionLevel" />.</returns>
    public static PermissionLevel StringToPermissionLevel(string permissionLevel)
    {
        return permissionLevel.ToLower() switch
        {
            "everyone" => PermissionLevel.Everyone,
            "moderator" => PermissionLevel.Moderator,
            "admin" => PermissionLevel.Admin,
            "owner" => PermissionLevel.Owner,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    // integer to PermissionLevel
    /// <summary>
    ///     Converts an integer to a <see cref="PermissionLevel" />.
    /// </summary>
    /// <param name="permissionLevel">The integer to convert.</param>
    /// <returns>The converted <see cref="PermissionLevel" />.</returns>
    public static PermissionLevel IntToPermissionLevel(int permissionLevel)
    {
        return permissionLevel switch
        {
            0 => PermissionLevel.Everyone,
            1 => PermissionLevel.Moderator,
            2 => PermissionLevel.Admin,
            _ => PermissionLevel.Everyone
        };
    }

    /// <summary>
    ///     Tries to parse a string to a <see cref="PermissionLevel" />.
    /// </summary>
    /// <param name="permissionLevel"> The string to parse. </param>
    /// <param name="result"> The parsed <see cref="PermissionLevel" />. </param>
    /// <returns> Whether the parsing was successful. </returns>
    public static bool TryParsePermissionLevel(string permissionLevel, out PermissionLevel result)
    {
        if (int.TryParse(permissionLevel, out var intResult))
        {
            result = IntToPermissionLevel(intResult);
            return true;
        }

        try
        {
            result = StringToPermissionLevel(permissionLevel);
            return true;
        }
        catch
        {
            result = PermissionLevel.Everyone;
            return false;
        }
    }
}
