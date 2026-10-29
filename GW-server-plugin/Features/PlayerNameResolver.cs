using System;
using System.Collections.Generic;
using System.Linq; 
using Cysharp.Threading.Tasks;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;

namespace GW_server_plugin.Features;

/// <summary>
///     Class to resolve and cache player names using the steam API.
/// </summary>
public static class PlayerNameResolver
{
    /// <summary>
    /// Maps each connected player's SteamID to their current display name.
    /// <seealso cref="UnitRegistry.cachedPlayerNames"/>
    /// </summary>
    private static readonly Dictionary<ulong, string> ConnectedPlayerNames = []; // TODO explore native steamworks to be able to query it from UnitRegistry
    private static readonly object ConnectedPlayerNamesLock = new();
    
    
    /// <summary>
    ///     Attempt to get a steamID from a player's name.
    /// </summary>
    /// <param name="playerName">input name string</param>
    /// <param name="steamId">output value</param>
    /// <returns>true if successful</returns>
    public static bool TryGetConnectedPlayerSteamId(string playerName, out ulong? steamId)
    {
        lock (ConnectedPlayerNamesLock)
        {
            var player = ConnectedPlayerNames.FirstOrDefault(pair =>
                string.Equals(pair.Value, playerName, StringComparison.CurrentCultureIgnoreCase));
            steamId = player.Key;
            return player.Key != 0;
        }
    }
    
    /// <summary>
    ///     Attempt to get a player's name from their steamID
    /// </summary>
    /// <param name="steamId">input steamID</param>
    /// <param name="playerName">output value</param>
    /// <returns>true if success</returns>
    public static bool TryGetConnectedPlayerName(ulong steamId, out string playerName)
    {
        lock (ConnectedPlayerNamesLock)
        {
            return ConnectedPlayerNames.TryGetValue(steamId, out playerName);
        }
    }
    
    /// <summary>
    ///     Attempt to fetch a player's name, autmatically cached.
    /// </summary>
    /// <param name="player">player whose name to fetch</param>
    /// <returns>null if fail, the username otherwise.</returns>
    public static async UniTask<string?> FetchConnectedPlayerName(Player player)
    {
        var steamId = player.SteamID;
        var username = await SteamWebApi.GetUsernameAsync(steamId);
        if (string.IsNullOrWhiteSpace(username)) return null;
        
        lock (ConnectedPlayerNamesLock)
        {
            // Do not re-add a player who left while the web request was pending.
            if (UnitRegistry.playerLookupSteamId.ContainsKey(player.CSteamID))
            {
                ConnectedPlayerNames[steamId] = username!;
            }
        }
        return username;
    }
    
    /// <summary>
    ///     Remove a player from the name cache.
    /// </summary>
    /// <param name="steamId">steamID to remove for</param>
    public static void RemovePlayer(ulong steamId)
    {
        lock (ConnectedPlayerNamesLock)
        {
            ConnectedPlayerNames.Remove(steamId);
        }
    }
    
    
}