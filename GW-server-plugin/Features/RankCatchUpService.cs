using System.Collections.Generic;
using System.Globalization;
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
    private static ConfigEntry<string> _rankCatchUpThresholds = null!;
    private static (int Rank, float Percent)[] _thresholds = [];
    
    /// <summary>
    /// Initialize config properties
    /// </summary>
    /// <param name="config"></param>
    public static void Initialize(ConfigFile config)
    {
        RankCatchUp = config.Bind(PluginConfig.GeneralSection, "Rank Catchup", false,
            "On late game join, player will level up their rank based on current mission time");
        
        _rankCatchUpThresholds = config.Bind(PluginConfig.GeneralSection, "Rank Catchup Thresholds",
            "1@10;2@20;3@30;4@40;5@50",
            "Rank catchup thresholds, in format of: rank@percent separated by semicolon. " +
            "Highest rank entry takes precedence in case of overlap in timer completion " +
            "(e.g. rank 2 and rank 3 both given at 10% means players get rank 3 at 10%).");
        
        ParseThresholds();
    }
    
    /// <summary>
    /// Ranks up player
    /// </summary>
    /// <param name="player"></param>
    public static void CatchUpPlayer(Player player)
    {
        if (!RankCatchUp.Value || player.GetAuthData().SaveData.Faction != null) return;
        
        var currentMissionTime = Time.timeSinceLevelLoad;
        var maxMissionTime = Globals.DedicatedServerManagerInstance.CurrentMissionOption.MaxTime;
        var percentComplete = currentMissionTime / maxMissionTime * 100f;
        var rank = player.PlayerRank;
        
        foreach (var threshold in _thresholds)
            if (percentComplete >= threshold.Percent && threshold.Rank > rank)
                rank = threshold.Rank;
        
        if (player.PlayerRank >= rank) return;
        player.SetRank(rank, false);
        ChatService.SendPrivateChatMessage($"Late join - You have been promoted to Rank {rank}! :)", player);
    }
    
    private static void ParseThresholds()
    {
        var parsed = new List<(int Rank, float Percent)>();
        foreach (var entry in _rankCatchUpThresholds.Value.Split(';'))
        {
            var cleanedEntry = entry.Trim();
            if (cleanedEntry.Length == 0) continue;
            var parts = cleanedEntry.Split('@');
            
            if (parts.Length != 2
                || !int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var rank)
                || rank < 0
                || !float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var pct)
                || float.IsNaN(pct) || pct < 0f || pct > 100f)
            {
                GwServerPlugin.Logger.LogWarning($"Invalid rank threshold '{cleanedEntry}', expected rank@percent.");
                continue;
            }
            
            parsed.Add((rank, pct));
        }
        
        _thresholds = [.. parsed];
    }
}