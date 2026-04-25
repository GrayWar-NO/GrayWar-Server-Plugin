using System;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using GW_server_plugin.Enums;
using GW_server_plugin.Features.IPC.Packets;
using Mirage;
using Newtonsoft.Json;
using NuclearOption.DedicatedServer;
using NuclearOption.Networking;
using Steamworks;

namespace GW_server_plugin.Helpers;

/// <summary>
///     Helper class for player-related operations.
/// </summary>
public static class PlayerUtils
{
    /// <summary>
    ///     Get the Player object from an INetworkPlayer object.
    /// </summary>
    /// <param name="networkPlayer"> The INetworkPlayer object. </param>
    /// <returns> The Player object, if available. </returns>
    public static Player? GetPlayer(this INetworkPlayer networkPlayer)
    {
        return networkPlayer.Identity?.GetComponent<Player>();
    }

    /// <summary>
    ///     Get the Player object from an INetworkPlayer object, if available.
    /// </summary>
    /// <param name="networkPlayer"> The INetworkPlayer object. </param>
    /// <param name="playerComponent"> The Player component, if available. </param>
    /// <returns> The Player object, if available. </returns>
    public static bool TryGetPlayer(this INetworkPlayer networkPlayer, out Player? playerComponent)
    {
        playerComponent = networkPlayer.GetPlayer();
        return playerComponent != null;
    }

    /// <summary>
    ///     Try to find a player by name.
    /// </summary>
    /// <param name="playerName"> The name of the player to find. </param>
    /// <param name="playerObject"> The Player object, if available. </param>
    /// <returns></returns>
    public static bool TryFindPlayer(string playerName, out Player? playerObject)
    {
        playerObject = Globals.AuthenticatedPlayers.FirstOrDefault(p => string.Equals(StripStaffPrefix(StripIdPrefix(p.GetPlayer()?.PlayerName ?? "")), StripStaffPrefix(StripIdPrefix(playerName)), StringComparison.CurrentCultureIgnoreCase))?.GetPlayer();
        if (playerObject == null && ulong.TryParse(playerName, out var playerId))
        {
            ulong? playerSteamId;
            if (playerId <= (ulong)Globals.DedicatedServerManagerInstance.Config.MaxPlayers)
            {
                GwServerPlugin.PlayerIdentifier.GetPlayerById((int)playerId, out playerSteamId);
            }
            else playerSteamId = playerId;
            TryFindPlayerBySteamId(playerSteamId ?? 0ul, out playerObject);
        }
        return playerObject != null;
    }

    /// <summary>
    ///     Tries to find a player on the server from his steamID.
    /// </summary>
    /// <param name="steamid">The steamID to search for</param>
    /// <param name="playerObject">Player that got found</param>
    /// <returns>false if no object was found</returns>
    public static bool TryFindPlayerBySteamId(ulong steamid, out Player? playerObject)
    {
        playerObject = Globals.AuthenticatedPlayers.FirstOrDefault(p => p.GetPlayer()?.SteamID == steamid)?.GetPlayer();
        return playerObject != null; 
    }
    
    /// <summary>
    ///     Utility function to strip a player name of the staff tag, if they have it.
    /// </summary>
    /// <param name="playerName"> The player name. </param>
    /// <returns>Actual playername.</returns>
    private static string StripStaffPrefix(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
            return playerName;

        var pattern = $@"^{Regex.Escape(PluginConfig.StaffPrefix!.Value)}\s*";
        var cleanName = Regex.Replace(playerName, pattern, "", RegexOptions.IgnoreCase);

        return cleanName;
    }
    
    /// <summary>
    /// Removes ID prefix from a player's name.
    /// </summary>
    /// <param name="playerName"> Original player name </param>
    /// <returns> Stripped player name </returns>
    public static string StripIdPrefix(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
            return playerName;

        const string pattern = @"^\s*\[(?:[1-9]\d?|1\d\d|20[01])\]\s*";

        return Regex.Replace(playerName, pattern, "");
    }
    
    /// <summary>
    /// Checks if a player is staff.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool IsStaff(Player player)
    {
        return !(!PluginConfig.IsAdmin(player.SteamID) &&
                !PluginConfig.IsOwner(player.SteamID) &&
                !PluginConfig.IsModerator(player.SteamID));
    }
    /// <summary>
    ///     Apply or remove the staff tag based on player permission level.
    /// </summary>
    /// <param name="playerObject"> The Player object. </param>
    /// <returns></returns>
    public static void ApplyOrRemoveStaffTag(Player playerObject)
    {
        if (!PluginConfig.UseStaffPrefix!.Value || !IsStaff(playerObject)) return;
        var newName = $"{PluginConfig.StaffPrefix!.Value} {playerObject.PlayerName}";
        playerObject.PlayerName = newName;
    }


    /// <summary>
    /// Counts the staff members in a given list of players.
    /// </summary>
    /// <returns></returns>
    public static int CountStaff()
    {
        return Globals.NetworkManagerNuclearOptionInstance.Server.AuthenticatedPlayers
            .Count(networkPlayer =>
                networkPlayer.TryGetPlayer<Player>(out var player) &&
                IsStaff(player));
    }

    /// <summary>
    /// Applies identification tag to a player.
    /// </summary>
    /// <param name="playerObject"> Player to apply the identification tag to.</param>
    /// <param name="id"> ID to apply to the player. </param>
    public static void ApplyIdentificationTag(Player playerObject, int id)
    {
        var newName = $"[{id}] {playerObject.PlayerName}";
        playerObject.PlayerName = newName; 
    }
    
    /// <summary>
    ///     Get the permission level of a player.
    /// </summary>
    /// <param name="player"> The player. </param>
    /// <returns> The permission level of the player. </returns>
    public static PermissionLevel GetPlayerPermissionLevel(Player player)
    {
        if (PluginConfig.Owner!.Value == player.SteamID.ToString())
            return PermissionLevel.Admin;
        
        if (PluginConfig.AdminsList.Contains(player.SteamID.ToString()))
            return PermissionLevel.Admin;
        
        if (PluginConfig.ModeratorsList.Contains(player.SteamID.ToString()))
            return PermissionLevel.Moderator;
        
        return PermissionLevel.Everyone;
    }

    /// <summary>
    /// Function that kicks a player.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="reason"></param>
    public static void KickPlayer(Player player, string reason)
    {
        Globals.NetworkManagerNuclearOptionInstance.KickPlayerAsync(player, reason).Forget();
        var kickLogPacket = new LogEntryPacket
        {
            LogText = $"1:{player.SteamID}:{reason}",
            Channel = LogChannel.Kick
        };
        GwServerPlugin.LoggingOutBox.Add(kickLogPacket);
    }

    /// <summary>
    /// Bans a player from a steamID.
    /// </summary>
    /// <param name="banSteamID"></param>
    /// <param name="reason"></param>
    /// <param name="duration"></param>
    public static void BanPlayer(ulong banSteamID, string reason, string? duration)
    {
        AllowBanList.BanAndAppendId(
            Globals.NetworkManagerNuclearOptionInstance.Authenticator.BanList,
            Globals.DedicatedServerManagerInstance.Config.BanListPaths[0],
            new CSteamID(banSteamID),
            reason
        ); 
        
        var banLogPacket = new LogEntryPacket
        {
            LogText = $"1:{banSteamID}:{duration ?? ""}:{reason}",
            Channel = LogChannel.Ban
        };
        GwServerPlugin.LoggingOutBox.Add(banLogPacket);
    }

    /// <summary>
    /// Kicks a player asynchronously with reason.
    /// </summary>
    /// <param name="managerNuclearOption"></param>
    /// <param name="player"></param>
    /// <param name="reason"></param>
    /// <exception cref="MethodInvocationException"></exception>
    public static async UniTaskVoid KickPlayerAsync(this NetworkManagerNuclearOption managerNuclearOption, Player player, string reason)
    {
        GwServerPlugin.Logger.LogDebug("Called new kick");
        if (!managerNuclearOption.Server.Active)
            throw new MethodInvocationException("KickPlayerAsync called when server is not active");
        var conn = player.Owner;
        managerNuclearOption.authenticator.OnKick(conn);
        var hostName = GameManager.GetLocalPlayer<Player>(out var localPlayer) ? localPlayer.PlayerName : "server";
        player.KickReason(reason, hostName);
        await UniTask.Delay(100);
        conn.Disconnect();
    }

    
}