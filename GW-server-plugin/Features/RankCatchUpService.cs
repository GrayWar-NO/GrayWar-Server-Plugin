using BepInEx.Configuration;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;
using UnityEngine;

namespace GW_server_plugin.Features;

/// <summary>
/// Ranks up player during late-game joins
/// </summary>
public static class RankCatchUpService
{
    internal static ConfigEntry<bool> RankCatchUp = null!;
    /// <summary>
    /// Initialize config properties
    /// </summary>
    /// <param name="config"></param>
    public static void Initialize(ConfigFile config)
    {
        RankCatchUp = config.Bind(PluginConfig.GeneralSection, "Rank Catchup", false,
            "On late game join, player will level up their rank based on current mission time");
    }
    /// <summary>
    /// Ranks up player
    /// </summary>
    /// <param name="player"></param>
    public static void CatchUpPlayer(Player player)
    {
        var currentMissionTime = Time.timeSinceLevelLoad;
        var maxMissionTime = Globals.DedicatedServerManagerInstance.CurrentMissionOption.MaxTime;
        var percentComplete = (currentMissionTime / maxMissionTime) * 2;
        var rank = 0;

        if (percentComplete < .20) return;
        
        if (player.GetAuthData().SaveData.Faction != null)
        {
            return; // Means that they already joined the server. No double-dipping!
        }
        if (percentComplete >= .80)
        {
            rank = 5;
        }
        else if (percentComplete >= .60) 
        {
            rank = 4;
        }
        else if (percentComplete >= .40) 
        {
            rank = 3;
        }
        else if (percentComplete >= .40) 
        {
            rank = 2;
        }
        else if (percentComplete >= .20) 
        {
            rank = 1;
        }

        if (player.PlayerRank > rank) return;
        player.SetRank(rank, false);
        ChatService.SendPrivateChatMessage($"Late join - You have been promoted to Rank {rank}! :)", player);
    }
}