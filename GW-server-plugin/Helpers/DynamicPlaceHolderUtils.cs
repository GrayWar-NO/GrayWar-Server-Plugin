using NuclearOption.Networking;
using GW_server_plugin.Features;

namespace GW_server_plugin.Helpers;

/// <summary>
///     Utility class for dynamic placeholders.
/// </summary>
public static class DynamicPlaceholderUtils
{
    /// <summary>
    ///     Placeholder for a player's username.
    /// </summary>
    public const string PlayerName = "{player_name}";

    /// <summary>
    ///     Placeholder for a player's username, censored.
    /// </summary>
    public const string PlayerNameCensored = "{player_name_censored}";

    /// <summary>
    ///     Placeholder for a player's Steam ID.
    /// </summary>
    public const string SteamID = "{steamid}";

    /// <summary>
    ///     Placeholder for the message broadcaster name.
    /// </summary>
    public const string ServerBroadcastName = "{server_broadcast_name}";

    /// <summary>
    ///     Replaces dynamic placeholders in a string with the appropriate values.
    /// </summary>
    /// <param name="original"> The original string. </param>
    /// <param name="player"> The player to get the values from. Ignored if null. </param>
    /// <returns> The string with the placeholders replaced. </returns>
    public static string ReplaceDynamicPlaceholders(string original, Player? player = null)
    {
        if (player)
        {
            original = original.Replace(PlayerName, player!.PlayerName);
            original = original.Replace(PlayerNameCensored, player.GetNameOrCensored());
            original = original.Replace(SteamID, player.SteamID.ToString());
        }
        
        original = original.Replace(ServerBroadcastName, PluginConfig.ServerBroadcastName!.Value);
        return original;
    }
}