using System.Linq;
using NuclearOption.Networking;
using NuclearOption.SavedMission;

namespace GW_server_plugin.Features;

internal class MissionBalanceService
{
    private bool _isCurrentMissionPvE;
    
    internal void CheckAndApplyBalance()
    {
        if (_isCurrentMissionPvE) return;
        
        var factionMinPlayers = MissionManager.CurrentMission.factions
            .Select(faction => faction.FactionHQ.GetPlayers(false).Count).Min();
        foreach (var faction in MissionManager.CurrentMission.factions)
        {
            var count = faction.FactionHQ.GetPlayers(false).Count;
            var limit = factionMinPlayers + PluginConfig.MaxFactionPlayerCountDiff!.Value; 
            
            var shouldPreventJoin = count >= limit;
            if (faction.FactionHQ.NetworkpreventJoin != shouldPreventJoin)
            {
                faction.FactionHQ.NetworkpreventJoin = shouldPreventJoin;

                ChatService.SendChatMessageAsServer(
                    shouldPreventJoin
                        ? $"{faction.factionName} is no longer joinable for balance reasons."
                        : $"{faction.factionName} can be joined again!");
            }
            
            GwServerPlugin.Logger.LogDebug($"Can join {faction.factionName}: {!faction.FactionHQ.NetworkpreventJoin}");
        }
    }

    internal static void OnPlayerJoin(Player player)
    {
        var saveData = player.GetAuthData().SaveData;
        if (saveData == null || saveData.Faction == null) return;
        if (player.HQ == saveData.Faction) return;
        player.HQ = saveData.Faction;
        player.HQ.AddPlayer(player);
        player.HQ.RequestTrackingStates(player);
    }

    internal void OnMissionLoad(Mission mission)
    {
        var c = mission.factions.Count(fac => !fac.preventJoin);
        _isCurrentMissionPvE = c <= 1;
    }
    
}