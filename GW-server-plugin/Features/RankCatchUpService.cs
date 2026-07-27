using BepInEx.Configuration;
using GW_server_plugin.Helpers;
using NuclearOption.Networking;
using UnityEngine;

namespace GW_server_plugin.Features;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public static class RankCatchUpService
{
    internal static ConfigEntry<bool> RankCatchUp = null!;
    public static void InitializeRankCatchUpService(ConfigFile config)
    {
        RankCatchUp = config.Bind(PluginConfig.GeneralSection, "Rank Catchup", false,
            "On late game join, player will level up their rank based on current mission time");
    }
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