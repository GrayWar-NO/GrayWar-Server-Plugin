using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using GW_server_plugin.Helpers;

namespace GW_server_plugin.Features;

/// <summary>
/// Service to manage warnings.
/// </summary>
/// <param name="config"></param>
public class WarnService(ConfigFile config)
{
    private const int DefaultWarnsToKick = 3;
    private readonly Dictionary<ulong, int> _playerWarnCount = new();

    private const bool DefaultWarnsToKickEnabled = true;
    private ConfigEntry<int> WarnsToKickConfig { get; } = config.Bind("Warns", "WarnsToKick", DefaultWarnsToKick, "Number of warnings per mission until player is kicked.");
    private ConfigEntry<bool> WarnsToKickOnConfig { get; } = config.Bind("Warns", "WarnsToKick Enabled", DefaultWarnsToKickEnabled, "Do you want to kick players after x warns?");

    /// <summary>
    /// Adds a warning to the designated player.
    /// </summary>
    /// <param name="steamID">The player's steamID</param>
    /// <returns></returns>
    public bool AddWarn(ulong steamID)
    {
        if (!_playerWarnCount.Keys.Contains(steamID))
        {
            _playerWarnCount[steamID] = 1;
            return true;
        }
        _playerWarnCount[steamID]++;
        if (_playerWarnCount[steamID] < WarnsToKickConfig.Value || !WarnsToKickOnConfig.Value) return true;
        if (!PlayerUtils.TryFindPlayerBySteamId(steamID, out var player)) return false;
        PlayerUtils.KickPlayer(player!, "Too many warnings!");
        return true;
    }

    /// <summary>
    /// Clears all existing warnings
    /// </summary>
    public void ClearWarns()
    {
        _playerWarnCount.Clear();
    }
    
    
}