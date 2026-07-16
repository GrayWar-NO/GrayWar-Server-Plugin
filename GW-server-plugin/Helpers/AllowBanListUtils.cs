using System.Collections.Generic;
using NuclearOption.DedicatedServer;
using Steamworks;

namespace GW_server_plugin.Helpers;

/// <summary>
/// Utility functions for banlists
/// </summary>
public static class AllowBanListUtils
{
    /// <summary>
    /// Unbans a player and removes his steamID from the banlist.
    /// </summary>
    /// <param name="list">The banlist to remove the player from.</param>
    /// <param name="path">Path of the banlist file to remove the player from</param>
    /// <param name="id">CSteamID of the player</param>
    public static void UnbanAndRemoveId(AllowBanList list, string path, CSteamID id)
    {
        list.Remove(id);
        AllowBanList.RemoveId(path, id);
    }
    
    /// <summary>
    ///     Replaces all the bans in an AllowBanList with the data contained in data
    /// </summary>
    /// <param name="list">allowBanList whose content to replace</param>
    /// <param name="path">Path of the file to save to</param>
    /// <param name="data">Data to replace with</param>
    public static void ReplaceWithNewData(AllowBanList list, string path, List<(CSteamID id, string reason)> data)
    {
        list.Clear();
        foreach (var (id, reason) in data)
        {
            list.Add(id, reason);
        }
        AllowBanList.Save(path, data);
    }
}