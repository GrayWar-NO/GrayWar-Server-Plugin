using System.Linq;

namespace GW_server_plugin.Features;

internal static class MissionBalanceService
{
    internal static void CheckAndApplyBalance()
    {
        var factionMinPlayers = MissionManager.CurrentMission.factions
            .Select(faction => faction.FactionHQ.GetPlayers(false).Count).Min();
        foreach (var faction in MissionManager.CurrentMission.factions)
        {
            faction.FactionHQ.NetworkpreventJoin = faction.FactionHQ.GetPlayers(false).Count >=
                                                   factionMinPlayers + PluginConfig.MaxFactionPlayerCountDiff!.Value;
        }
    }
}