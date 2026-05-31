using System.Linq;
using NuclearOption.Networking;

namespace GW_server_plugin.Features;

internal static class MissionBalanceService
{
    internal static void CheckAndApplyBalance()
    {
        var factionMinPlayers = MissionManager.CurrentMission.factions
            .Select(faction => faction.FactionHQ.GetPlayers(false).Count).Min();
        foreach (var faction in MissionManager.CurrentMission.factions)
        {
            GwServerPlugin.Logger.LogDebug($"{faction.factionName}:{faction.FactionHQ.GetPlayers(false).Count}:{factionMinPlayers + PluginConfig.MaxFactionPlayerCountDiff!.Value}:{faction.FactionHQ.NetworkpreventJoin}");
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
}