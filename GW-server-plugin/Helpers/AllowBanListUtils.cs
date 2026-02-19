using NuclearOption.DedicatedServer;
using Steamworks;

namespace GW_server_plugin.Helpers;

/// <summary>
/// Utility functions for banlists
/// </summary>
public class AllowBanListUtils
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
}